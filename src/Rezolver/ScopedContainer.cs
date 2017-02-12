// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Extends the <see cref="Container"/> to implement lifetime scoping through a private <see cref="IContainerScope"/>.
	/// 
	/// Both the <see cref="Resolve(ResolveContext)"/> and <see cref="TryResolve(ResolveContext, out object)"/> methods
	/// will inject the <see cref="Scope"/> into <see cref="ResolveContext"/> that's passed if the context doesn't already
	/// have a scope.
	/// 
	/// If you want your root container to act as a lifetime scope, then you should use this
	/// class instead of using <see cref="Container"/>.
	/// </summary>
	/// <remarks>Note that this class does NOT implement the <see cref="IContainerScope"/> interface because
	/// the two interfaces are not actually compatible with each other, thanks to identical sets of extension methods.
	/// </remarks>
	public class ScopedContainer : Container, IDisposable
	{
		private readonly IContainerScope _scope;

		/// <summary>
		/// Gets the underlying scope used by this container for all <see cref="IContainer.Resolve(ResolveContext)"/> calls
		/// which do not already have a scope defined.
		/// </summary>
		/// <value>The scope.</value>
		public IContainerScope Scope
		{
			get
			{
				return _scope;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScopedContainer"/> class.
		/// </summary>
		/// <param name="targets">Optional.  The underlying target container to be used to resolve objects.</param>
		/// <param name="compilerConfig">Optional.  The compiler configuration.</param>
		public ScopedContainer(ITargetContainer targets = null, ICompilerConfigurationProvider compilerConfig = null)
			: base(targets, compilerConfig)
		{
			_scope = new ContainerScope(this);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_scope.Dispose();
				}

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		public override IContainerScope CreateScope()
		{
			return new ContainerScope(Scope);
		}
		#endregion
		public override bool CanResolve(ResolveContext context)
		{
			return base.CanResolve(context.Scope == null ? context.CreateNew(Scope) : context);
		}

		public override bool TryResolve(ResolveContext context, out object result)
		{
			return base.TryResolve(context.Scope == null ? context.CreateNew(Scope) : context, out result);
		}

		public override object Resolve(ResolveContext context)
		{
			return base.Resolve(context.Scope == null ? context.CreateNew(Scope) : context);
		}
	}
}
