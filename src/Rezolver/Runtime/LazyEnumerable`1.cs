// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
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
    public class LazyEnumerable<T> : IEnumerable<T>
    {
        private readonly ResolveContext _context;
        private readonly ICompiledTarget[] _factories;

        /// <summary>
        /// Creates a new <see cref="LazyEnumerable{T}"/> instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="factories"></param>
        public LazyEnumerable(ResolveContext context, ICompiledTarget[] factories)
        {
            this._context = context.ChangeRequestedType(typeof(T));
            this._factories = factories;
        }

        private IEnumerable<T> GetInstances()
        {
            foreach(var factory in _factories)
            {
                yield return (T)factory.GetObject(_context);
            }
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return GetInstances().GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
