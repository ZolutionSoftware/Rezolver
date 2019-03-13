// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Runtime;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    public static partial class RootTargetContainerExtensions
    {
        /// <summary>
        /// Fetches all targets from the <paramref name="rootContainer"/> registered against service types which are
        /// either equal to or compatible with the given <paramref name="serviceType"/>.  
        /// </summary>
        /// <param name="rootContainer">The root target container whose targets are to be searched</param>
        /// <param name="serviceType">The service type for which targets are sought.</param>
        /// <returns>An enumerable of targets which match the <paramref name="serviceType"/>.
        /// If no targets match, then the enumerable will be empty.</returns>
        /// <remarks>
        /// This is effectively a covariant version of the <see cref="ITargetContainer.FetchAll(Type)"/> function,
        /// and is used by built-in functionality such as automatic enumerable injection (when <see cref="Options.EnableEnumerableCovariance"/>
        /// is <c>true</c>) and automatic factory injection <see cref="Func{TResult}"/>.
        /// 
        /// Note: the targets are de-duped before being returned, so if one target is registered multiple times against different
        /// compatible service types, it will still only appear once in the output enumerable.</remarks>
        public static IEnumerable<ITarget> FetchAllCompatibleTargets(this IRootTargetContainer rootContainer, Type serviceType)
        {
            return FetchAllCompatibleTargetsInternal(rootContainer ?? throw new ArgumentNullException(nameof(rootContainer)),
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)));
        }

        internal static IEnumerable<ITarget> FetchAllCompatibleTargetsInternal(this IRootTargetContainer rootContainer, Type serviceType)
        {
            return rootContainer.FetchAll(serviceType)
                    .Concat(rootContainer.GetKnownCompatibleTypes(serviceType)
                        .SelectMany(compatibleType => rootContainer.FetchAll(compatibleType)
                            .Select(t => VariantMatchTarget.Wrap(t, serviceType, compatibleType))
                        )).Distinct(TargetIdentityComparer.Instance);
        }
    }
}
