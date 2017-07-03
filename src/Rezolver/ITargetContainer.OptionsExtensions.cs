using Rezolver.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Contains extension methods for getting and setting container options which are used to control the behaviour, chiefly,
    /// of the various well-known <see cref="ITargetContainer"/> implementations.
    /// </summary>
    /// <remarks>Options are different to target container behaviours (<see cref="ITargetContainerConfig"/>) in that options
    /// are actively *read* by the various <see cref="ITargetContainer"/>-related types throughout the Rezolver framework to
    /// control how certain standard functionality operates.
    /// 
    /// The <see cref="ITargetContainerConfig"/>, however, can be used both to *configure* those options and to add extra
    /// registrations (both <see cref="ITarget"/> and, more commonly, other <see cref="ITargetContainer"/>s via the 
    /// <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> method).
    /// 
    /// The automatic building of <see cref="IEnumerable{T}"/> sequences from all the targets registered for a type, for example,
    /// is enabled by attaching the <see cref="Configuration.InjectEnumerables"/> to the target container.  Whereas, the 
    /// ability to actually register more than one target for a particular service in the first place is controlled by the 
    /// <see cref="Options.AllowMultiple"/> option.
    /// 
    /// ## Types of options
    /// 
    /// Ultimately an option can be of any type, but most of the built-in options use the <see cref="Options.ContainerOption{TOption}"/>
    /// type to wrap simple types (<see cref="bool"/>, <see cref="string"/>, <see cref="int"/> and so on) as a human-readably
    /// named type that differentiates that option from others of the same underlying value type.  Note that the phrase 'value type'
    /// there doesn't mean that all options must be literal value types (i.e. <see cref="ValueType"/>).
    /// 
    /// Rezolver has several built-in option types - including <see cref="Options.AllowMultiple"/>, <see cref="Options.EnumerableInjection"/>,
    /// <see cref="Options.EnableContravariance"/> plus many more.  These use the <see cref="Options.ContainerOption{TOption}"/> type to enable
    /// reading and writing simple boolean-like option values which switch behaviour on and off.
    /// 
    /// In addition, the <see cref="Targets.ConstructorTarget"/> and <see cref="Targets.GenericConstructorTarget"/> classes use the options 
    /// API to discover <see cref="IMemberBindingBehaviour"/> objects to use when deciding whether to bind properties and/or fields when 
    /// creating new objects.
    /// 
    /// So you really can store anything you want inside an option.</remarks>
    public static class OptionsTargetContainerExtensions
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
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (option == null) throw new ArgumentNullException(nameof(option));
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
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (option == null) throw new ArgumentNullException(nameof(option));

            if (serviceType == null) return SetOption<TOption>(targets, option);
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
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (option == null) throw new ArgumentNullException(nameof(option));

            // see long comment in method above
            targets.Register(new OptionContainer<TOption>(option),
                typeof(IOptionContainer<TService, TOption>));

            return targets;
        }

        public static TOption GetOption<TOption>(this ITargetContainer targets, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            var optionContainer = targets.FetchDirect<IOptionContainer<TOption>>();
            return optionContainer?.Option ?? @default;
        }

        public static TOption GetOption<TOption>(this ITargetContainer targets, Type serviceType, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            bool useGlobalFallback = GetOption(targets, EnableGlobalOptions.Default);

            var optionContainer = (IOptionContainer<TOption>)targets.FetchDirect(typeof(IOptionContainer<,>)
                .MakeGenericType(serviceType, typeof(TOption)));
            
            if(optionContainer == null && useGlobalFallback)
                optionContainer = targets.FetchDirect<IOptionContainer<TOption>>();

            return optionContainer?.Option ?? @default;
        }

        public static TOption GetOption<TOption, TService>(this ITargetContainer targets, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            bool useGlobalFallback = GetOption(targets, EnableGlobalOptions.Default);

            var optionContainer = (IOptionContainer<TOption>)targets.FetchDirect(typeof(IOptionContainer<TService, TOption>));

            if(optionContainer == null && useGlobalFallback)
                optionContainer = targets.FetchDirect<IOptionContainer<TOption>>();
            
            return optionContainer?.Option ?? @default;
        }
    }
}
