using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal static class ChildTargetContainerExtensions
    {
        internal static bool ShouldUseGenericTypeDef(this ITargetContainer targets, Type serviceType)
        {
            return TypeHelpers.IsGenericType(serviceType) && !TypeHelpers.IsGenericTypeDefinition(serviceType);
        }

        internal static Type GetChildContainerType(this ITargetContainer targets, Type serviceType, ITargetContainer root = null)
        {
            if (root == null) root = targets;

            if (ShouldUseGenericTypeDef(targets, serviceType))
                return serviceType.GetGenericTypeDefinition();

            return root.GetOption<ITargetContainerTypeResolver>(serviceType)?.GetContainerType(serviceType);
        }
        
        internal static ITargetContainer CreateChildContainer(this ITargetContainer targets, Type targetContainerType, ITargetContainer root = null)
        {
            if (root == null) root = targets;

            if (TypeHelpers.IsGenericTypeDefinition(targetContainerType))
                return new GenericTargetContainer(root, targetContainerType);

            return root.GetOption<ITargetContainerFactory>(targetContainerType)?.CreateContainer(targetContainerType, targets, root);
        }
    }
}
