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
        private readonly ConcurrentDictionary<Type, Lazy<IExpressionBuilder>> _cache
             = new ConcurrentDictionary<Type, Lazy<IExpressionBuilder>>();

        private readonly IContainer _container;

        public ExpressionBuilderCache(IContainer container)
        {
            this._container = container;
        }

        public IExpressionBuilder ResolveBuilder(ITarget target)
        {
            if (this._cache.TryGetValue(target.GetType(), out Lazy<IExpressionBuilder> builder))
            {
                return builder.Value;
            }

            return this._cache.GetOrAdd(target.GetType(), t => new Lazy<IExpressionBuilder>(() =>
            {
                List<Type> builderTypes =
                    TargetSearchTypes(t)
                    .Distinct()
                    .Select(tt => typeof(IExpressionBuilder<>).MakeGenericType(tt)).ToList();

                // and add the IExpressionBuilder type
                builderTypes.Add(typeof(IExpressionBuilder));

                foreach (var type in builderTypes)
                {
                    foreach (IExpressionBuilder expressionBuilder in (IEnumerable)this._container.Resolve(typeof(IEnumerable<>).MakeGenericType(type)))
                    {
                        if (expressionBuilder != this)
                        {
                            if (expressionBuilder.CanBuild(target))
                            {
                                return expressionBuilder;
                            }
                        }
                    }
                }

                return null;
            })).Value;
        }

        private IEnumerable<Type> TargetSearchTypes(Type targetType)
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

            if (typeof(ICompiledTarget).IsAssignableFrom(targetType))
            {
                yield return typeof(ICompiledTarget);
            }
        }
    }
}
