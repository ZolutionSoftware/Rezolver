// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


namespace Rezolver.Options
{
    /// <summary>
    /// Boolean option used to control whether <see cref="System.Lazy{T}"/> injection will automatically
    /// work.  Used by 
    /// </summary>
    public class EnableAutoLazyInjection : ContainerOption<bool>
    {
        /// <summary>
        /// Default value for this option - equivalent to <c>false</c>
        /// </summary>
        public static EnableAutoLazyInjection Default { get; } = false;

        /// <summary>
        /// Implicit conversion operator from <see cref="bool"/> to <see cref="EnableAutoLazyInjection"/>
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableAutoLazyInjection(bool value)
        {
            return new EnableAutoLazyInjection() { Value = value };
        }
    }
}
