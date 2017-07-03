using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// An <see cref="ICompiledTarget"/> which wraps around an <see cref="IDirectTarget"/>.
    /// 
    /// The implementation of <see cref="GetObject(IResolveContext)"/> simply executes the target's
    /// <see cref="IDirectTarget.GetValue"/> method.
    /// </summary>
    internal class DirectCompiledTarget : ICompiledTarget
    {
        private readonly IDirectTarget _directTarget;

        public ITarget SourceTarget => _directTarget;

        public DirectCompiledTarget(IDirectTarget target)
        {
            _directTarget = target;
        }

        public object GetObject(IResolveContext context)
        {
            return _directTarget.GetValue();
        }
    }
}
