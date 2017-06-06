using Rezolver.Behaviours;
using Rezolver.Compilation;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Contains the global behaviours for <see cref="ContainerBase"/>-derived containers (that's all
    /// containers in Rezolver), the special <see cref="OverridingContainer"/>, and the <see cref="TargetContainer"/>
    /// classes.
    /// </summary>
    /// <remarks>
    /// Customisation and extension of the behaviour of <see cref="ITargetContainer"/> and <see cref="IContainer"/> types 
    /// is performed by instances of the <see cref="ITargetContainerBehaviour"/> and <see cref="IContainerBehaviour"/> interfaces.
    /// Implementations are executed when these objects are created and they will typically register services which are later 
    /// resolved by the container types.
    /// 
    /// For example, compilation is a key container-related behaviour which is configured via the 
    /// <see cref="IContainerBehaviour"/> interface; which is expected to register the <see cref="ITargetCompiler"/>
    /// for the container so that these objects can then be used to convert registered targets
    /// (of the type <see cref="ITarget"/>) into compiled targets (of the type <see cref="ICompiledTarget"/>).
    /// </remarks>
    public static class GlobalBehaviours
    {
        private static ContainerBehaviourCollection _containerBehaviour = new ContainerBehaviourCollection();
        private static ContainerBehaviourCollection _overridingContainerBehaviour = new ContainerBehaviourCollection();
        private static TargetContainerBehaviourCollection _targetContainerBehaviour = new TargetContainerBehaviourCollection();

        /// <summary>
        /// Gets/sets the default behaviour to attach to all the <see cref="ContainerBase"/> 
        /// classes (all except the <see cref="OverridingContainer"/>)
        /// </summary>
        /// <remarks>
        /// You can either add/remove/replace items in this collection, 
        /// or you can replace it.  When setting the property, attempting to set it to <c>null</c>
        /// will cause an <see cref="ArgumentNullException"/> to be thrown.
        /// 
        /// Caution: The collection is only thread-safe for multiple readers.
        /// </remarks>
        public static ContainerBehaviourCollection ContainerBehaviour {
            get { return _containerBehaviour; }
            set
            {
                _containerBehaviour = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets/sets the default behaviour to attach to all <see cref="OverridingContainer"/> instances.
        /// </summary>
        /// <remarks>
        /// You can either add/remove/replace items in this collection, 
        /// or you can replace it.  When setting the property, attempting to set it to <c>null</c>
        /// will cause an <see cref="ArgumentNullException"/> to be thrown.
        /// 
        /// Caution: The collection is only thread-safe for multiple readers.
        /// </remarks>
        public static ContainerBehaviourCollection OverridingContainerBehaviour
        {
            get
            {
                return _overridingContainerBehaviour;
            }
            set
            {
                _overridingContainerBehaviour = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the default behaviour to attached to all <see cref="TargetContainer"/> instances.
        /// 
        /// When a target container is created, it executes the <see cref="ITargetContainerBehaviour.Attach(ITargetContainer)"/>
        /// method of the <see cref="ITargetContainerBehaviour"/> passed to it.  If none is passed, then this instance's
        /// method is called instead.
        /// </summary>
        /// <remarks>
        /// You can either add/remove/replace items in this collection, 
        /// or you can replace it.  When setting the property, attempting to set it to <c>null</c>
        /// will cause an <see cref="ArgumentNullException"/> to be thrown.
        /// 
        /// Caution: The collection is only thread-safe for multiple readers.
        /// </remarks>
        public static TargetContainerBehaviourCollection TargetContainerBehaviour
        {
            get
            {
                return _targetContainerBehaviour;
            }
            set
            {
                _targetContainerBehaviour = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        static GlobalBehaviours()
        {
            // automatic IEnumerable<> resolving is enabled by default
            TargetContainerBehaviour.AddAll(AutoEnumerableBehaviour.Instance,
                ContextResolvingBehaviour.Instance);

            // expression compiler is used by default,
            // and members of classes are not bound by default.
            ContainerBehaviour.UseExpressionCompiler();

            OverridingContainerBehaviour.Add(OverridingEnumerableBehaviour.Instance);
        }
    }
}
