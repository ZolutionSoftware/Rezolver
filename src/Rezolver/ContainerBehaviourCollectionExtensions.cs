using Rezolver.Behaviours;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// Contains extension methods for the <see cref="ContainerBehaviourCollection"/> class which simplify the process of 
    /// adding and replacing behaviours which manage well-known container services.
    /// </summary>
    public static class ContainerBehaviourCollectionExtensions
    {
        /// <summary>
        /// Sets the default member binding behaviour to be used by containers to which the 
        /// <paramref name="collection"/> is attached.
        /// </summary>
        /// <param name="collection">The collection to which the behaviour is to be added.</param>
        /// <param name="behaviour">The <see cref="IMemberBindingBehaviour"/> to be used as the default for any
        /// container to which the <paramref name="collection"/> is attached.</param>
        /// <returns>The collection on which the operation is called, to allow chaining of further calls.</returns>
        /// <remarks>This extension method replaces any existing <see cref="IContainerBehaviour{IMemberBindingBehaviour}"/> 
        /// present in the collection with a new one created for the passed <paramref name="behaviour"/></remarks>
        public static ContainerBehaviourCollection UseMemberBindingBehaviour(this ContainerBehaviourCollection collection, IMemberBindingBehaviour behaviour)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));

            collection.ReplaceAnyOrAdd<IContainerBehaviour<IMemberBindingBehaviour>>(
                new DefaultMemberBindingBehaviour(behaviour));
            return collection;
        }

        /// <summary>
        /// Replaces any existing <see cref="IContainerBehaviour{ITargetCompiler}" /> in the collection with the 
        /// <see cref="ExpressionCompilerBehaviour.Instance"/> from <see cref="ExpressionCompilerBehaviour"/> so that
        /// any container which uses this behaviour collection will use the <see cref="ExpressionCompiler"/>.
        /// </summary>
        /// <param name="collection">The collection to which the behaviour is to be added.</param>
        /// <returns>The collection on which the operation is called, to allow chaining of further calls.</returns>
        public static ContainerBehaviourCollection UseExpressionCompiler(this ContainerBehaviourCollection collection)
        {
            return collection.UseCompiler(ExpressionCompilerBehaviour.Instance);
        }

        /// <summary>
        /// Configures the container
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="compilerBehaviour"></param>
        /// <returns></returns>
        public static ContainerBehaviourCollection UseCompiler(this ContainerBehaviourCollection collection, IContainerBehaviour<ITargetCompiler> compilerBehaviour)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (compilerBehaviour == null) throw new ArgumentNullException(nameof(compilerBehaviour));

            collection.ReplaceAnyOrAdd<IContainerBehaviour<ITargetCompiler>>(compilerBehaviour);
            return collection;
        }
    }
}
