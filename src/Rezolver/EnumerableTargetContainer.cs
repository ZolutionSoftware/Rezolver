// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    internal class EnumerableTargetContainer : GenericTargetContainer
    {
        ITargetContainer _parent;

        public EnumerableTargetContainer(ITargetContainer parent) : base(typeof(IEnumerable<>))
        {
            _parent = parent;
        }

        protected virtual ITarget CreateListTarget(Type elementType, IEnumerable<ITarget> targets, bool asArray = false)
        {
            if (targets.All(t => t is ICompiledTarget))
                return PrecompiledListTarget.ForTargets(elementType, targets, asArray);
            else
                return new ListTarget(elementType, targets, asArray);
        }

        public override ITarget Fetch(Type type)
        {
            if (!TypeHelpers.IsGenericType(type))
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
            Type genericType = type.GetGenericTypeDefinition();
            if (genericType != typeof(IEnumerable<>))
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));

            //we allow for specific IEnumerable<T> registrations
            var result = base.Fetch(type);

            if (result != null)
                return result;

            var enumerableType = TypeHelpers.GetGenericArguments(type)[0];

            var targets = _parent.FetchAll(enumerableType);

            //the method below has a shortcut for an enumerable of targets which are all ICompiledTarget
            //this enables containers to bypass compilation for an IEnumerable when all the underlying
            //targets are already able to return their objects (added to support expression compiler).
            return CreateListTarget(enumerableType, targets, true);
        }

        public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            //caters for the situation where our extension method EnableEnumerableResolving() is called more than once.
            if (existing is EnumerableTargetContainer) return existing;
            return base.CombineWith(existing, type);
        }
    }
}
