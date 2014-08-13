using System;

namespace Rezolver
{
	/// <summary>
	/// A rezolver that's also a lifetime scope - that is, it's disposable,
	/// and will dispose of any disposable instances that it creates when it's disposed.
	/// 
	/// Also, any subsequent lifetime scopes that it, or any child, creates will 
	/// be disposed of when this scope is disposed.
	/// </summary>
	public interface ILifetimeScopeRezolver : IRezolver, IDisposable
	{
		//TODO: add methods here to register and fetch disposable instances directly to/from this scope.
		void AddToScope(IDisposable disposable);
	}
}