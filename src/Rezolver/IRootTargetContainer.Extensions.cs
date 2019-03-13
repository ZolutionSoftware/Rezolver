// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    public partial class RootTargetContainerExtensions
    {
        /// <summary>
        /// Calls <see cref="IRootTargetContainer.GetContainerRegistrationType(Type)"/> to get the correct registration type for the target container 
        /// that should own a target that will be registered against the given <paramref name="serviceType"/>.  Once obtained, that type is then
        /// passed to the <see cref="IRootTargetContainer.CreateTargetContainer(Type)"/> to create the correct <see cref="ITargetContainer"/> for
        /// that container registration type.
        /// 
        /// This is typically used by 'advanced' target containers which wrap others under potentially any type and which therefore need to reuse
        /// the root container's logic for ensuring that registrations are handled correctly.
        /// </summary>
        /// <param name="rootContainer"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static ITargetContainer CreateTargetContainerForServiceType(this IRootTargetContainer rootContainer, Type serviceType)
        {
            if (rootContainer == null)
                throw new ArgumentNullException(nameof(rootContainer));
            return CreateTargetContainerForServiceTypeInternal(
                rootContainer,
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)));
        }

        internal static ITargetContainer CreateTargetContainerForServiceTypeInternal(this IRootTargetContainer rootContainer, Type serviceType)
        {
            return rootContainer.CreateTargetContainer(
                            rootContainer.GetContainerRegistrationType(serviceType));
        }
    }
}
