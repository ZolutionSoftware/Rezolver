// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using Rezolver.Options;

namespace Rezolver
{
    public static partial class TargetContainerExtensions
    {
        /// <summary>
        /// Sets the passed <paramref name="option"/> into the <paramref name="targets"/> target container.
        ///
        /// The value can later be retrieved through a call to <see cref="GetOption{TOption}(ITargetContainer, TOption)"/>
        /// or one of its overloads.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="targets">The target container into which the option will be set.</param>
        /// <param name="option">The option value to be set</param>
        /// <returns>The target container on which the method is called, to enable method chaining.</returns>
        public static ITargetContainer SetOption<TOption>(this ITargetContainer targets, TOption option)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            targets.Register(new OptionContainer<TOption>(option), typeof(IOptionContainer<TOption>));
            return targets;
        }

        /// <summary>
        /// Sets the passed <paramref name="option"/> into the <paramref name="targets"/> target container, associating
        /// it with the given <paramref name="serviceType"/>.
        ///
        /// The value can later be retrieved through a call to <see cref="GetOption{TOption, TService}(ITargetContainer, TOption)"/>
        /// or <see cref="GetOption{TOption}(ITargetContainer, Type, TOption)"/>, passing the same type, or a derived type.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="targets">The target container into which the option will be set.</param>
        /// <param name="option">The option value to be set</param>
        /// <param name="serviceType">The type against which the option is to be set.  It's called 'serviceType' because the majority
        /// of the time, you will used this method and its generic overload to customise behaviour for specific types.  If <c>null</c>,
        /// then it's equivalent to calling <see cref="SetOption{TOption}(ITargetContainer, TOption)"/>.</param>
        /// <returns>The target container on which the method is called, to enable method chaining.</returns>
        public static ITargetContainer SetOption<TOption>(this ITargetContainer targets, TOption option, Type serviceType)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            if (serviceType == null)
            {
                return SetOption<TOption>(targets, option);
            }

            // this is cheeky - we create an instance of OptionContainer<TOption> to service
            // the type OptionContainer<TService, TOption>.  When we retrieve it in the various GetOption
            // methods, we *expect* only an OptionContainer<TOption> despite the fact that we fetch
            // a target for the <TService, TOption> pair.  That's part of the reason why all this stuff is
            // internal.
            targets.Register(new OptionContainer<TOption>(option),
                typeof(IOptionContainer<,>).MakeGenericType(serviceType, typeof(TOption)));

            return targets;
        }

        /// <summary>
        /// Sets the passed <paramref name="option"/> into the <paramref name="targets"/> target container, associating
        /// it with the given <typeparamref name="TService"/>.
        ///
        /// The value can later be retrieved through a call to <see cref="GetOption{TOption, TService}(ITargetContainer, TOption)"/>
        /// or <see cref="GetOption{TOption}(ITargetContainer, Type, TOption)"/>, passing the same type, or a derived type.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <typeparam name="TService">The type against which the option is to be set.</typeparam>
        /// <param name="targets">The target container into which the option will be set.</param>
        /// <param name="option">The option value to be set</param>
        /// <returns>The target container on which the method is called, to enable method chaining.</returns>
        public static ITargetContainer SetOption<TOption, TService>(this ITargetContainer targets, TOption option)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            // see long comment in method above
            targets.Register(new OptionContainer<TOption>(option),
                typeof(IOptionContainer<TService, TOption>));

            return targets;
        }

        /// <summary>
        /// Sets an option which will apply to any service type which is generic (either closed or open).
        ///
        /// This is a *very* special case which is reserved only for internal functionality at the moment.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be set.</typeparam>
        /// <param name="targets">The target container into which the option is to be set.</param>
        /// <param name="option">The option value to be set</param>
        /// <returns>The target container on which the method is called, to enabled method chaining.</returns>
        internal static ITargetContainer SetGenericServiceOption<TOption>(this ITargetContainer targets, TOption option)
            where TOption: class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            targets.Register(new OptionContainer<TOption>(option),
                typeof(IAnyGenericOptionContainer<TOption>));

            return targets;
        }

        /// <summary>
        /// Gets a globally-defined option of the type <typeparamref name="TOption"/> from the <paramref name="targets"/> target container,
        /// returning the <paramref name="default"/> if the option has not been explicitly set.
        /// </summary>
        /// <typeparam name="TOption">The type of option to retrieve</typeparam>
        /// <param name="targets">Required. The target container from which the option is to be read.</param>
        /// <param name="default">The default value to return if the option has not been set.</param>
        /// <returns>An option value which was either previously set, or the <paramref name="default"/> if not</returns>
        public static TOption GetOption<TOption>(this ITargetContainer targets, TOption @default = default)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            var optionContainer = targets.FetchDirect<IOptionContainer<TOption>>();
            return optionContainer?.Option ?? @default;
        }

        /// <summary>
        /// Gets an option either specific to the <paramref name="serviceType"/>, or a global option (if <see cref="Options.EnableGlobalOptions"/> is
        /// enabled), of the type <typeparamref name="TOption"/>
        /// from the <paramref name="targets"/> target container, returning the <paramref name="default"/> if the option has not been explicitly set.
        /// </summary>
        /// <typeparam name="TOption">The type of option to retrieve</typeparam>
        /// <param name="targets">Required. The target container from which the option is to be read.</param>
        /// <param name="serviceType">A type for which the option is to be retrieved.  Note that the default behaviour is to search for
        /// an option which is specific to this service, and then to search for more generally-defined options.  See the remarks section for more.</param>
        /// <param name="default">The default value to return if the option has not been set.</param>
        /// <returns>An option value which was either previously set, or the <paramref name="default"/> if not</returns>
        /// <remarks>Options are frequently used to control how a Rezolver container interprets registrations.  Take, for example, the
        /// <see cref="Options.AllowMultiple"/> option - which is used to control whether a target container accepts multiple registrations
        /// for a given type.
        ///
        /// When defined globally (i.e. without a service type) it determines whether multiple registrations can be performed for all types.  However,
        /// it can also be defined on a per-service basis - so, for example, if you want to restrict an application only to register one target for a
        /// particular service - e.g. <c>IMyApplication</c> - then you can set the <see cref="AllowMultiple"/> option to <c>false</c> specifically against
        /// that type, and multiple registrations will result in a runtime error.
        ///
        /// When searching for service-specific options, generics are automatically processed in descending order of specificity - i.e. <c>IFoo&lt;Bar&gt;</c>
        /// is more specific than <c>IFoo&lt;&gt;</c> - so you can set options for a specific closed generic, or its open generic.
        ///
        /// **Global Fallback**
        ///
        /// In the absence of a service-specific option, a globally-defined option will instead be used if the <see cref="EnableGlobalOptions"/> option
        /// is set to <c>true</c> for the <paramref name="targets"/> target container.  By default, this is enabled.</remarks>
        public static TOption GetOption<TOption>(this ITargetContainer targets, Type serviceType, TOption @default = default)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            bool useGlobalFallback = GetOption(targets, EnableGlobalOptions.Default);

            var optionContainer = (IOptionContainer<TOption>)targets.FetchDirect(typeof(IOptionContainer<,>)
                .MakeGenericType(serviceType, typeof(TOption)));

            // currently internal-only option container which allows us to set options
            // that take effect only for generic types (closed/open classes, structs or interfaces)
            if (optionContainer == null && serviceType.IsGenericType)
            {
                optionContainer = targets.FetchDirect<IAnyGenericOptionContainer<TOption>>();
            }

            if (optionContainer == null && useGlobalFallback)
            {
                optionContainer = targets.FetchDirect<IOptionContainer<TOption>>();
            }

            return optionContainer?.Option ?? @default;
        }

        /// <summary>
        /// Generic equivalent of <see cref="GetOption{TOption}(ITargetContainer, Type, TOption)"/>.  See documentation on that method for more.
        /// </summary>
        /// <typeparam name="TOption">The type of option to retrieve</typeparam>
        /// <typeparam name="TService">The service type for which the option is to be retrieved</typeparam>
        /// <param name="targets">That target container from which the option is to be read.</param>
        /// <param name="default">The default value to be returned if the option is not set.</param>
        /// <returns>An option value which was either previously set, or the <paramref name="default"/> if not</returns>
        public static TOption GetOption<TOption, TService>(this ITargetContainer targets, TOption @default = default)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return GetOption(targets, typeof(TService), @default);
        }

        /// <summary>
        /// Gets all globally-defined options of the type <typeparamref name="TOption"/> from the <paramref name="targets"/> target container,
        /// returning an empty enumerable if none have been set.
        /// </summary>
        /// <typeparam name="TOption">The type of option to retrieve</typeparam>
        /// <param name="targets">Required. The target container from which the options are to be read.</param>
        /// <returns>An enumerable of the type <typeparamref name="TOption"/> containing zero or more options that have been
        /// set.</returns>
        public static IEnumerable<TOption> GetOptions<TOption>(this ITargetContainer targets)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return targets.FetchAllDirect<IOptionContainer<TOption>>().Select(o => o.Option);
        }

        /// <summary>
        /// Get all options of the type <typeparamref name="TOption"/> which have been set for the service type <paramref name="serviceType"/>
        /// or any of its derivatives.  Globally-defined options will also be included in the results unless the <see cref="EnableGlobalOptions"/>
        /// option has been set to <c>false</c> on the <paramref name="targets"/> target container.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be retrieved</typeparam>
        /// <param name="targets">Required.  The target container from which the options are to be read.</param>
        /// <param name="serviceType">Required.  The service type for which options are to be retrieved.</param>
        /// <returns>An enumerable of the type <typeparamref name="TOption"/> containing zero or more options that have been
        /// set.</returns>
        public static IEnumerable<TOption> GetOptions<TOption>(this ITargetContainer targets, Type serviceType)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            // global fallback in this instance causes all options - service-specific or not - to be included
            // in the enumerable.
            bool useGlobalFallback = GetOption(targets, EnableGlobalOptions.Default);

            var optionContainers = targets.FetchAllDirect(typeof(IOptionContainer<,>)
                .MakeGenericType(serviceType, typeof(TOption))).Cast<IOptionContainer<TOption>>();

            if (useGlobalFallback)
            {
                optionContainers = optionContainers.Concat(targets.FetchAllDirect<IOptionContainer<TOption>>());
            }

            return optionContainers.Select(o => o.Option);
        }

        /// <summary>
        /// Generic version of the <see cref="GetOptions{TOption}(ITargetContainer, Type)"/> method.  See the documentation on that method
        /// for more.
        /// </summary>
        /// <typeparam name="TOption">The type of option to be retrieved</typeparam>
        /// <typeparam name="TService">The service type for which options are to be retrieved.</typeparam>
        /// <param name="targets">Required.  The target container from which the options are to be read.</param>
        /// <returns>An enumerable of the type <typeparamref name="TOption"/> containing zero or more options that have been
        /// set.</returns>
        public static IEnumerable<TOption> GetOptions<TOption, TService>(this ITargetContainer targets)
            where TOption : class
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return GetOptions<TOption>(targets, typeof(TService));
        }
    }
}
