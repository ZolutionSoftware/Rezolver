// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Events
{
    public class TargetRegisteredEventArgs : EventArgs
    {
        public ITarget Target { get; }
        public Type Type { get; }
        public TargetRegisteredEventArgs(ITarget target, Type type)
        {
            Target = target;
            Type = type;
        }
    }
}