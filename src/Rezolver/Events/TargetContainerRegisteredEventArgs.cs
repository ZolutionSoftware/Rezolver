// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Events
{
    public class TargetContainerRegisteredEventArgs : EventArgs
    {
        public ITargetContainer Container { get; }
        public Type Type { get; }
        public TargetContainerRegisteredEventArgs(ITargetContainer container, Type type)
        {
            Container = container;
            Type = type;
        }
    }
}