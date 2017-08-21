using System;

namespace Rezolver.Runtime
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface| AttributeTargets.Struct)]
    internal class GenericContainerTypeHelperAttribute : ContainerTypeHelperAttribute
    {
        public override Type GetContainerType(Type serviceType)
        {
            if (TypeHelpers.IsGenericType(serviceType))
                return !TypeHelpers.IsGenericTypeDefinition(serviceType) ? serviceType.GetGenericTypeDefinition() : serviceType;

            throw new ArgumentException("Fatal error: internal attribute GenericContainerTypeHelperAttribute applied to non-generic type", nameof(serviceType));
        }

        public override ITargetContainer CreateContainer(Type type, ITargetContainer targets, ITargetContainer rootTargetContainer)
        {
            return new GenericTargetContainer(rootTargetContainer, type);
        }
    }
}