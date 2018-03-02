// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Runtime;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Implements projections of IEnumerables from one type to another
    ///
    /// Effectively, this daisy chains one enumerable into another via something
    /// similar to a Linq Select.
    /// </summary>
    public class ProjectionTargetContainer : ITargetContainer
    {
        /// <summary>
        /// The ultimate root target container to which this target container belongs.
        /// </summary>
        public IRootTargetContainer Root { get; }
        /// <summary>
        /// Element type of the <see cref="IEnumerable{T}"/> whose elements will be projected into a
        /// new enumerable type.  Note - this is always equal to the type argument that's present in the
        /// <see cref="SourceEnumerableType"/>.
        /// </summary>
        public Type SourceElementType { get; }
        /// <summary>
        /// Element type of the <see cref="IEnumerable{T}"/> that is to be projected from the source
        /// enumerable.
        /// </summary>
        public Type OutputElementType { get; }

        private Func<IRootTargetContainer, ITarget, TargetProjection> TargetProjectionFactory { get; }

        /// <summary>
        /// The concrete type of the <see cref="IEnumerable{T}"/> instance that will feed the projection
        /// to an instance of the <see cref="OutputEnumerableType"/>.
        ///
        /// The single type argument is available through the <see cref="SourceElementType"/> property.
        /// </summary>
        public Type SourceEnumerableType { get; }

        /// <summary>
        /// The concrete type of the <see cref="IEnumerable{T}"/> that's projected from the source enumerable.
        ///
        /// The single type argument is available through the <see cref="OutputElementType"/> property.
        /// </summary>
        public Type OutputEnumerableType { get; }

        private readonly ConcurrentDictionary<Type, EnumerableTarget> _cache = new ConcurrentDictionary<Type, EnumerableTarget>();

        internal ProjectionTargetContainer(
            IRootTargetContainer root,
            Type sourceElementType,
            Type outputElementType,
            Func<IRootTargetContainer, ITarget, TargetProjection> targetProjectionFactory)
            : this(root, sourceElementType, outputElementType)
        {
            this.TargetProjectionFactory = targetProjectionFactory ?? throw new ArgumentNullException(nameof(targetProjectionFactory));
        }

        private ProjectionTargetContainer(IRootTargetContainer root, Type sourceElementType, Type outputElementType)
        {
            this.Root = root;
            this.SourceElementType = sourceElementType;
            this.OutputElementType = outputElementType;
            this.SourceEnumerableType = typeof(IEnumerable<>).MakeGenericType(sourceElementType);
            this.OutputEnumerableType = typeof(IEnumerable<>).MakeGenericType(outputElementType);

            this.Root.AddKnownType(this.OutputEnumerableType);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Fetch(Type)"/> - always produces an <see cref="EnumerableTarget"/>
        /// which, when compiled, will produce an enumerable of <see cref="OutputEnumerableType"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITarget Fetch(Type type)
        {
            return this._cache.GetOrAdd(type, t =>
            {
                if (type != this.OutputEnumerableType)
                {
                    throw new ArgumentException($"This projection container only supports the type {this.OutputEnumerableType}", nameof(type));
                }

                var input = this.Root.Fetch(this.SourceEnumerableType);

                if (!(input is IEnumerable<ITarget> targets))
                {
                    throw new InvalidOperationException($"Projection of {this.OutputEnumerableType} requires {this.SourceEnumerableType} to result in an IEnumerable<ITarget> result from the root target container.  Cannot build projection.");
                }

                return new EnumerableTarget(
                    targets.Select(tgt => new { input = tgt, projection = this.TargetProjectionFactory(this.Root, tgt) })
                    .Select(tp => new ProjectionTarget(
                        tp.input,
                        this.SourceElementType,
                        this.OutputElementType,
                        tp.projection)),
                    this.OutputElementType);
            });
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serviceType"></param>
        public void Register(ITarget target, Type serviceType = null)
        {
            throw new NotSupportedException("A projection cannot be overriden");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<ITarget> FetchAll(Type type)
        {
            return new[] { this.Fetch(type) };
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITargetContainer FetchContainer(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        public void RegisterContainer(Type type, ITargetContainer container)
        {
            throw new NotSupportedException("A projection cannot register other containers");
        }
    }
}
