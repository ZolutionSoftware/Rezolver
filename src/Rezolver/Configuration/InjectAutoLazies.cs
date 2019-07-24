// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Options;
using Rezolver.Sdk;
using System;
using System.Collections.Generic;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Configuration which, if the <see cref="EnableAutoLazyInjection"/>, option is
    /// configured to <c>true</c>, will auto-register a target container for <see cref="Lazy{T}"/>
    /// that will, in turn, automatically enable the injection of lazy instances - rather than
    /// requiring explicit registration.
    /// </summary>
    /// <remarks>Note - successful injection relies on the lazy being able to resolve a <see cref="Func{TResult}"/>
    /// which will be used as the instance factory.  Typically, if you're enabling automatic lazy injection,
    /// then you'll also have enabled automatic <see cref="Func{TResult}"/> injection via the 
    /// <see cref="InjectAutoFuncs"/> configuration and its associated option: <see cref="EnableAutoFuncInjection"/>.</remarks>
    public class InjectAutoLazies : OptionDependentConfigBase
    {
        /// <summary>
        /// The one and only instance of this configuration.
        /// </summary>
        public static InjectAutoLazies Instance { get; } = new InjectAutoLazies();

        private InjectAutoLazies() { }

        /// <summary>
        /// Adds the necessary registration to the passed root target container
        /// for <see cref="Lazy{T}"/> so long as no <see cref="ITargetContainer"/> is already
        /// registered.
        /// </summary>
        /// <param name="targets">The root target container to which this configuation will be applied.</param>
        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption<EnableAutoLazyInjection>())
                return;

            if (targets.FetchContainer(typeof(Lazy<>)) == null)
                targets.RegisterContainer(typeof(Lazy<>), new AutoLazyTargetContainer(targets));
        }

        /// <summary>
        /// Implementation of the <see cref="OptionDependentConfigBase.GetDependenciesBase"/>
        /// method which returns optional dependencies on the <see cref="EnableAutoLazyInjection"/>
        /// and <see cref="EnableAutoFuncInjection"/> options.
        /// </summary>
        /// <returns>The dependencies for this configuration.</returns>
        protected override IEnumerable<DependencyMetadata> GetDependenciesBase()
        {
            return new[]
            {
                CreateOptionDependency<EnableAutoLazyInjection>(false),
                CreateOptionDependency<EnableAutoFuncInjection>(false)
            };
        }
    }
}
