using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    public class ContainerBehaviourCollection : DependantCollection<IContainerBehaviour>, IContainerBehaviour
    {
        public ContainerBehaviourCollection()
        {

        }

        public ContainerBehaviourCollection(IEnumerable<IContainerBehaviour> behaviours)
            : base(behaviours)
        {

        }

        public void Configure(IContainer container, ITargetContainer targets)
        {
            foreach(var behaviour in OrderByDependency())
            {
                behaviour.Configure(container, targets);
            }
        }

        public IEnumerable<IContainerBehaviour> GetDependencies(IEnumerable<IContainerBehaviour> behaviours)
        {
            return Enumerable.Empty<IContainerBehaviour>();
        }
    }
}
