// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// The standard IOC container class in Rezolver.
	/// </summary>
	/// <remarks>
	/// All of this class' functionality is inherited through <see cref="CachingContainerBase"/> and its base classes.
	/// 
	/// Note that it doesn't implement lifetime scoping (although you can create a lifetime scope from it by calling its <see cref="IContainer.CreateLifetimeScope"/>
	/// method).
	/// 
	/// Also note that the class implements <see cref="ITargetContainer"/> through its <see cref="ContainerBase"/> base (which merely wraps around the 
	/// <see cref="ContainerBase.Targets"/> property.  The reason for this is simplicity: in many applications, you'll want to simply create a new container, register
	/// services into it, and then start using it.
	/// </remarks>
	public class Container : CachingContainerBase
	{
		/// <summary>
		/// Constructs a new instance of the <see cref="Container"/> class.
		/// </summary>
		/// <param name="targets">Optional.  The targets that will be used to resolve objects.  If left null, then a new, empty, target container will be constructed.</param>
		/// <param name="compilerConfig">Optional.  An object which will be used to configure this container and its targets to use a specific compilation
		/// strategy.  If <c>null</c>, then the <see cref="CompilerConfiguration.DefaultProvider"/> provider will be used.</param>
		public Container(ITargetContainer targets = null, ICompilerConfigurationProvider compilerConfig = null)
			: base(targets, compilerConfig)
		{
			
		}

		/// <summary>
		/// Constructs a new instance of the <see cref="Container"/> class using the given target container and the
		/// default compiler configuration (<see cref="CompilerConfiguration.DefaultProvider"/>).
		/// </summary>
		/// <param name="targets">The targets that will be used to resolve objects.  If left null, then a new, empty, target container will be constructed.</param>
		public Container(ITargetContainer targets): this(targets, null)
		{

		}

		/// <summary>
		/// Constructs a new instance of the <see cref="Container"/> class using a default empty <see cref="ITargetContainer"/>
		/// </summary>
		/// <param name="compilerConfig">An object which will be used to configure this container and its targets to use a 
		/// specific compilation strategy.  If <c>null</c>, then the <see cref="CompilerConfiguration.DefaultProvider"/> provider 
		/// will be used.</param>
		public Container(ICompilerConfigurationProvider compilerConfig) : this(null, compilerConfig)
		{

		}
	}
}