using Rezolver.Targets;

namespace Rezolver.Compilation
{
	/// <summary>
	/// Interface for an object which configures the compiler for the given container and/or targets object
	/// to use a specific <see cref="ITargetCompiler"/> and <see cref="ICompileContextProvider"/>.
	/// 
	/// Used by all the standard container types in the Rezolver framework.
	/// 
	/// You can provide a specific provider to most containers on creation, and you can configure
	/// the default system-wide provider via the <see cref="CompilerConfiguration.DefaultProvider"/> static
	/// property.
	/// </summary>
	public interface ICompilerConfigurationProvider
	{
		/// <summary>
		/// Called to configure the compiler and context provider for the given container and/or targets.
		/// 
		/// When using the standard container types (based on <see cref="ContainerBase"/>), the method 
		/// should register directly-resolvable targets in the <paramref name="targets"/> target container
		/// for the types <see cref="ITargetCompiler"/> and <see cref="ICompileContextProvider"/> so that 
		/// the container can resolve the compiler and context provider.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="targets">The targets.</param>
		/// <remarks>The built-in container classes (all those which inherit from <see cref="ContainerBase"/>)
		/// rely on their <see cref="ITargetContainer"/> to directly resolve the compiler and context provider
		/// that is used to compile <see cref="ITarget"/> objects into <see cref="ICompiledTarget"/> instances.
		/// 
		/// Therefore, in order to use a specific compilation strategy, the framework needs some way of knowing
		/// how the underlying <see cref="ITargetContainer"/> should be configured to use that strategy.  Since
		/// this requires adding registrations to the container, the most flexible way to do that is via a callback,
		/// which is what this method technically represents.
		/// 
		/// Note that the registered targets must NOT require compilation - i.e. - the target must either directly
		/// implement <see cref="ITargetCompiler"/>/<see cref="ICompileContextProvider"/> or also implement
		/// <see cref="ICompiledTarget"/> such that when <see cref="ICompiledTarget.GetObject(ResolveContext)"/>
		/// is called, the actual compiler or context provider are returned.
		/// 
		/// The simplest way to achieve this is to build the compiler/context provider conventionally, and then
		/// use the <see cref="ObjectTarget"/> target to store them directly in the target container against
		/// the types for which they should be used.  This is how the default expression compiler does it - in addition
		/// to registering additional targets to support the different compilation strategies required for the
		/// different target types.</remarks>
		void Configure(IContainer container, ITargetContainer targets);
	}
}