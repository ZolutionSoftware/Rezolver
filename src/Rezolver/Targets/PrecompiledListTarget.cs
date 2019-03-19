// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Targets
{
    using CompiledListFactory = Func<IEnumerable<ITarget>, bool, ITarget>;

    /// <summary>
    /// Special variant of ListTarget which is used when auto-resolving an enumerable of objects where all
    /// the resolved targets are all ICompiledTarget implementations.  This type also implements ICompiledTarget,
    /// which, in its implementation of GetObject, returns a fully materialised array or list of the objects
    /// returned by each target's GetObject call - bypassing the need to compile each target first, or compiling
    /// the list first.
    /// </summary>
    /// <typeparam name="TElement">The type of the t element.</typeparam>
    /// <seealso cref="Rezolver.Targets.ListTarget" />
    /// <seealso cref="Rezolver.ICompiledTarget" />
    internal class PrecompiledListTarget<TElement> : ListTarget, IResolvable
    {
        private readonly IEnumerable<ICompiledTarget> _compiledTargets;

        public ITarget SourceTarget => this;

        public PrecompiledListTarget(IEnumerable<ITarget> targets, bool asArray = false)
                : base(typeof(TElement), targets, asArray)
        {
            this._compiledTargets = targets.Cast<ICompiledTarget>();
        }

        public object GetObject(ResolveContext context)
        {
            var elements = this._compiledTargets.Select(i => (TElement)i.GetObject(context.ChangeRequestedType(serviceType: typeof(TElement))));
            return AsArray ? (object)elements.ToArray() : new List<TElement>(elements);
        }
    }

    internal class PrecompiledListTarget
    {
        private static readonly ConcurrentDictionary<Type, Lazy<CompiledListFactory>> _precompiledListTargetFactories
                    = new ConcurrentDictionary<Type, Lazy<CompiledListFactory>>();

        internal static ITarget ForTargets(Type elementType, IEnumerable<ITarget> targets, bool asArray)
        {
            // get or build (and add) a dynamic binding to the generic CompiledTargetListTarget constructor
            return _precompiledListTargetFactories.GetOrAdd(elementType, t =>
            {
                return new Lazy<CompiledListFactory>(() =>
                {
                    var listTargetType = typeof(PrecompiledListTarget<>).MakeGenericType(t);
                    var targetsParam = Expression.Parameter(typeof(IEnumerable<ITarget>), "targets");
                    var asArrayParam = Expression.Parameter(typeof(bool), "asArray");

                    return Expression.Lambda<CompiledListFactory>(
                        Expression.Convert(
                            Expression.New(listTargetType.GetConstructor(new[] { typeof(IEnumerable<ITarget>), typeof(bool) }), targetsParam, asArrayParam),
                            typeof(ITarget)
                        ),
                        targetsParam,
                        asArrayParam
                    )
                .CompileForRezolver();
                });
            }).Value(targets, asArray);
        }
    }
}
