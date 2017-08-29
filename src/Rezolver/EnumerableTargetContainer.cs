// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;

namespace Rezolver
{
    internal class EnumerableTargetContainer : GenericTargetContainer
    {
        public EnumerableTargetContainer(ITargetContainer root) 
            : base(root, typeof(IEnumerable<>))
        {

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
            if(Root is OverridingTargetContainer overridingContainer)
            {
                result = overridingContainer.Parent.Fetch(type);
                // if the root result is an enumerable target; then we won't use it, because
                // the one we will return below will be *more* correct than that one (because
                // it will contain all the individual targets registered for the element type, 
                // but not include those which are registered directly in the overriding container.
                if (!(result is EnumerableTarget) && !(result?.UseFallback ?? true))
                    return result;
            }

            var elementType = TypeHelpers.GetGenericArguments(type)[0];

            return new EnumerableTarget(Root.FetchAll(elementType), elementType);
        }

        public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (existing is EnumerableTargetContainer) return existing;
            return base.CombineWith(existing, type);
        }
    }
}
