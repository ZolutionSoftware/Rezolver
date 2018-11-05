// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Configuration (enabled by default in the <see cref="TargetContainer.DefaultConfig"/> configuration collection)
    /// which enables the automatic injection of arrays by converting automatically injected enumerables into array
    /// instances via the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> method from Linq.
    /// </summary>
    /// <remarks>Note that this configuration requires that the <see cref="InjectEnumerables"/> configuration is
    /// also applied.</remarks>
    public class InjectArrays : OptionDependentConfig<Options.EnableArrayInjection>
    {
        /// <summary>
        /// The one and only instance of the <see cref="InjectArrays"/> configuration object
        /// </summary>
        public static InjectArrays Instance { get; } = new InjectArrays();

        internal class ArrayTypeResolver : ITargetContainerTypeResolver
        {
            public static ArrayTypeResolver Instance { get; } = new ArrayTypeResolver();

            private ArrayTypeResolver() { }

            public Type GetContainerType(Type serviceType)
            {
                if (TypeHelpers.IsArray(serviceType))
                {
                    return typeof(Array);
                }

                return null;
            }
        }

        private readonly IEnumerable<DependencyMetadata> _dependencies;

        /// <summary>
        /// Overrides the <see cref="OptionDependentConfig{TOption}"/> implementation to include a required dependency
        /// on the <see cref="InjectEnumerables"/> configuration.
        /// </summary>
        public override IEnumerable<DependencyMetadata> Dependencies => this._dependencies;

        private InjectArrays() : base(false)
        {
            this._dependencies = base.Dependencies.Concat(new[] { this.CreateTypeDependency<InjectEnumerables>(true) }).ToArray();
        }

        /// <summary>
        /// Implements the <see cref="OptionDependentConfig{TOption}.Configure(IRootTargetContainer)"/> abstract method
        /// by configuring the passed <paramref name="targets"/> so it can produce targets for any array type, regardless
        /// of whether a single object has been registered for the array's element type.
        ///
        /// After enabling, the ability to register specific targets for concrete array types will still be present.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            // REVIEW: should this also check that EnableEnumerableInjection is true?
            // At the moment, it's just dependent upon the Enumerable config; but it actually needs it to
            // be *enabled* too.

            if (!targets.GetOption(Options.EnableArrayInjection.Default))
            {
                return;
            }

            targets.RegisterContainer(typeof(Array), new ArrayTargetContainer(targets));
            targets.SetOption<ITargetContainerTypeResolver, Array>(ArrayTypeResolver.Instance);
        }
    }
}
