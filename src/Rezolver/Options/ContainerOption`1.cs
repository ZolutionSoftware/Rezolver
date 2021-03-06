﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Options
{
    /// <summary>
    /// A suggested base class to use for custom container options to be read/written through extensions
    /// such as <see cref="TargetContainerExtensions.SetOption{TOption}(ITargetContainer, TOption)"/>,
    /// <see cref="TargetContainerExtensions.GetOptions{TOption, TService}(ITargetContainer)"/> and the various
    /// overloads.S
    ///
    /// The type of the option value is the argument to the <typeparamref name="TOption"/> type parameter.
    ///
    /// Options must currently be objects - the ability to use callbacks to get options might be added at a
    /// future date.
    /// </summary>
    /// <typeparam name="TOption">The underlying option value type - e.g. <see cref="bool"/>, <see cref="string"/>,
    /// <see cref="Uri"/> or whatever</typeparam>
    /// <remarks>
    /// **This type is creatable only through inheritance.**
    ///
    ///
    /// Options in Rezolver are achieved by using registrations in the <see cref="ITargetContainer"/> that
    /// is to be configured (and, in turn, which might then configure any <see cref="Container"/>s built from
    /// that target container).
    ///
    /// Since options often take the form of primitive types - e.g <see cref="bool"/>, <see cref="string"/> etc - this
    /// means it's impossible to register multiple options which control different things which have the same underlying
    /// type if we registered them directly.
    ///
    /// This class is offered as a way around this.  Options (e.g. <see cref="AllowMultiple"/>) are derived from this type,
    /// with the argument to the <typeparamref name="TOption"/> type parameter set to the underlying type of the option value.
    ///
    /// Thus - each distinct option is a different type, which then means that an <see cref="ITargetContainer"/> can distinguish
    /// between them.
    ///
    /// ---
    ///
    /// ### Note:
    ///
    /// Whilst Rezolver uses this type for most of its configurable options, you don't need to use it. Any type can be used as 
    /// an option.</remarks>
    [System.Diagnostics.DebuggerDisplay("Value = {Value}")]
    public class ContainerOption<TOption>
    {
        /// <summary>
        /// The underlying value wrapped by this option.
        /// </summary>
        public TOption Value { get; protected set; }

        /// <summary>
        /// Inheritance constructor.
        /// </summary>
        protected ContainerOption() { }

        /// <summary>
        /// Implicit casting operator to convert to the option value type from an instance of <see cref="ContainerOption{TOption}"/>.
        ///
        /// All derived types are encouraged to have a similar casting operator from <typeparamref name="TOption" /> to <see cref="ContainerOption{TOption}"/>
        /// (for example, see <see cref="EnableEnumerableInjection"/>).
        /// </summary>
        /// <param name="option">The option object to be cast to <typeparamref name="TOption" /> (by reading its <see cref="Value"/> property).
        ///
        /// Note that if this is <c>null</c>, then the return value will be the default for <typeparamref name="TOption" /></param>
        public static implicit operator TOption(ContainerOption<TOption> option)
        {
            return option != null ? option.Value : default;
        }

        /// <summary>
        /// Provides a textual representation of the value of this option and its underlying type.
        /// </summary>
        /// <returns>A string in the form <code>"{Value} ({typeof(TOption)})"</code></returns>
        public override string ToString()
        {
            return $"{Value} ({typeof(TOption)})";
        }
    }
}
