// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Runtime
{
    /// <summary>
    /// Can be used as an implementation of <see cref="IEnumerable{T}"/> when injecting enumerables from
    /// one or more registered targets.
    /// </summary>
    /// <typeparam name="T">The type of object enumerated by the enumerable.</typeparam>
    /// <remarks>Objects are not expected to take a dependency on this type - but <see cref="IEnumerable{T}"/>.  The
    /// default compiler, <see cref="Compilation.Expressions.ExpressionCompiler"/>, constructs an instance of this
    /// when building an <see cref="Targets.EnumerableTarget"/> (via the <see cref="Compilation.Expressions.EnumerableTargetBuilder"/>
    /// expression builder).
    ///
    /// ## Lazy vs Eager enumerables
    ///
    /// The default enumerable produced by Rezolver containers is lazily evaluated - via this type.  You can disable this entirely
    /// (i.e. switching to eagerly loaded enumerables implemented via the <see cref="EagerEnumerable{T}"/> type)
    /// by setting the <see cref="Options.LazyEnumerables"/> option to <c>false</c> in the underlying <see cref="ITargetContainer"/>
    /// used by the container.
    ///
    /// You can also switch to eagerly loaded enumerables on a per-type basis - by setting the <see cref="Options.LazyEnumerables"/>
    /// option to <c>false</c> against the specific type of enumerable you want to be eagerly loaded - e.g. for an eagerly loaded <see cref="IEnumerable{T}"/> of
    /// type <see cref="string"/>, then you would set the <see cref="Options.LazyEnumerables"/> to <c>false</c> for the type <c>IEnumerable&lt;string&gt;</c>
    /// </remarks>
    /// <seealso cref="EagerEnumerable{T}"/>
    public sealed class LazyEnumerable<T> //: IEnumerable<T>
    {
        private readonly Func<ResolveContext, T>[] _factories;

        /// <summary>
        /// Creates a new <see cref="LazyEnumerable{T}"/> instance.
        /// </summary>
        /// <param name="factories"></param>
        public LazyEnumerable(Delegate[] factories)
        {
            this._factories = factories.OfType<Func<ResolveContext, T>>().ToArray();
        }

        /// <summary>
        /// The method that is used to produce instances by the container.
        /// </summary>
        /// <param name="context">The current context</param>
        /// <returns>An enumerable which produces each instance as it is enumerated.</returns>
        public IEnumerable<T> GetInstances(ResolveContext context)
        {
            foreach(var factory in _factories)
            {
                yield return factory(context);
            }
        }
    }
}
