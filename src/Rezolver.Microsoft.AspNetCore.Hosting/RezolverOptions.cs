// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Options for using Rezolver as the DI container for Asp.Net Core web applications.
	/// </summary>
	public class RezolverOptions
	{
        /// <summary>
        /// The <see cref="ITargetContainerConfig"/> to be used to initialise the <see cref="ITargetContainer"/>
        /// for the application.
        /// </summary>
        /// <remarks>This is always cloned from the <see cref="TargetContainer.DefaultConfig"/> using the 
        /// <see cref="CombinedTargetContainerConfig.Clone"/> method.</remarks>
		public CombinedTargetContainerConfig TargetContainerConfig { get; set; }
        /// <summary>
        /// The <see cref="IContainerConfig"/> to be used to initialise the <see cref="IContainer"/>
        /// for the application.  Can be used to control the compiler used to translate <see cref="ITarget"/>
        /// registrations into <see cref="ICompiledTarget"/> objects.
        /// </summary>
        /// <remarks>This is always cloned from the <see cref="Container.DefaultConfig"/> using the 
        /// <see cref="CombinedContainerConfig.Clone"/> method.</remarks>
        public CombinedContainerConfig ContainerConfig { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="RezolverOptions"/> class.
        /// </summary>
        public RezolverOptions()
        {
            TargetContainerConfig = TargetContainer.DefaultConfig.Clone();
            ContainerConfig = Container.DefaultConfig.Clone();
        }
    }
}
