// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Runtime;

namespace Rezolver
{
    internal static class ChildTargetContainerExtensions
    {
        internal static Type GetChildContainerType(this ITargetContainer targets, Type serviceType)
        {
            var attr = TypeHelpers.GetCustomAttributes<ContainerTypeAttribute>(serviceType, true).FirstOrDefault();
            if (attr != null)
            {
                return attr.Type;
            }

            return targets.Root.GetOption<ITargetContainerTypeResolver>(serviceType)?.GetContainerType(serviceType);
        }

        internal static ITargetContainer CreateChildContainer(this ITargetContainer targets, Type targetContainerType)
        {
            return targets.Root.GetOption<ITargetContainerFactory>(targetContainerType)?.CreateContainer(targetContainerType, targets);
        }
    }
}
