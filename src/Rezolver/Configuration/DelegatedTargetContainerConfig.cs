using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    internal class DelegatedTargetContainerConfig : ITargetContainerConfig
    {
        private readonly Action<ITargetContainer> _configure;

        public DelegatedTargetContainerConfig(Action<ITargetContainer> configure)
        {
            _configure = configure;
        }
        void ITargetContainerConfig.Configure(ITargetContainer targets)
        {
            _configure(targets);
        }
    }
}
