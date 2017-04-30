using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainerConfiguration"/> which contains zero or or more other <see cref="IContainerConfiguration"/>
    /// objects.
    /// </summary>
    /// <remarks><see cref="IContainer"/> objects are configured by registering
    /// known services in the <see cref="ITargetContainer"/> that they're built from.  Configurations can also be
    /// interdependent - i.e. config A requires config B in order to work - hence
    /// the use of the <see cref="IDependant"/> interface on the <see cref="IContainerConfiguration"/> interface.
    /// 
    /// This collection implements <see cref="IContainerConfiguration.Configure(IContainer, ITargetContainer)"/> 
    /// by running through all the behaviours that have been added to it, in order of least to most dependant.</remarks>
    public class ContainerConfigurationCollection : DependantCollection<IContainerConfiguration>, IContainerConfiguration
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerConfigurationCollection"/> type
        /// </summary>
        public ContainerConfigurationCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerConfigurationCollection"/> type, using the passed configurations
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The configurations to be added to the collection on construction.</param>
        public ContainerConfigurationCollection(IEnumerable<IContainerConfiguration> configs)
            : base(configs)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerConfigurationCollection"/> type, using the passed configurations
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The configurations to be added to the collection on construction.</param>
        public ContainerConfigurationCollection(params IContainerConfiguration[] configs)
            : this((IEnumerable<IContainerConfiguration>)configs)
        { 

        }

        /// <summary>
        /// Runs through each configuration that has been added to the collection, in dependency order, calling its
        /// <see cref="IContainerConfiguration.Configure(IContainer, ITargetContainer)"/> method.
        /// </summary>
        /// <param name="container">The container being configured</param>
        /// <param name="targets">The target container used by the <paramref name="container"/> for its registrations.</param>
        public void Configure(IContainer container, ITargetContainer targets)
        {
            foreach(var behaviour in Ordered)
            {
                behaviour.Configure(container, targets);
            }
        }
    }
}
