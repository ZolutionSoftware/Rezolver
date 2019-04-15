// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Provides factory methods for the <see cref="Configure{TOption}"/> generic class for when you need to
    /// create a configuration object for a specific option outside of the functionality provided by the
    /// <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/> function and
    /// its various overloads.
    /// </summary>
    public static class Configure
    {
        /// <summary>
        /// Wraps the <see cref="Configure{TOption}.Configure(TOption, Type)"/> constructor.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="value">The value that the option is to be set to.</param>
        /// <param name="serviceType">If the option is to be configured for a specific type of service in the target container,
        /// pass it here.</param>
        /// <returns>A new <see cref="Configure{TOption}"/> instance</returns>
        public static Configure<TOption> Option<TOption>(TOption value, Type serviceType = null)
            where TOption : class
        {
            return new Configure<TOption>(value, serviceType);
        }

        /// <summary>
        /// Wraps the <see cref="Configure{TOption}.Configure(TOption, Type)"/> constructor, passing
        /// <typeparamref name="TService"/> as the argument to the <c>serviceType</c> parameter.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <typeparam name="TService">The type of service against which the option will be set.</typeparam>
        /// <param name="value">The value the option is to be set to.</param>
        /// <returns>A new <see cref="Configure{TOption}"/> instance</returns>
        public static Configure<TOption> Option<TOption, TService>(TOption value)
            where TOption : class
        {
            return new Configure<TOption>(value, typeof(TService));
        }

        /// <summary>
        /// Wraps the <see cref="Configure{TOption}.Configure(Func{ITargetContainer, Type, TOption}, Type)"/>.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <param name="optionFactory">A callback that will be used to obtain the option value to be set.</param>
        /// <param name="serviceType">If the option is to be configured for a specific type of service in the target container,
        /// pass it here.</param>
        /// <returns>A new <see cref="Configure{TOption}"/> instance</returns>
        public static Configure<TOption> Option<TOption>(Func<ITargetContainer, Type, TOption> optionFactory, Type serviceType = null)
            where TOption : class
        {
            return new Configure<TOption>(optionFactory, serviceType);
        }

        /// <summary>
        /// Wraps the <see cref="Configure{TOption}.Configure(Func{ITargetContainer, Type, TOption}, Type)"/> constructor,
        /// passing <typeparamref name="TService"/> as the argument to the <c>serviceType</c> parameter.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <typeparam name="TService">The type of service against which the option will be set.</typeparam>
        /// <param name="optionFactory">A callback that will be used to obtain the option value to be set.</param>
        /// <returns>A new <see cref="Configure{TOption}"/> instance</returns>
        public static Configure<TOption> Option<TOption, TService>(Func<ITargetContainer, Type, TOption> optionFactory)
            where TOption : class
        {
            return new Configure<TOption>(optionFactory, typeof(TService));
        }
    }
}
