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
	/// Note that it doesn't implement lifetime scoping (although you can create a lifetime scope from it by calling its <see cref="IScopeFactory.CreateScope"/>
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
        /// <param name="targets">Optional.  The targets that will be used as the source of registrations for the container.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <param name="initialiser">Can be null.  An initialiser which configures the new instance (and its <see cref="Targets"/>)
        /// with additional functionality, such as compiler etc.  If not provided, then the <see cref="GlobalBehaviours.ContainerBehaviour"/>
        /// will be used.</param>
        public Container(ITargetContainer targets = null, IContainerBehaviour initialiser = null)
            //note the use of the non-initialising base constructor.  It's important that we don't initialise
            //till after all bases' constructors are done.
			: base(targets)
		{
            (initialiser ?? GlobalBehaviours.ContainerBehaviour).Attach(this, Targets);
		}

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class, with a default new <see cref="TargetContainer"/>
        /// as the <see cref="Targets"/>; using the passed <paramref name="initialiser"/> to initialise additional functionality.
        /// </summary>
        /// <param name="initialiser">Can be null.  An initialiser which configures the new instance (and its <see cref="Targets"/>)
        /// with additional functionality, such as compiler etc.  If not provided, then the <see cref="GlobalBehaviours.ContainerBehaviour"/>
        /// will be used.</param>
        public Container(IContainerBehaviour initialiser) :
            this(targets: null, initialiser: initialiser)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class using the given target container (or a new <see cref="TargetContainer"/>
        /// if not provided).  No <see cref="IContainerBehaviour"/> will be used, leaving the deriving class free to do so.
        /// </summary>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the container.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <remarks>This constructor will not use any <see cref="IContainerBehaviour"/> objects to perform post-creation initialisation
        /// of the container.  If you want it to do so, then use the <see cref="CachingContainerBase.CachingContainerBase(IContainerBehaviour, ITargetContainer)"/>
        /// constructor.</remarks>
        protected Container(ITargetContainer targets)
            : base(targets)
		{

		}
	}
}