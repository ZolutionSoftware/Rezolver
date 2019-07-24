using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Specification
{
    public abstract partial class CompilerTestsBase
    {
		/// <summary>
		/// Gets the output helper supplied by the XUnit test runner.
		/// </summary>
		/// <value>The output.</value>
		protected ITestOutputHelper Output { get; }
		protected CompilerTestsBase(ITestOutputHelper output)
		{
			Output = output;
		}

		/// <summary>
		/// Gets the behaviour to be used to configure the compiler in the container
        /// 
        /// 
		/// </summary>
		/// <param name="testName">Name of the test.</param>
		protected abstract IContainerConfig<ITargetCompiler> GetCompilerConfig([CallerMemberName]string testName = null);

        protected IContainerConfig GetContainerConfig([CallerMemberName]string testName = null)
        {
            // clone our default configuration for containers.
            var behaviour = new CombinedContainerConfig(Container.DefaultConfig);
            behaviour.UseCompiler(GetCompilerConfig(testName));
            return behaviour;
        }

		/// <summary>
		/// Creates the target container for the test.
		/// </summary>
		/// <param name="configOverride">An explicit <see cref="ITargetContainerConfig"/> to use to configure the 
        /// <see cref="ITargetContainer"/>.  If not provided, then the config returned by <see cref="GetDefaultTargetContainerConfig(string)"/> 
        /// is used, which is always a clone of the <see cref="TargetContainer.DefaultConfig"/>.</param>
        /// <param name="testName">Name of the test.</param>
		protected virtual IRootTargetContainer CreateTargetContainer(ITargetContainerConfig configOverride = null, [CallerMemberName]string testName = null)
        {
			return new TargetContainer(configOverride ?? GetDefaultTargetContainerConfig(testName));
		}

        /// <summary>
        /// Gets the default <see cref="ITargetContainerConfig"/> to be used to configure <see cref="ITargetContainer"/> objects to be used for the tests.
        /// 
        /// Note that the return type is specialised for <see cref="CombinedTargetContainerConfig"/> to allow per-test alteration of the configuration.
        /// 
        /// The base implementation returns a new <see cref="CombinedTargetContainerConfig"/> cloned from <see cref="TargetContainer.DefaultConfig"/>
        /// </summary>
        /// <param name="testName"></param>
        /// <returns></returns>
        protected virtual CombinedTargetContainerConfig GetDefaultTargetContainerConfig([CallerMemberName]string testName = null)
        {
            return new CombinedTargetContainerConfig(TargetContainer.DefaultConfig.AsEnumerable());
        }

		/// <summary>
		/// Creates the container for the test or theory from the targets created by 
		/// <see cref="CreateTargetContainer(string)"/>.
		/// 
		/// Your implementation of <see cref="GetCompilerConfig(string)"/> is used to create a clone of the 
        /// <see cref="GlobalBehaviours.ContainerBehaviour"/> with its compiler behaviour replaced so that the tests
        /// run with the standard configuration - just with your compiler.
		/// </summary>
		/// <param name="targets">The targets to be used for the container.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual Container CreateContainer(IRootTargetContainer targets, [CallerMemberName]string testName = null)
		{
			return new Container(targets, GetContainerConfig(testName));
		}

		/// <summary>
		/// Creates a scoped container for the test or theory from the targets created by 
		/// <see cref="CreateTargetContainer(string)"/>.
		/// 
		/// Your implementation of <see cref="GetCompilerConfig(string)"/> is used to configure the container
		/// on creation.
		/// </summary>
		/// <param name="targets">The targets.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual ScopedContainer CreateScopedContainer(IRootTargetContainer targets, [CallerMemberName]string testName = null)
		{
			return new ScopedContainer(targets, GetContainerConfig(testName));
		}

		/// <summary>
		/// Creates an overriding container using the passed <paramref name="baseContainer"/> as the parent container.
		/// 
		/// The <paramref name="newTargets"/> target container is optionally used to seed the new container with additional
		/// registrations.
		/// </summary>
		/// <param name="baseContainer">The base container.</param>
		/// <param name="newTargets">The new targets.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual OverridingContainer CreateOverridingContainer(Container baseContainer, [CallerMemberName]string testName = null)
		{
			return new OverridingContainer(baseContainer, GetContainerConfig(testName));
		}

		/// <summary>
		/// Shortcut method for testing a compiler with a single target registered in a container.
		/// 
		/// Reduces common boilerplate found in many of the target-specfic compilation tests (although not
		/// all of it has been swapped over to using this method! :$)
		/// 
		/// Creates the target container through <see cref="CreateTargetContainer(string)"/>, then
		/// registers the <paramref name="target"/> in it, optionally using the passed <paramref name="registeredType"/>,
		/// and then creates and returns the container built from those targets by 
		/// <see cref="CreateContainer(ITargetContainer, string)"/>
		/// </summary>
		/// <param name="target">The target to be registered in the target container and, then, 
		/// compiled by the compiler under test via the returned container.</param>
		/// <param name="registeredType">Type against which the target should be registered, if different from the 
		/// <paramref name="target"/>'s <see cref="ITarget.DeclaredType"/>.</param>
		/// <param name="testName">Name of the test executing the method.  The compiler fills this in if ommitted.</param>
		protected Container CreateContainerForSingleTarget(ITarget target, Type serviceType = null, [CallerMemberName]string testName = null)
		{
			var targets = CreateTargetContainer(testName: testName);
			targets.Register(target, serviceType);
			return CreateContainer(targets, testName);
		}		
	}
}
