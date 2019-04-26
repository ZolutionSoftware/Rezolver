// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Collections.Generic;
using Rezolver.Sdk;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Abstract base class for <see cref="ITargetContainerConfig"/> implementations which are dependent upon
    /// one or more configuration objects having been applied before being applied themselves.
    /// </summary>
    /// <remarks>Implement this type directly if your config object has dependencies on multiple config types/objects.
    /// 
    /// Your implementation of <see cref="GetDependenciesBase"/> should return metadata which describes those
    /// dependencies.  The easiest way to create these is through one of:
    /// 
    /// - <see cref="CreateOptionDependency{TOption}(bool)"/>
    /// - <see cref="DependantExtensions.CreateTypeDependency{TDependency}(IDependant, bool)"/>
    /// - <see cref="DependantExtensions.CreateObjectDependency{TDependency}(IDependant, TDependency, bool)"/></remarks>
    public abstract class OptionDependentConfigBase : ITargetContainerConfig, IDependant
    {
        private IEnumerable<DependencyMetadata> _dependencies;

        /// <summary>
        /// Gets the dependencies for this in instance.  Implementation of <see cref="IDependant.Dependencies"/>
        /// </summary>
        public IEnumerable<DependencyMetadata> Dependencies
        {
            get
            {
                if (_dependencies == null)
                    return _dependencies = GetDependenciesBase();
                return _dependencies;
            }
        }

        /// <summary>
        /// Implement this to provide your dependency logic.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<DependencyMetadata> GetDependenciesBase();

        /// <summary>
        /// Abstract implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/>
        /// </summary>
        /// <param name="targets">The root target container to which this configuration is to be applied.</param>
        public abstract void Configure(IRootTargetContainer targets);

        /// <summary>
        /// Use this to create a <see cref="TypeDependency"/> on an option of type <typeparamref name="TOption"/>
        /// being set with the <see cref="Configure{TOption}"/> <see cref="ITargetContainerConfig"/> configuration.
        /// 
        /// This enables your config to take a dependency on an option being set before being applied, thus enabling
        /// it to take decisions based on that option value.
        /// </summary>
        /// <typeparam name="TOption"></typeparam>
        /// <param name="required"></param>
        /// <returns></returns>
        protected DependencyMetadata CreateOptionDependency<TOption>(bool required = true)
            where TOption : class
        {
            return this.CreateTypeDependency<Configure<TOption>>(required);
        }
    }
}
