using Rezolver.Compilation;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Contains the default configurations for <see cref="ContainerBase"/>-derived containers (that's all
    /// containers in Rezolver), the special <see cref="OverridingContainer"/>, and the <see cref="TargetContainer"/>
    /// classes.
    /// </summary>
    /// <remarks>
    /// Configuration of <see cref="ITargetContainer"/> and <see cref="IContainer"/> types is performed by instances of the
    /// <see cref="ITargetContainerConfiguration"/> and <see cref="IContainerConfiguration"/> interfaces.  Implementations
    /// are executed when these objects are created and they will typically register services which are later resolved by them
    /// in order to customise behaviour.
    /// 
    /// For example, compilation is a key container-related behaviour which is configured via the 
    /// <see cref="IContainerConfiguration"/> interface; which is expected to register the types <see cref="ITargetCompiler"/>
    /// and <see cref="ICompileContextProvider"/> so that these objects can then be used to convert registered targets
    /// (of the type <see cref="ITarget"/>) into compiled targets (of the type <see cref="ICompiledTarget"/>).
    /// </remarks>
    public static class DefaultConfiguration
    {
        private static ContainerConfigurationCollection _containerConfig = new ContainerConfigurationCollection();
        private static ContainerConfigurationCollection _overridingContainerConfig = new ContainerConfigurationCollection();
        private static TargetContainerConfigurationCollection _targetContainerConfig = new TargetContainerConfigurationCollection();
        private static TargetContainerConfigurationCollection _childTargetContainerConfig = new TargetContainerConfigurationCollection();
        /// <summary>
        /// Gets/sets the default configuration to be used to configure the <see cref="ContainerBase"/> 
        /// classes (all except the <see cref="OverridingContainer"/>)
        /// </summary>
        /// <remarks>
        /// To change the default configuration, you can either change the contents of this collection, 
        /// or you can replace it.  When setting the property, attempting to set it to <c>null</c>
        /// will cause an <see cref="ArgumentNullException"/> to be thrown.
        /// </remarks>
        public static ContainerConfigurationCollection ContainerConfig {
            get { return _containerConfig; }
            set
            {
                _containerConfig = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets/sets the default configuration to be used to configure any <see cref="TargetContainer"/> object.
        /// </summary>
        /// <remarks>
        /// To change the default configuration, you can either change the contents of this collection, 
        /// or you can replace it.  When setting the property, attempting to set it to <c>null</c>
        /// will cause an <see cref="ArgumentNullException"/> to be thrown.</remarks>
        public static ContainerConfigurationCollection OverridingContainerConfig
        {
            get
            {
                return _overridingContainerConfig;
            }
            set
            {
                _overridingContainerConfig = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the default configuration to be applied to all new <see cref="TargetContainer"/> instances
        /// (except the <see cref="ChildTargetContainer"/>).
        /// </summary>
        /// <remarks>If you simply need to add configuration objects to all target containers, then you 
        /// can add directly into the collection; or you can set it to a new instance.</remarks>
        public static TargetContainerConfigurationCollection TargetContainerConfig
        {
            get
            {
                return _targetContainerConfig;
            }
            set
            {
                _targetContainerConfig = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// The default 
        /// </summary>
        public static TargetContainerConfigurationCollection ChildTargetContainerConfig
        {
            get
            {
                return _childTargetContainerConfig;
            }
            set
            {
                _childTargetContainerConfig = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        static DefaultConfiguration()
        {
            // automatic IEnumerable<> resolving is enabled by default
            TargetContainerConfig.AddAll(AutoEnumerableConfiguration.Instance);

            // expressino compiler is used by default
            ContainerConfig.AddAll(Compilation.Expressions.ExpressionCompilerConfiguration.Instance);
        }
    }
}
