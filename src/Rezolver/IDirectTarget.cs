using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// A target that can return an object without needing to be compiled.
    /// 
    /// This has similarities to <see cref="ICompiledTarget"/> except implementations of
    /// this interface guarantee that an object can be obtained via <see cref="GetValue"/> 
    /// without requiring any compilation or other container operations.
    /// 
    /// Crucially, the object returned from <see cref="GetValue"/> will be the same (although
    /// not necessarily the same reference) as if the target was compiled and used for a 
    /// 'normal' <see cref="IContainer.Resolve(IResolveContext)"/> operation.
    /// </summary>
    /// <remarks>Rezolver uses targets like this extensively for its configuration and container 
    /// services functionality.  For example, the <see cref="TargetDictionaryContainer"/> uses
    /// its own registrations (configured by <see cref="ITargetContainerBehaviour"/> objects)
    /// </remarks>
    internal interface IDirectTarget : ITarget
    {
        /// <summary>
        /// Gets the object 
        /// </summary>
        /// <returns></returns>
        object GetValue();
    }
}
