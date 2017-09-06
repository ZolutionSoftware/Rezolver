using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal class DefaultTargetContainerFactory : ITargetContainerFactory
    {
        public static DefaultTargetContainerFactory Instance { get; } = new DefaultTargetContainerFactory();

        private DefaultTargetContainerFactory()
        {

        }

        public ITargetContainer CreateContainer(Type type, ITargetContainer targets, ITargetContainer rootTargetContainer)
        {
            return new TargetListContainer(rootTargetContainer, type);
        }
    }
}
