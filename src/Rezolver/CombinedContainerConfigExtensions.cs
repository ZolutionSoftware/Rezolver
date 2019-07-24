// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using Rezolver.Configuration;

namespace Rezolver
{
    /// <summary>
    /// Contains extension methods for the <see cref="CombinedContainerConfig"/> class which simplify the process of
    /// adding and replacing <see cref="IContainerConfig"/> objects which set options and register well-known container services.
    /// </summary>
    public static class CombinedContainerConfigExtensions
    {
        /// <summary>
        /// Replaces any existing <see cref="IContainerConfig{ITargetCompiler}" /> in the collection with the
        /// <see cref="ExpressionCompilation.Instance"/> from <see cref="ExpressionCompilation"/> so that
        /// any container to which the config collection will be applied will use the <see cref="ExpressionCompiler"/>.
        /// </summary>
        /// <param name="combined">The collection to which the behaviour is to be added.</param>
        /// <returns>The collection on which the operation is called, to allow chaining of further calls.</returns>
        public static CombinedContainerConfig UseExpressionCompiler(this CombinedContainerConfig combined)
        {
            return combined.UseCompiler(ExpressionCompilation.Instance);
        }

        /// <summary>
        /// Replaces any existing <see cref="IContainerConfig{ITargetCompiler}"/> with the passed <paramref name="configuration"/> -
        /// thus ensuring that any <see cref="Container"/> objects which are initialised with the config collection will use whichever
        /// compiler that is configured when the configuration's <see cref="IContainerConfig.Configure(Container, IRootTargetContainer)"/> method
        /// is called.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static CombinedContainerConfig UseCompiler(this CombinedContainerConfig collection, IContainerConfig<ITargetCompiler> configuration)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            collection.ReplaceAnyOrAdd<IContainerConfig<ITargetCompiler>>(configuration);
            return collection;
        }
    }
}
