using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Events
{
    public sealed class TargetContainerRegisteredEvent
    {
        public ITargetContainer TargetContainer { get; }
        public Type Type { get; }

        public TargetContainerRegisteredEvent(ITargetContainer targetContainer, Type type)
        {
            TargetContainer = targetContainer;
            Type = type;
        }
    }
}
