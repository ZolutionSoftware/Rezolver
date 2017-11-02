using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Extensions for pre-configuring options in an <see cref="ITargetContainer"/> via a <see cref="CombinedContainerConfig"/>, such as the one
    /// exposed by <see cref="TargetContainer.DefaultConfig"/>.
    /// </summary>
    public static class CombinedTargetContainerConfigExtensions
    {
        /// <summary>
        /// Adds a <see cref="Configuration.Configure{TOption}"/> callback to the configuration, which will set the passed <paramref name="optionValue"/>
        /// option in the <see cref="ITargetContainer"/> to which the <paramref name="config"/> is later applied.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <param name="config">The combined config to which the configuration callback will be added.</param>
        /// <param name="optionValue">The value of the option that is to be set when <paramref name="config"/> is applied to an <see cref="ITargetContainer"/>
        /// via its implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/></param>
        /// <returns>The <paramref name="config"/> object, for method chaining.</returns>
        public static CombinedTargetContainerConfig ConfigureOption<TOption>(this CombinedTargetContainerConfig config, TOption optionValue)
            where TOption : class
        {
            return ConfigureOptionInternal(
                config ?? throw new ArgumentNullException(nameof(config)),
                optionValue ?? throw new ArgumentNullException(nameof(optionValue)));
        }

        /// <summary>
        /// Adds a <see cref="Configuration.Configure{TOption}"/> callback to the configuration, which will set the passed <paramref name="optionValue"/>
        /// option for the service type <typeparamref name="TService"/> in the <see cref="ITargetContainer"/> to which the <paramref name="config"/> is later applied.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <typeparam name="TService">The service type for which the option is to be set.  Use of this is option-dependent - i.e. some options are read 
        /// in a service-specific way and some aren't.</typeparam>
        /// <param name="config">The combined config to which the configuration callback will be added.</param>
        /// <param name="optionValue">The value of the option that is to be set when <paramref name="config"/> is applied to an <see cref="ITargetContainer"/>
        /// via its implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/></param>
        /// <returns>The <paramref name="config"/> object, for method chaining.</returns>
        public static CombinedTargetContainerConfig ConfigureOption<TOption, TService>(this CombinedTargetContainerConfig config, TOption optionValue)
            where TOption : class
        {
            return ConfigureOptionInternal(
                config ?? throw new ArgumentNullException(nameof(config)),
                optionValue ?? throw new ArgumentNullException(nameof(optionValue)),
                typeof(TService));
        }

        /// <summary>
        /// Adds a <see cref="Configuration.Configure{TOption}"/> callback to the configuration, which will set the passed <paramref name="optionValue"/>
        /// option for the service type <paramref name="serviceType"/> in the <see cref="ITargetContainer"/> to which the <paramref name="config"/> is later applied.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="config">The combined config to which the configuration callback will be added.</param>
        /// <param name="serviceType">The service type for which the option is to be set.  Use of this is option-dependent - i.e. some options are read 
        /// in a service-specific way and some aren't.  Passing <c>null</c> is equivalent to calling <see cref="ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/>.</param>
        /// <param name="optionValue">The value of the option that is to be set when <paramref name="config"/> is applied to an <see cref="ITargetContainer"/>
        /// via its implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/></param>
        /// <returns>The <paramref name="config"/> object, for method chaining.</returns>
        public static CombinedTargetContainerConfig ConfigureOption<TOption>(this CombinedTargetContainerConfig config, Type serviceType, TOption optionValue)
            where TOption : class
        {
            return ConfigureOptionInternal(
                config ?? throw new ArgumentNullException(nameof(config)),
                optionValue ?? throw new ArgumentNullException(nameof(optionValue)),
                serviceType);
        }


        /// <summary>
        /// Adds a <see cref="Configuration.Configure{TOption}"/> callback to the configuration, which will set the option value returned by 
        /// the <paramref name="optionValueFactory"/> in the <see cref="ITargetContainer"/> to which the <paramref name="config"/> is applied.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set</typeparam>
        /// <param name="config">The combined config to which the configuration callback will be added.</param>
        /// <param name="optionValueFactory">A callback which returns value of the option that is to be set when <paramref name="config"/> is applied to an <see cref="ITargetContainer"/>
        /// via its implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/>.  The callback will be passed the target container being configured at that time.</param>
        /// <returns>The <paramref name="config"/> object, for method chaining.</returns>
        public static CombinedTargetContainerConfig ConfigureOption<TOption>(this CombinedTargetContainerConfig config, Func<ITargetContainer, TOption> optionValueFactory)
            where TOption : class
        {
            return ConfigureOptionInternal(
                config ?? throw new ArgumentNullException(nameof(config)),
                optionValueFactory ?? throw new ArgumentNullException(nameof(optionValueFactory)));
        }

        /// <summary>
        /// Adds a <see cref="Configuration.Configure{TOption}"/> callback to the configuration, which will set the option value returned by 
        /// the <paramref name="optionValueFactory"/> for the service type <typeparamref name="TService"/> in the <see cref="ITargetContainer"/> to which 
        /// the <paramref name="config"/> is applied.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <typeparam name="TService">The service type for which the option is to be set.  Use of this is option-dependent - i.e. some options are read 
        /// in a service-specific way and some aren't.</typeparam>
        /// <param name="config">The combined config to which the configuration callback will be added.</param>
        /// <param name="optionValueFactory">A callback which returns value of the option that is to be set when <paramref name="config"/> is applied to an <see cref="ITargetContainer"/>
        /// via its implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/>.  The callback will be passed the target container being configured at that time, 
        /// and a <see cref="Type"/> equal to <typeparamref name="TService"/>.</param>
        /// <returns>The <paramref name="config"/> object, for method chaining.</returns>
        public static CombinedTargetContainerConfig ConfigureOption<TOption, TService>(this CombinedTargetContainerConfig config, Func<ITargetContainer, Type, TOption> optionValueFactory)
            where TOption : class
        {
            return ConfigureOptionInternal(
                config ?? throw new ArgumentNullException(nameof(config)),
                optionValueFactory ?? throw new ArgumentNullException(nameof(optionValueFactory)),
                typeof(TService));
        }

        /// <summary>
        /// Adds a <see cref="Configuration.Configure{TOption}"/> callback to the configuration, which will set the option value returned by 
        /// the <paramref name="optionValueFactory"/> for the service type <paramref name="serviceType"/> in the <see cref="ITargetContainer"/> to which 
        /// the <paramref name="config"/> is applied.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="config">The combined config to which the configuration callback will be added.</param>
        /// <param name="serviceType">The service type for which the option is to be set.  Use of this is option-dependent - i.e. some options are read 
        /// in a service-specific way and some aren't.  Passing <c>null</c> is equivalent to calling <see cref="ConfigureOption{TOption}(CombinedTargetContainerConfig, Type, Func{ITargetContainer, Type, TOption})"/>.</param>
        /// <param name="optionValueFactory">A callback which returns value of the option that is to be set when <paramref name="config"/> is applied to an <see cref="ITargetContainer"/>
        /// via its implementation of <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/>.  The callback will be passed the target container being configured at that time, 
        /// and a <see cref="Type"/> equal to <paramref name="serviceType"/>.</param>
        /// <returns>The <paramref name="config"/> object, for method chaining.</returns>
        public static CombinedTargetContainerConfig ConfigureOption<TOption>(this CombinedTargetContainerConfig config, Type serviceType, Func<ITargetContainer, Type, TOption> optionValueFactory)
            where TOption : class
        {
            return ConfigureOptionInternal(
                config ?? throw new ArgumentNullException(nameof(config)),
                optionValueFactory ?? throw new ArgumentNullException(nameof(optionValueFactory)),
                serviceType);
        }

        internal static CombinedTargetContainerConfig ConfigureOptionInternal<TOption>(CombinedTargetContainerConfig config, TOption optionValue, Type serviceType = null)
            where TOption : class
        {
            config.Add(new Configuration.Configure<TOption>(optionValue, serviceType));
            return config;
        }

        internal static CombinedTargetContainerConfig ConfigureOptionInternal<TOption>(CombinedTargetContainerConfig config, Func<ITargetContainer, TOption> optionValueFactory, Type serviceType = null)
            where TOption : class
        {
            config.Add(new Configuration.Configure<TOption>((tc, t) => optionValueFactory(tc), serviceType));
            return config;
        }

        internal static CombinedTargetContainerConfig ConfigureOptionInternal<TOption>(CombinedTargetContainerConfig config, Func<ITargetContainer, Type, TOption> optionValueFactory, Type serviceType = null)
            where TOption : class
        {
            config.Add(new Configuration.Configure<TOption>(optionValueFactory, serviceType));
            return config;
        }
    }
}
