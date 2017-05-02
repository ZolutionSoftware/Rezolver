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
	/// </remarks>
	public class Container : CachingContainerBase
	{
        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when <see cref="Resolve(IResolveContext)"/> 
        /// (and other operations) is called.  If not provided, a new <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available 
        /// to inherited types, after construction, through the <see cref="Targets"/> property.</param>
        /// <param name="behaviour">Can be null.  A behaviour to attach to this container (and, potentially its <see cref="Targets"/>).
        /// If not provided, then the global <see cref="GlobalBehaviours.ContainerBehaviour"/> will be used.</param>
        /// <remarks>Note to inheritors - to create an instance without attaching behaviours (thereby avoiding inadvertant
        /// virtual method calls from this class via the constructor), use the <see cref="Container.Container(ITargetContainer)"/> 
        /// protected constructor.</remarks>
        public Container(ITargetContainer targets = null, IContainerBehaviour behaviour = null)
			: base(targets)
		{
            (behaviour ?? GlobalBehaviours.ContainerBehaviour).Attach(this, Targets);
		}

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class, with a default new <see cref="TargetContainer"/>
        /// as the <see cref="Targets"/>; using the passed <paramref name="behaviour"/> to attach additional functionality
        /// to the container.
        /// </summary>
        /// <param name="behaviour">Can be null.  A behaviour to attach to this container (and, potentially its <see cref="Targets"/>).
        /// If not provided, then the global <see cref="GlobalBehaviours.ContainerBehaviour"/> will be used.</param>
        /// <remarks>Note to inheritors - to create an instance without attaching behaviours (thereby avoiding inadvertant
        /// virtual method calls from this class via the constructor), use the <see cref="Container.Container(ITargetContainer)"/> 
        /// protected constructor.</remarks>
        public Container(IContainerBehaviour behaviour) :
            this(targets: null, behaviour: behaviour)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class using the given target container (or a new 
        /// <see cref="TargetContainer"/> if not provided).  No <see cref="IContainerBehaviour"/> will be used, leaving the deriving class free to do so.
        /// </summary>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the container.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <remarks>This constructor does not attach any <see cref="IContainerBehaviour"/> behaviours, because behaviours typically
        /// call methods which are declared virtual on this class - which could be unsafe.
        /// 
        /// If this does not apply to your derived class (which is unlikely) - use one of the 
        /// <see cref="Container.Container(ITargetContainer, IContainerBehaviour)"/> or 
        /// <see cref="Container.Container(IContainerBehaviour)"/> constructors.</remarks>
        protected Container(ITargetContainer targets)
            : base(targets)
		{

		}
	}
}