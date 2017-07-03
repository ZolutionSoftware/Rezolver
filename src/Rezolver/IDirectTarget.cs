using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// A target that can return an object without needing to be compiled; but which can still take part
    /// in the compilation process (assuming a compatible compiler is configured).
    /// </summary>
    /// <remarks>This has similarities to <see cref="ICompiledTarget"/> except implementations of
    /// this interface guarantee that an object can be obtained (via <see cref="GetValue"/>)
    /// outside the context of a <see cref="IContainer.Resolve(IResolveContext)"/> operation,
    /// without requiring any compilation or other container operations.
    /// 
    /// Rezolver uses targets like this extensively for its configuration and container 
    /// services functionality - which are required to be accessible directly from an <see cref="ITargetContainer"/>
    /// without an <see cref="IContainer"/> being available.  For example, the <see cref="GenericTargetContainer"/> uses
    /// its own registrations (configured by <see cref="ITargetContainerConfig"/> objects) to control how generic
    /// registrations are mapped to generic type requests.</remarks>
    internal interface IDirectTarget : ITarget
    {
        /// <summary>
        /// Gets the object 
        /// </summary>
        /// <returns></returns>
        object GetValue();
    }
}
