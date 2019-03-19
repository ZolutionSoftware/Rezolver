// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
    internal class ExpressionBuilderCache
    {
        private readonly ConcurrentDictionary<Type, IExpressionBuilder> _cache
             = new ConcurrentDictionary<Type, IExpressionBuilder>();

        private readonly Container _container;

        public ExpressionBuilderCache(Container container)
        {
            this._container = container;
        }

        public IExpressionBuilder ResolveBuilder(ITarget target)
        {
            if (this._cache.TryGetValue(target.GetType(), out IExpressionBuilder builder))
            {
                return builder;
            }

            return this._cache.GetOrAdd(target.GetType(), t => 
            {
                List<Type> builderTypes =
                    TargetSearchTypes(t)
                    .Distinct()
                    .Select(tt => typeof(IExpressionBuilder<>).MakeGenericType(tt)).ToList();

                // and add the IExpressionBuilder type
                builderTypes.Add(typeof(IExpressionBuilder));

                foreach (var type in builderTypes)
                {
                    // TODO: investigate whether the TargetSearchTypes method is needed any more, because it was written before Rezolver supported contravariance.
                    foreach (IExpressionBuilder expressionBuilder in (IEnumerable)this._container.Resolve(typeof(IEnumerable<>).MakeGenericType(type)))
                    {
                        if (expressionBuilder != this)
                        {
                            if (expressionBuilder.CanBuild(t))
                            {
                                return expressionBuilder;
                            }
                        }
                    }
                }

                return null;
            });
        }

        private static IEnumerable<Type> TargetSearchTypes(Type targetType)
        {
            // the search list is:
            // 1) the target's type
            // 2) each base (in descending order of inheritance) of the target which is compatible with ITarget
            // So a target of type {MyTarget<T> : TargetBase} will yield the list
            // [ MyTarget<T>, TargetBase ]
            // Whereas a target of type MyTarget<T> : ITarget yields the list
            // [ MyTarget<T> ]
            // because it has no bases which implement ITarget
            yield return targetType;
            // return all bases which can be treated as ITarget
            foreach (var baseT in targetType.GetAllBases().Where(t => typeof(ITarget).IsAssignableFrom(t)))
            {
                yield return baseT;
            }

            if (typeof(IInstanceProvider).IsAssignableFrom(targetType))
            {
                yield return typeof(IInstanceProvider);
            }

            if(typeof(IFactoryProvider).IsAssignableFrom(targetType))
            {
                yield return typeof(IFactoryProvider);
            }
        }
    }
}
