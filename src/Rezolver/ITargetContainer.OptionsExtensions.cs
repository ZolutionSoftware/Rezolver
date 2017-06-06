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
    /// <remarks>Options are different to target container behaviours (<see cref="ITargetContainerBehaviour"/>) in that options
    /// are actively *read* by the various <see cref="ITargetContainer"/>-related types throughout the Rezolver framework to
    /// control how certain standard functionality operates.
    /// 
    /// The <see cref="ITargetContainerBehaviour"/>, however, can be used both to *configure* those options and to add extra
    /// registrations (both <see cref="ITarget"/> and, more commonly, other <see cref="ITargetContainer"/>s via the 
    /// <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> method).
    /// 
    /// The automatic building of <see cref="IEnumerable{T}"/> sequences from all the targets registered for a type, for example,
    /// is enabled by attaching the <see cref="Behaviours.AutoEnumerableBehaviour"/> to the target container.  Whereas, the 
    /// ability to actually register more than one target for a particular service in the first place is controlled by the 
    /// <see cref="Options.AllowMultiple"/> option.
    /// 
    /// ## Types of options
    /// 
    /// Ultimately an option can be of any type, but most of the built-in options use the <see cref="Options.ContainerOption{TOption}"/>
    /// type to wrap simple types (<see cref="bool"/>, <see cref="string"/>, <see cref="int"/> and so on) as a human-readably
    /// named type that differentiates that option from others of the same underlying value type.  Note that the phrase 'value type'
    /// there doesn't mean that all options must be literal value types (i.e. <see cref="ValueType"/>).</remarks>
    public static class OptionsTargetContainerExtensions
    {
        public static ITargetContainer SetOption<TOption>(this ITargetContainer targets, TOption option)
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (option == null) throw new ArgumentNullException(nameof(option));
            targets.Register(new OptionContainer<TOption>(option));
            return targets;
        }

        public static ITargetContainer SetOption<TOption>(this ITargetContainer targets, Type serviceType, TOption option)
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (option == null) throw new ArgumentNullException(nameof(option));

            // this is cheeky - we create an instance of OptionContainer<TOption> to service
            // the type OptionContainer<TService, TOption>.  When we retrieve it in the various GetOption
            // methods, we *expect* only an OptionContainer<TOption> despite the fact that we fetch
            // a target for the <TService, TOption> pair.  That's part of the reason why all this stuff is 
            // internal.
            targets.Register(new OptionContainer<TOption>(option), 
                typeof(OptionContainer<,>).MakeGenericType(serviceType, typeof(TOption)));
            
            return targets;
        }

        public static ITargetContainer SetOption<TOption, TService>(this ITargetContainer targets, TOption option)
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (option == null) throw new ArgumentNullException(nameof(option));

            // see long comment in method above
            targets.Register(new OptionContainer<TOption>(option),
                typeof(OptionContainer<TService, TOption>));

            return targets;
        }

        public static TOption GetOption<TOption>(this ITargetContainer targets, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            var optionContainer = targets.FetchDirect<OptionContainer<TOption>>();
            return optionContainer?.Option ?? @default;
        }

        public static TOption GetOption<TOption>(this ITargetContainer targets, Type serviceType, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            bool useGlobalFallback = GetOption(targets, EnableGlobalOptions.Default);

            var optionContainer = (OptionContainer<TOption>)targets.FetchDirect(typeof(OptionContainer<,>)
                .MakeGenericType(serviceType, typeof(TOption)));
            
            if(optionContainer == null && useGlobalFallback)
                optionContainer = targets.FetchDirect<OptionContainer<TOption>>();

            return optionContainer?.Option ?? @default;
        }

        public static TOption GetOption<TOption, TService>(this ITargetContainer targets, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            bool useGlobalFallback = GetOption(targets, EnableGlobalOptions.Default);

            var optionContainer = (OptionContainer<TOption>)targets.FetchDirect(typeof(OptionContainer<TService, TOption>));

            if(optionContainer == null && useGlobalFallback)
                optionContainer = targets.FetchDirect<OptionContainer<TOption>>();
            
            return optionContainer?.Option ?? @default;
        }
    }
}
