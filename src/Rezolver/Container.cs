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
	/// Note that it doesn't implement lifetime scoping (although you can create a lifetime scope from it by calling its 
    /// <see cref="IScopeFactory.CreateScope"/> method).
	/// 
	/// Also note that the class implements <see cref="ITargetContainer"/> by wrapping the <see cref="ContainerBase.Targets"/> 
    /// property inherited from <see cref="ContainerBase"/>.
    /// 
    /// The reason for this is simplicity: in many applications, you'll want to simply create a new container, register
	/// services into it, and then start using it.
    /// 
    /// ## Configuration
    /// 
    /// Rezolver uses a configuration callback mechanism for pre-registering services or pre-configuring containers for certain
    /// types via the <see cref="IContainerConfig"/> interface.  This is similar to the <see cref="ITargetContainerConfig"/> interface
    /// that's used by <see cref="TargetContainer"/> to provide functionality such as automatic enumerable injection out of the box,
    /// except it focuses specifically on configuring the container itself, rather than just registrations.
    /// 
    /// If you don't provide a config on construction, then the <see cref="DefaultConfig"/> will be used.
	/// </remarks>
	public class Container : CachingContainerBase
	{
        /// <summary>
        /// The default container config used by all new containers.  You can add/remove configurations from this collection
        /// to change the defaults which are applied to new container instances; or you can supply an explicit configuration
        /// when creating your container.
        /// </summary>
        /// <remarks>
        /// The configurations present in this collection by default will set up the expression target compiler and extend
        /// the automatic enumerable injection functionality so that the <see cref="OverridingContainer"/> class can produce
        /// enumerables which are made up of targets registered in both the overriding container and its inner container.</remarks>
        public static CombinedContainerConfig DefaultConfig { get; } = new CombinedContainerConfig(new IContainerConfig[]
        {
            Configuration.ExpressionCompilation.Instance,
            // note: this config object only applies itself to OverridingContainer objects, and only when the 
            // EnableAutoEnumerables option is set to true in the ITargetContainer.
            Configuration.OverridingEnumerables.Instance
        });

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when 
        /// <see cref="IContainer.Resolve(IResolveContext)"/> (and other operations) is called.  If not provided, a new 
        /// <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available 
        /// to derived types, after construction, through the <see cref="Targets"/> property.</param>
        /// <param name="config">Can be null.  Configuration to apply to this container (and, potentially its <see cref="Targets"/>).
        /// If not provided, then the <see cref="DefaultConfig"/> will be used.</param>
        /// <remarks>Note to inheritors - this constructor throws an <see cref="InvalidOperationException"/> if used by a derived class,
        /// because the application of configuration to the container will likely cause virtual methods to be called.  Instead, you 
        /// should declare your own constructor with the same signature which chains instead to the <see cref="Container.Container(ITargetContainer)"/> 
        /// protected constructor; and then you should apply the configuration yourself in that constructor (falling back to 
        /// <see cref="DefaultConfig"/> if null).</remarks>
        public Container(ITargetContainer targets = null, IContainerConfig config = null)
			: base(targets)
		{
            if (this.GetType() != typeof(Container))
                throw new InvalidOperationException("This constructor must not be used by derived types because applying configuration will most likely trigger calls to virtual methods on this instance.  Please use the protected constructor and apply configuration explicitly in your derived class");

            (config ?? DefaultConfig).Configure(this, Targets);
		}

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class using the given target container (or a new 
        /// <see cref="TargetContainer"/> if not provided).  No <see cref="IContainerConfig"/> will be used, leaving the deriving class free to do so.
        /// </summary>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the container.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <remarks>This constructor must be used by derived types in order to avoid apply any configuration before they are initialised.
        /// 
        /// In order to apply configuration, derived types must declare their own constructor which accepts an <see cref="IContainerConfig"/>, and
        /// apply it (or the <see cref="DefaultConfig"/> as a default) after performing all initialisation.</remarks>
        protected Container(ITargetContainer targets)
            : base(targets)
		{

		}
	}
}