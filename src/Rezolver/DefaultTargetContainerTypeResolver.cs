using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal class DefaultTargetContainerTypeResolver : ITargetContainerTypeResolver
    {
        internal static DefaultTargetContainerTypeResolver Instance { get; } = new DefaultTargetContainerTypeResolver();

        private DefaultTargetContainerTypeResolver() { }

        public Type GetContainerType(Type serviceType)
        {
            return serviceType;
        }
    }
}
