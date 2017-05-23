using Rezolver.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
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

        public static ITargetContainer SetOption<TService, TOption>(this ITargetContainer targets, TOption option)
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

            bool useGlobalFallback = GetOption<PerServiceOptionGlobalFallback>(targets, true);

            var optionContainer = (OptionContainer<TOption>)targets.FetchDirect(typeof(OptionContainer<,>)
                .MakeGenericType(serviceType, typeof(TOption)));
            
            if(optionContainer == null && useGlobalFallback)
                optionContainer = targets.FetchDirect<OptionContainer<TOption>>();

            return optionContainer?.Option ?? @default;
        }

        public static TOption GetOption<TService, TOption>(this ITargetContainer targets, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            bool useGlobalFallback = GetOption<PerServiceOptionGlobalFallback>(targets, true);

            var optionContainer = (OptionContainer<TOption>)targets.FetchDirect(typeof(OptionContainer<TService, TOption>));

            if(optionContainer == null && useGlobalFallback)
                optionContainer = targets.FetchDirect<OptionContainer<TOption>>();
            
            return optionContainer?.Option ?? @default;
        }
    }
}
