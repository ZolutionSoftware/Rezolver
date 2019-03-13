// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
            this._configure = configure;
        }

        void ITargetContainerConfig.Configure(IRootTargetContainer targets)
        {
            this._configure(targets);
        }
    }
}
