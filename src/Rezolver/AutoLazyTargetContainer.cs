﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Concurrent;

namespace Rezolver
{
    internal class AutoLazyTargetContainer : GenericTargetContainer
    {
        private ConcurrentDictionary<Type, ITarget> _cachedTargets = new ConcurrentDictionary<Type, ITarget>();
        internal AutoLazyTargetContainer(IRootTargetContainer root)
            : base(root, typeof(Lazy<>))
        {

        }

        public override ITarget Fetch(Type type)
        {
            Type genericType;
            if (!type.IsGenericType || (genericType = type.GetGenericTypeDefinition()) != GenericType)
            {
                throw new ArgumentException($"Only {GenericType} is supported by this container", nameof(type));
            }

            // just like EnumerableTargetContainer, we allow for specific registrations
            var result = base.Fetch(type);

            if (result != null)
                return result;


            return _cachedTargets.GetOrAdd(type, BuildTarget);

            ITarget BuildTarget(Type t)
            {
                // we need a ConstructorTarget that binds to the correct Lazy<T> constructor (for the specific type 
                // passed as the generic type argument)
                // for that, we need the generic type argument
                var innerLazyType = t.GetGenericArguments()[0];
                var constructor = t.GetConstructor(new[] { typeof(Func<>).MakeGenericType(innerLazyType) });

                if (constructor == null)
                    throw new InvalidOperationException($"Could not get Func callback constructor for {t}");

                return new ConstructorTarget(constructor).Unscoped();
            }
        }
    }
}
