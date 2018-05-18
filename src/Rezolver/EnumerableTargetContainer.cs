// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using Rezolver.Events;
using Rezolver.Runtime;
using Rezolver.Targets;

namespace Rezolver
{
    internal class EnumerableTargetContainer : GenericTargetContainer
    {
        private class TargetOrderTracker
        {
            private Dictionary<ITarget, int> _dictionary = new Dictionary<ITarget, int>(ReferenceComparer<ITarget>.Instance);
            private int _counter = 0;

            public TargetOrderTracker(IRootTargetContainer root)
            {
                root.TargetRegistered += this.Root_TargetRegistered1;
            }

            private void Root_TargetRegistered1(object sender, TargetRegisteredEventArgs e)
            {
                this.Track(e.Target);
            }

            public int? GetOrder(ITarget target)
            {
                if (this._dictionary.TryGetValue(target, out var order))
                {
                    return order;
                }

                return null;
            }

            public int Track(ITarget target)
            {
                if (!this._dictionary.TryGetValue(target, out var result))
                {
                    this._dictionary[target] = result = ++this._counter;
                }

                return result;
            }
        }

        private readonly TargetOrderTracker _tracker;

        public EnumerableTargetContainer(IRootTargetContainer root)
            : base(root, typeof(IEnumerable<>))
        {
            this._tracker = root.GetOption<TargetOrderTracker>();
            if (this._tracker == null)
            {
                // this is the first enumerable container in the root
                // so create the tracker and register our own event handler
                // for adding enumerable types.
                root.SetOption(this._tracker = new TargetOrderTracker(this.Root));
                this.Root.TargetRegistered += this.Root_TargetRegistered;
            }
        }

        private void Root_TargetRegistered(object sender, TargetRegisteredEventArgs e)
        {
            // this container enables the production of IEnumerables of any type for which
            // targets are registered (and indeed empty enumerables for those which aren't).
            // So, every time a target is registered we make sure to its IEnumerable variant
            // as a known type.
            this.Root.AddKnownType(typeof(IEnumerable<>).MakeGenericType(e.Type));
        }

        private class CovariantMatch
        {
            public Type RequestedType { get; set; }
            public ITarget Target { get; set; }
            public static IEqualityComparer<CovariantMatch> Comparer => TargetIDComparer.Instance;
            private class TargetIDComparer : IEqualityComparer<CovariantMatch>
            {
                public static TargetIDComparer Instance { get; } = new TargetIDComparer();
                private TargetIDComparer() { }

                public bool Equals(CovariantMatch x, CovariantMatch y) => TargetIdentityComparer.Instance.Equals(x.Target, y.Target);

                public int GetHashCode(CovariantMatch obj) => TargetIdentityComparer.Instance.GetHashCode(obj.Target);
            }
        }

        public override ITarget Fetch(Type type)
        {
            if (!TypeHelpers.IsGenericType(type))
            {
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
            }

            Type genericType = type.GetGenericTypeDefinition();
            if (genericType != typeof(IEnumerable<>))
            {
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
            }

            // we allow for specific IEnumerable<T> registrations
            // note that this will also allow the whole enumerable behaviour to be
            // superseded by an explicit registration for IEnumerable<>
            var result = base.Fetch(type);

            if (result != null)
            {
                return result;
            }

            // TODO: (Bit of a hack, this - need a better solution)
            // TODO: Solution might be simply to have a subclassed EnumerableTargetContainer that is registered when
            // the target container is an OverridingTargetContainer.

            // if the root is an OverridingTargetContainer, then
            if (this.Root is OverridingTargetContainer overridingContainer)
            {
                result = overridingContainer.Parent.Fetch(type);
                // if the root result is an enumerable target; then we won't use it, because
                // the one we will return below will be *more* correct than that one (because
                // it will contain all the individual targets registered for the element type,
                // but not include those which are registered directly in the overriding container.
                // This is to allow IEnumerable Decorators from overriden containers to be used when no
                // specific IEnumerable registration exists in an overriding container.
                if (!(result is EnumerableTarget) && !(result?.UseFallback ?? true))
                {
                    return result;
                }
            }

            var elementType = TypeHelpers.GetGenericArguments(type)[0];

            bool enableCovariance = this.Root.GetOption(elementType, Options.EnableEnumerableCovariance.Default);

            if (enableCovariance)
            {
                return new EnumerableTarget(Root.FetchAll(elementType)
                    .Concat(Root.GetKnownCompatibleTypes(elementType)
                        .SelectMany(compatibleType => Root.FetchAll(compatibleType)
                            .Select(t => VariantMatchTarget.Wrap(t, elementType, compatibleType))
                        )).Distinct(TargetIdentityComparer.Instance)
                        .OrderBy(t => (t is VariantMatchTarget variant ? _tracker.GetOrder(variant.Target) : _tracker.GetOrder(t)) ?? int.MaxValue), elementType);
            }
            else
            {
                return new EnumerableTarget(this.Root.FetchAll(elementType)
                    .Distinct(TargetIdentityComparer.Instance) // don't duplicate targets
                    .OrderBy(t => this._tracker.GetOrder(t) ?? int.MaxValue), elementType);
            }
        }

        public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (existing is EnumerableTargetContainer)
            {
                return existing;
            }

            return base.CombineWith(existing, type);
        }
    }
}
