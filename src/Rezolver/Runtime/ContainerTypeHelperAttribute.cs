using System;

namespace Rezolver.Runtime
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface| AttributeTargets.Struct)]
    internal abstract class ContainerTypeHelperAttribute : Attribute, ITargetContainerTypeResolver, ITargetContainerFactory
    {
        public abstract ITargetContainer CreateContainer(Type type, ITargetContainer targets, ITargetContainer rootTargetContainer);

        public abstract Type GetContainerType(Type serviceType);
    }
}