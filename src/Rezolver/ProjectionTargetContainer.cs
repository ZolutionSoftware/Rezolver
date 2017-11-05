using Rezolver.Targets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public IRootTargetContainer Root { get; }
        public Type SourceElementType { get; }
        public Type OutputElementType { get; }
        private Func<IRootTargetContainer, ITarget, ITarget> ImplementationTargetFactory { get; }
        public Type SourceEnumerableType { get; }
        public Type OutputEnumerableType { get; }

        private readonly ConcurrentDictionary<Type, EnumerableTarget> _cache = new ConcurrentDictionary<Type, EnumerableTarget>();

        public ProjectionTargetContainer(
            IRootTargetContainer root,
            Type sourceElementType,
            Type outputElementType,
            Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
            : this(root, sourceElementType, outputElementType)
        {
            ImplementationTargetFactory = implementationTargetFactory ?? throw new ArgumentNullException(nameof(implementationTargetFactory));
        }

        private ProjectionTargetContainer(IRootTargetContainer root, Type sourceElementType, Type outputElementType)
        {
            Root = root;
            SourceElementType = sourceElementType;
            OutputElementType = outputElementType;
            SourceEnumerableType = typeof(IEnumerable<>).MakeGenericType(sourceElementType);
            OutputEnumerableType = typeof(IEnumerable<>).MakeGenericType(outputElementType);

            Root.AddKnownType(OutputEnumerableType);
        }

        public ITarget Fetch(Type type)
        {
            return _cache.GetOrAdd(type, t =>
            {
                if (!TypeHelpers.IsGenericType(type))
                    throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType != typeof(IEnumerable<>))
                    throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));

                var input = Root.Fetch(SourceEnumerableType);

                if (!(input is IEnumerable<ITarget> targets))
                    throw new InvalidOperationException($"Projection of { OutputEnumerableType } requires { SourceEnumerableType } to result in an IEnumerable<ITarget> result from the root target container.  Cannot build projection.");

                return new EnumerableTarget(
                    targets.Select(tgt => new ProjectionTarget(
                        tgt, 
                        ImplementationTargetFactory(Root, tgt), 
                        SourceElementType, 
                        OutputElementType)), 
                    OutputElementType);
            });
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serviceType"></param>
        public void Register(ITarget target, Type serviceType = null)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<ITarget> FetchAll(Type type)
        {
            return new[] { Fetch(type) };
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
            throw new NotSupportedException();
        }
    }
}
