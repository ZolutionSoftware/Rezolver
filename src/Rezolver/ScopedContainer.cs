﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
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
	/// Extends the <see cref="Container"/> to implement lifetime implicit scoping through the
	/// <see cref="Scope"/> that's created along with it.
	/// 
	/// Implementation of the <see cref="IScopedContainer"/> interface.
	/// </summary>
	/// <remarks>
	/// If you want your root container to act as a lifetime scope, then you should use this
	/// class instead of using <see cref="Container"/>.
	/// 
	/// Note that this class does NOT implement the <see cref="IContainerScope"/> interface because
	/// the two interfaces are not actually compatible with each other, thanks to identical sets of extension methods.
	/// </remarks>
	public sealed class ScopedContainer : Container, IScopedContainer, IDisposable
	{
		private readonly IContainerScope _scope;

		/// <summary>
		/// Gets the scope for this scoped container.
		/// 
		/// Note that this is used automatically by the container for <see cref="IContainer.Resolve(IResolveContext)"/>
		/// operations where the <see cref="IResolveContext.Scope"/> property is not already set.
		/// </summary>
		public IContainerScope Scope
		{
			get
			{
				return _scope;
			}
		}

        /// <summary>
        /// Constructs a new instance of the <see cref="ScopedContainer"/> class.
        /// </summary>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the 
        /// container, ultimately being passed to the <see cref="Targets"/> property.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <param name="behaviour">Can be null.  A behaviour to attach to this container (and, potentially its 
        /// <see cref="ContainerBase.Targets"/>). If not provided, then the global 
        /// <see cref="GlobalBehaviours.ContainerBehaviour"/> will be used.</param>
        public ScopedContainer(ITargetContainer targets = null, IContainerConfig behaviour = null)
			: base(targets)
		{
			_scope = new ContainerScope(this);
            (behaviour ?? GlobalBehaviours.ContainerBehaviour).Attach(this, Targets);
		}

        /// <summary>
        /// Constructs a new instance of the <see cref="ScopedContainer"/> class, with a default new 
        /// <see cref="TargetContainer"/> as the <see cref="ContainerBase.Targets"/>; using the passed 
        /// <paramref name="behaviour"/> to initialise additional functionality.
        /// </summary>
        /// <param name="behaviour">Can be null.  A behaviour to attach to this container (and, potentially its 
        /// <see cref="ContainerBase.Targets"/>). If not provided, then the global 
        /// <see cref="GlobalBehaviours.ContainerBehaviour"/> will be used.</param>
        public ScopedContainer(IContainerConfig behaviour)
            : this(targets: null, behaviour: behaviour)
        {

        }

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
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

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Overrides the base method's implementation of <see cref="IScopeFactory.CreateScope" /> to pass the call to the <see cref="Scope" />.
		/// </summary>
		public override IContainerScope CreateScope()
		{
			return Scope.CreateScope();
		}
		#endregion

	}
}
