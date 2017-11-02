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
    /// 
    /// Note - the implementation requires that the target for the source enumerable is returned
    /// from the <see cref="Root"/> as an <see cref="Targets.EnumerableTarget"/>.
    /// </summary>
    public class ProjectionTargetContainer
    {
        public ITargetContainer Inner { get; private set; }
        public IRootTargetContainer Root { get; }
        public Type SourceEnumerableType { get; }
        public Type OutputEnumerableType { get; }
        private Func<ITarget, ITarget> TargetProjection { get; }
        public ProjectionTargetContainer(IRootTargetContainer root, Type sourceEnumerableType, Type outputEnumerableType, Func<ITarget, ITarget> targetProjection)
        {
            Root = root;
            SourceEnumerableType = sourceEnumerableType;
            TargetProjection = targetProjection;
        }

        private ITargetContainer EnsureInner()
        {

        }

        public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (Inner != null)
                throw new InvalidOperationException("Projection already has an inner target container");
            Inner = existing;
        }

        public ITarget Fetch(Type type)
        {
            return Inner.Fetch(type);
        }

        public IEnumerable<ITarget> FetchAll(Type type)
        {
            return Inner.FetchAll(type);
        }

        public ITargetContainer FetchContainer(Type type)
        {
            return Inner.FetchContainer(type);
        }

        public void Register(ITarget target, Type serviceType = null)
        {
            Inner.Register(target, serviceType);
        }

        public void RegisterContainer(Type type, ITargetContainer container)
        {
            Inner.RegisterContainer(type, container);
        }
    }
}
