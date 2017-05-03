using Rezolver.Behaviours;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    public static class ContainerBehaviourCollectionExtensions
    {
        /// <summary>
        /// Sets the default member binding behaviour to be used by containers to which the 
        /// <paramref name="collectionBehaviour"/> is attached.
        /// </summary>
        /// <param name="collectionBehaviour">The collection to which the behaviour is to be added.</param>
        /// <param name="behaviour">The <see cref="IMemberBindingBehaviour"/> to be used as the default for any
        /// container to which the <paramref name="collectionBehaviour"/> is attached.</param>
        /// <returns>The collection on which the operation is called, to allow chaining of further calls.</returns>
        /// <remarks>This extension method replaces any existing <see cref="IContainerBehaviour{IMemberBindingBehaviour}"/> 
        /// present in the collection with a new one created for the passed <paramref name="behaviour"/></remarks>
        public static ContainerBehaviourCollection UseMemberBindingBehaviour(this ContainerBehaviourCollection collectionBehaviour, IMemberBindingBehaviour behaviour)
        {
            if (collectionBehaviour == null) throw new ArgumentNullException(nameof(collectionBehaviour));
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));

            collectionBehaviour.ReplaceAnyOrAdd<IContainerBehaviour<IMemberBindingBehaviour>>(
                new DefaultMemberBindingBehaviour(behaviour));
            return collectionBehaviour;
        }

        /// <summary>
        /// Configures the container to use the <see cref="ExpressionCompiler"/> for its <see cref="ITargetCompiler"/>
        /// service.  Also performs all additional registrations required by the <see cref="ExpressionCompiler"/>.
        /// </summary>
        /// <param name="collectionBehaviour">The collection to which the behaviour is to be added.</param>
        /// <returns>The collection on which the operation is called, to allow chaining of further calls.</returns>
        public static ContainerBehaviourCollection UseExpressionCompiler(this ContainerBehaviourCollection collectionBehaviour)
        {
            if (collectionBehaviour == null) throw new ArgumentNullException(nameof(collectionBehaviour));

            collectionBehaviour.ReplaceAnyOrAdd<IContainerBehaviour<ITargetCompiler>>(ExpressionCompilerBehaviour.Instance);
            return collectionBehaviour;
        }
    }
}
