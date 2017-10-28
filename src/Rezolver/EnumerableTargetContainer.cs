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
            _tracker = new TargetOrderTracker();
            Root.TargetRegistered += Root_TargetRegistered;
        }

        private void Root_TargetRegistered(object sender, TargetRegisteredEventArgs e)
        {
            _tracker.Track(e.Target);
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

            // we need to find all the targets which have types which compatible with elementType
            // for that we use the ICovariantTypeIndex implementation of the Root target container.
            return new EnumerableTarget(new[] { elementType }
                .Concat(Root.GetKnownCompatibleTypes(elementType))
                .SelectMany(t => Root.FetchAll(t))
                .Distinct(TargetIdentityComparer.Instance) //don't duplicate targets
                .OrderBy(t => _tracker.GetOrder(t) ?? int.MaxValue), elementType);
        }

        public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (existing is EnumerableTargetContainer) return existing;
            return base.CombineWith(existing, type);
        }
    }
}
