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
		/// Gets the compiler configuration provider to be used to configure the container 
		/// returned by <see cref="CreateContainer(ITargetContainer, string)"/> and when performing
		/// the tests of the <see cref="CompilerConfiguration.DefaultProvider"/> configuration provider.
		/// </summary>
		/// <param name="testName">Name of the test.</param>
		protected abstract IContainerConfiguration GetCompilerConfigProvider([CallerMemberName]string testName = null);

		/// <summary>
		/// Creates the target container for the test.
		/// </summary>
		/// <param name="testName">Name of the test.</param>
		protected virtual ITargetContainer CreateTargetContainer([CallerMemberName]string testName = null)
		{
			return new TargetContainer();
		}

		/// <summary>
		/// Creates the container for the test or theory from the targets created by 
		/// <see cref="CreateTargetContainer(string)"/>.
		/// 
		/// Your implementation of <see cref="GetCompilerConfigProvider(string)"/> is used to configure the container
		/// on creation.
		/// </summary>
		/// <param name="targets">The targets to be used for the container.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual IContainer CreateContainer(ITargetContainer targets, [CallerMemberName]string testName = null)
		{
			return new Container(targets, GetCompilerConfigProvider(testName));
		}

		/// <summary>
		/// Creates a scoped container for the test or theory from the targets created by 
		/// <see cref="CreateTargetContainer(string)"/>.
		/// 
		/// Your implementation of <see cref="GetCompilerConfigProvider(string)"/> is used to configure the container
		/// on creation.
		/// </summary>
		/// <param name="targets">The targets.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual IScopedContainer CreateScopedContainer(ITargetContainer targets, [CallerMemberName]string testName = null)
		{
			return new ScopedContainer(targets, GetCompilerConfigProvider(testName));
		}

		/// <summary>
		/// Creates an overriding container using the passed <paramref name="baseContainer"/> as the parent container.
		/// 
		/// The <paramref name="newTargets"/> target container is optionally used to seed the new container with additional
		/// registrations.
		/// 
		/// Note that the new container should resolve the same compiler as the base container by virtue of the fact that
		/// it is sharing its registrations.  There are tests which cover this.
		/// </summary>
		/// <param name="baseContainer">The base container.</param>
		/// <param name="newTargets">The new targets.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual IContainer CreateOverridingContainer(IContainer baseContainer, ITargetContainer newTargets = null, [CallerMemberName]string testName = null)
		{
			return new OverridingContainer(baseContainer, newTargets);
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
		protected IContainer CreateContainerForSingleTarget(ITarget target, Type serviceType = null, [CallerMemberName]string testName = null)
		{
			var targets = CreateTargetContainer(testName);
			targets.Register(target, serviceType);
			return CreateContainer(targets, testName);
		}		
	}
}
