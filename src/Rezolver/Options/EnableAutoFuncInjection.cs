// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Options
{
    /// <summary>
    /// Option which controls whether the <see cref="Configuration.InjectAutoFuncs"/> configuration is applied, which
    /// enables the automatic registration of <see cref="Func{TResult}"/> whenever a target is registered.
    /// </summary>
    public class EnableAutoFuncInjection : ContainerOption<bool>
    {
        /// <summary>
        /// The Default setting for the <see cref="EnableAutoFuncInjection"/> option - evaluates to <c>false</c>
        /// </summary>
        public static EnableAutoFuncInjection Default { get; } = false;

        /// <summary>
        /// Convenience operator for treating booleans as <see cref="EnableAutoFuncInjection"/> option values.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableAutoFuncInjection(bool value)
        {
            return new EnableAutoFuncInjection() { Value = value };
        }
    }
}
