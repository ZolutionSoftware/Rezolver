// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using Rezolver.Events;
using Rezolver.Runtime;

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
                root.TargetRegistered += Root_TargetRegistered1;
            }

            private void Root_TargetRegistered1(object sender, TargetRegisteredEventArgs e)
            {
                Track(e.Target);
            }

            public int? GetOrder(ITarget target)
            {
                if (_dictionary.TryGetValue(target, out var order))
                    return order;
                return null;
            }

            public int Track(ITarget target)
            {
                if (!_dictionary.TryGetValue(target, out var result))
                    _dictionary[target] = result = ++_counter;
                return result;
            }
        }

        private readonly TargetOrderTracker _tracker;

        public EnumerableTargetContainer(IRootTargetContainer root)
            : base(root, typeof(IEnumerable<>))
        {
            _tracker = root.GetOption<TargetOrderTracker>();
            if (_tracker == null)
            {
                // this is the first enumerable container in the root
                // so create the tracker and register our own event handler
                // for adding enumerable types.
                root.SetOption(_tracker = new TargetOrderTracker(Root));
                Root.TargetRegistered += Root_TargetRegistered;
            }
        }

        private void Root_TargetRegistered(object sender, TargetRegisteredEventArgs e)
        {
            // this container enables the production of IEnumerables of any type for which 
            // targets are registered (and indeed empty enumerables for those which aren't).  
            // So, every time a target is registered we make sure to its IEnumerable variant
            // as a known type.
            Root.AddKnownType(typeof(IEnumerable<>).MakeGenericType(e.Type));
        }

        public override ITarget Fetch(Type type)
        {
            if (!TypeHelpers.IsGenericType(type))
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
            Type genericType = type.GetGenericTypeDefinition();
            if (genericType != typeof(IEnumerable<>))
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));

            // we allow for specific IEnumerable<T> registrations
            // note that this will also allow the whole enumerable behaviour to be 
            // superseded by an explicit registration for IEnumerable<>
            var result = base.Fetch(type);

            if (result != null)
                return result;

            // TODO: (Bit of a hack, this - need a better solution)
            // TODO: Solution might be simply to have a subclassed EnumerableTargetContainer that is registered when
            // the target container is an OverridingTargetContainer.

            // if the root is an OverridingTargetContainer, then 
            if (Root is OverridingTargetContainer overridingContainer)
            {
                result = overridingContainer.Parent.Fetch(type);
                // if the root result is an enumerable target; then we won't use it, because
                // the one we will return below will be *more* correct than that one (because
                // it will contain all the individual targets registered for the element type, 
                // but not include those which are registered directly in the overriding container.
                // This is to allow IEnumerable Decorators from overriden containers to be used when no
                // specific IEnumerable registration exists in an overriding container.
                if (!(result is EnumerableTarget) && !(result?.UseFallback ?? true))
                    return result;
            }

            var elementType = TypeHelpers.GetGenericArguments(type)[0];

            bool enableCovariance = Root.GetOption(elementType, Options.EnableEnumerableCovariance.Default);

            if (enableCovariance)
            {
                // we need to find all the targets which have types which compatible with elementType
                // for that we use the ICovariantTypeIndex implementation of the Root target container.
                return new EnumerableTarget(new[] { elementType }
                    .Concat(Root.GetKnownCompatibleTypes(elementType))
                    .SelectMany(t => Root.FetchAll(t))
                    .Distinct(TargetIdentityComparer.Instance) //don't duplicate targets
                    .OrderBy(t => _tracker.GetOrder(t) ?? int.MaxValue), elementType);
            }
            else
            {
                return new EnumerableTarget(Root.FetchAll(elementType)
                    .Distinct(TargetIdentityComparer.Instance) //don't duplicate targets
                    .OrderBy(t => _tracker.GetOrder(t) ?? int.MaxValue), elementType);
            }
        }

        public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (existing is EnumerableTargetContainer) return existing;
            return base.CombineWith(existing, type);
        }
    }
}
