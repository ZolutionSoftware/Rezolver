using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Provides factory methods for the <see cref="ConfigureOption{TOption}"/> generic class for when you need to
    /// create a configuration object for a specific option outside of the functionality provided by the 
    /// <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/> function and
    /// its various overloads.
    /// </summary>
    public static class ConfigureOption
    {
        /// <summary>
        /// Wraps the <see cref="ConfigureOption{TOption}.ConfigureOption(TOption, Type)"/> constructor.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="value">The value that the option is to be set to.</param>
        /// <param name="serviceType">If the option is to be configured for a specific type of service in the target container,
        /// pass it here.</param>
        /// <returns>A new <see cref="ConfigureOption{TOption}"/> instance</returns>
        public static ConfigureOption<TOption> With<TOption>(TOption value, Type serviceType = null)
            where TOption : class
        {
            return new ConfigureOption<TOption>(value, serviceType);
        }

        /// <summary>
        /// Wraps the <see cref="ConfigureOption{TOption}.ConfigureOption(TOption, Type)"/> constructor, passing
        /// <typeparamref name="TService"/> as the argument to the <c>serviceType</c> parameter.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <typeparam name="TService">The type of service against which the option will be set.</typeparam>
        /// <param name="value">The value the option is to be set to.</param>
        /// <returns>A new <see cref="ConfigureOption{TOption}"/> instance</returns>
        public static ConfigureOption<TOption> With<TOption, TService>(TOption value)
            where TOption : class
        {
            return new ConfigureOption<TOption>(value, typeof(TService));
        }

        /// <summary>
        /// Wraps the <see cref="ConfigureOption{TOption}.ConfigureOption(Func{ITargetContainer, Type, TOption}, Type)"/>.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <param name="optionFactory">A callback that will be used to obtain the option value to be set.</param>
        /// <param name="serviceType">If the option is to be configured for a specific type of service in the target container,
        /// pass it here.</param>
        /// <returns>A new <see cref="ConfigureOption{TOption}"/> instance</returns>
        public static ConfigureOption<TOption> With<TOption>(Func<ITargetContainer, Type, TOption> optionFactory, Type serviceType = null)
            where TOption : class
        {
            return new ConfigureOption<TOption>(optionFactory, serviceType);
        }

        /// <summary>
        /// Wraps the <see cref="ConfigureOption{TOption}.ConfigureOption(Func{ITargetContainer, Type, TOption}, Type)"/> constructor,
        /// passing <typeparamref name="TService"/> as the argument to the <c>serviceType</c> parameter.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <typeparam name="TService">The type of service against which the option will be set.</typeparam>
        /// <param name="optionFactory">A callback that will be used to obtain the option value to be set.</param>
        /// <returns>A new <see cref="ConfigureOption{TOption}"/> instance</returns>
        public static ConfigureOption<TOption> With<TOption, TService>(Func<ITargetContainer, Type, TOption> optionFactory)
            where TOption : class
        {
            return new ConfigureOption<TOption>(optionFactory, typeof(TService));
        }
    }
}
