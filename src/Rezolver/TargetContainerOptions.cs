using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    public static class testextensions
    {
        internal class OptionContainer<TOption> : IDirectTarget, ITarget
        {
            public TOption Option { get; }

            bool ITarget.UseFallback => false;

            Type ITarget.DeclaredType => typeof(OptionContainer<TOption>);

            ScopeBehaviour ITarget.ScopeBehaviour => ScopeBehaviour.None;

            ScopePreference ITarget.ScopePreference => ScopePreference.Current;

            public OptionContainer(TOption option)
            {
                Option = option;
            }

            object IDirectTarget.GetValue() => Option;

            bool ITarget.SupportsType(Type type) => typeof(OptionContainer<TOption>) == type;
        }

        internal class OptionContainer<TService, TOption> : IDirectTarget
        {
            public TOption Option { get; }

            bool ITarget.UseFallback => false;

            Type ITarget.DeclaredType => typeof(OptionContainer<TService, TOption>);

            ScopeBehaviour ITarget.ScopeBehaviour => ScopeBehaviour.None;

            ScopePreference ITarget.ScopePreference => ScopePreference.Current;

            public OptionContainer(TOption option)
            {
                Option = option;
            }

            object IDirectTarget.GetValue() => Option;

            bool ITarget.SupportsType(Type type) => typeof(OptionContainer<TService, TOption>) == type;
        }

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

            ITarget optContainer = (ITarget)Activator.CreateInstance(typeof(OptionContainer<,>).MakeGenericType(serviceType, typeof(TOption)), option);
            targets.Register(optContainer);

            return targets;
        }

        public static ITargetContainer SetOption<TService, TOption>(this ITargetContainer targets, TOption option)
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (option == null) throw new ArgumentNullException(nameof(option));
            targets.Register(new OptionContainer<TService, TOption>(option));

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

            return default(TOption);
        }

        public static TOption GetOption<TService, TOption>(this ITargetContainer targets, TOption @default = default(TOption))
            where TOption : class
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            return GetOption<TOption>(targets, typeof(TService), @default);
        }
    }

    public class Option<T>
    {
        public T Value { get; protected set; }

        public static implicit operator T(Option<T> option)
        {
            return option != null ? option.Value : default(T);
        }
    }

    public class AllowMultiple : Option<bool>
    {
        public static implicit operator AllowMultiple(bool value)
        {
            return new AllowMultiple() { Value = value };
        }
    }
}
