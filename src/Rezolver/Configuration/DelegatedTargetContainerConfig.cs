using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    internal class DelegatedTargetContainerConfig : ITargetContainerConfig
    {
        private readonly Action<IRootTargetContainer> _configure;

        public DelegatedTargetContainerConfig(Action<IRootTargetContainer> configure)
        {
            _configure = configure;
        }
        void ITargetContainerConfig.Configure(IRootTargetContainer targets)
        {
            _configure(targets);
        }
    }
}
