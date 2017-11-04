using Rezolver.Targets;
using System;
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
        //private Func<ITarget, Type> ImplementationTypeSelector { get; }
        public Type SourceEnumerableType { get; }
        public Type OutputEnumerableType { get; }

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
            if (!TypeHelpers.IsGenericType(type))
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
            Type genericType = type.GetGenericTypeDefinition();
            if (genericType != typeof(IEnumerable<>))
                throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));

            var input = Root.Fetch(SourceEnumerableType);

            if (!(input is IEnumerable<ITarget> targets))
                throw new InvalidOperationException($"Projection of { OutputEnumerableType } requires { SourceEnumerableType } to result in an IEnumerable<ITarget> result from the root target container.  Cannot build projection.");

            return new EnumerableTarget(targets.Select(t => new ProjectionTarget(t, ImplementationTargetFactory(Root, t), SourceElementType)), OutputElementType);
        }

        public void Register(ITarget target, Type serviceType = null)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<ITarget> FetchAll(Type type)
        {
            return new[] { Fetch(type) };
        }

        public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            throw new NotSupportedException();
        }

        public ITargetContainer FetchContainer(Type type)
        {
            throw new NotSupportedException();
        }

        public void RegisterContainer(Type type, ITargetContainer container)
        {
            throw new NotSupportedException();
        }
    }
}
