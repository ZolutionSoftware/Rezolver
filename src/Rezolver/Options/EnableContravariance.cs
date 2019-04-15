// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Options
{
    /// <summary>
    /// A <see cref="bool"/> container options that controls whether contravariant generic
    /// parameters will be matched to registrations of bases/interfaces of the associated
    /// type.
    ///
    /// This option can be applied globally or on a per-service basis. See the remarks section
    /// for more.
    ///
    /// The <see cref="Default"/> (unset) is equivalent to <c>true</c>, meaning that contravariance
    /// is enabled, for all applicable types.
    /// </summary>
    /// <remarks>To disable contravariance globally, you can set this option to <c>false</c>
    /// using the <see cref="OptionsTargetContainerExtensions.SetOption{TOption}(ITargetContainer, TOption)"/>
    /// extension method.
    ///
    /// You can also disable contravariance for a particular interface or delegate type - either by
    /// targetting the open generic (e.g. <see cref="Action{T}"/> - which disables it for any type lookup
    /// for <see cref="Action{T}"/>) or for a specific closed version of that generic
    /// (e.g. <c>Action&lt;Foo, Bar&gt;</c>).</remarks>
    /// <seealso cref="EnableGlobalOptions"/>
    public class EnableContravariance : ContainerOption<bool>
    {
        /// <summary>
        /// Default value for this option - equivalent to <c>true</c>
        /// </summary>
        public static EnableContravariance Default = true;

        /// <summary>
        /// Implicit conversion operator from <see cref="bool"/> to <see cref="EnableContravariance"/> -
        /// simplifies getting and setting this option.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableContravariance(bool value)
        {
            return new EnableContravariance() { Value = value };
        }
    }
}
