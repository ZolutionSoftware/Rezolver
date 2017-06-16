using Rezolver.Configuration;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// Contains extension methods for the <see cref="ContainerConfigCollection"/> class which simplify the process of 
    /// adding and replacing behaviours which manage well-known container services.
    /// </summary>
    public static class ContainerConfigCollectionExtensions
    {
        /// <summary>
        /// Replaces any existing <see cref="IContainerConfig{ITargetCompiler}" /> in the collection with the 
        /// <see cref="ExpressionCompilerBehaviour.Instance"/> from <see cref="ExpressionCompilerBehaviour"/> so that
        /// any container which uses this behaviour collection will use the <see cref="ExpressionCompiler"/>.
        /// </summary>
        /// <param name="collection">The collection to which the behaviour is to be added.</param>
        /// <returns>The collection on which the operation is called, to allow chaining of further calls.</returns>
        public static ContainerConfigCollection UseExpressionCompiler(this ContainerConfigCollection collection)
        {
            return collection.UseCompiler(ExpressionCompilerBehaviour.Instance);
        }

        /// <summary>
        /// Configures the container
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="compilerBehaviour"></param>
        /// <returns></returns>
        public static ContainerConfigCollection UseCompiler(this ContainerConfigCollection collection, IContainerConfig<ITargetCompiler> compilerBehaviour)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (compilerBehaviour == null) throw new ArgumentNullException(nameof(compilerBehaviour));

            collection.ReplaceAnyOrAdd<IContainerConfig<ITargetCompiler>>(compilerBehaviour);
            return collection;
        }
    }
}
