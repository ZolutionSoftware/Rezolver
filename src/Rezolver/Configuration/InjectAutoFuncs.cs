// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Options;
using Rezolver.Sdk;
using Rezolver.Targets;
using System;
using System.Collections.Generic;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Controls whether the injection of <see cref="Func{TResult}"/> will automatically be
    /// available *without* having to use the <see cref="AutoFactoryRegistrationExtensions.RegisterAutoFunc{TResult}(IRootTargetContainer)"/>
    /// method or explicitly register <see cref="AutoFactoryTarget"/> targets.
    /// </summary>
    /// <remarks>
    /// If this is applied to an <see cref="IRootTargetContainer"/> and the <see cref="Options.EnableAutoFuncInjection"/>
    /// option has been configured to be <c>true</c> (**NOTE**: The default is <c>false</c>), then whenever a target is 
    /// registered against a particular service type, a second registration will automatically be made against 
    /// a <see cref="Func{TResult}"/> type with TResult equal to the registered service type.
    /// 
    /// Also, if the <see cref="Options.EnableEnumerableInjection"/> option is <c>true</c>, then an additional registration
    /// will be made for the same type for <see cref="Func{TResult}"/> where TResult is equal to <see cref="IEnumerable{T}"/>
    /// specialised for the registered type.</remarks>
    public class InjectAutoFuncs : OptionDependentConfigBase
    {
        private static readonly HashSet<Type> FuncTypes = new HashSet<Type>(new[]
        {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>),
            typeof(Func<,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,,>)
        });

        /// <summary>
        /// The one and only <see cref="InjectAutoFuncs"/> instance.
        /// </summary>
        public static InjectAutoFuncs Instance { get; } = new InjectAutoFuncs();

        private InjectAutoFuncs() 
        {
        }

        /// <summary>
        /// Called to apply this configuration to the passed <paramref name="targets"/> target container.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption(EnableAutoFuncInjection.Default))
                return;

            bool enableEnumerables = targets.GetOption(EnableEnumerableInjection.Default);

            targets.TargetRegistered += (object sender, Events.TargetRegisteredEventArgs e) =>
            {
                if (e.Target is AutoFactoryTarget ||
                    (typeof(Delegate).IsAssignableFrom(e.Type)
                    && e.Type.IsGenericType
                    && ((e.Type.IsGenericTypeDefinition && FuncTypes.Contains(e.Type))
                        || FuncTypes.Contains(e.Type.GetGenericTypeDefinition()))))
                {
                    return;
                }

                IRootTargetContainer root = (IRootTargetContainer)sender;
                var funcType = typeof(Func<>).MakeGenericType(e.Type);
                var existing = root.Fetch(funcType);

                if (existing == null || existing.UseFallback)
                {
                    // you'd think we would bind to the target that was registered, but we don't because
                    // that would prevent auto IEnumerable<delegate_type> from working, and would also prevent
                    // decorators from working.
                    root.Register(new AutoFactoryTarget(funcType, e.Type, Type.EmptyTypes));
                }

                if (enableEnumerables)
                {
                    var enumerableType = typeof(IEnumerable<>).MakeGenericType(e.Type);
                    funcType = typeof(Func<>).MakeGenericType(enumerableType);
                    existing = root.Fetch(funcType);
                    if(existing == null || existing.UseFallback)
                    {
                        root.Register(new AutoFactoryTarget(funcType, enumerableType, Type.EmptyTypes));
                    }
                }
            };
        }

        /// <summary>
        /// Overrides <see cref="OptionDependentConfigBase.GetDependenciesBase"/> by returning dependencies on:
        /// 
        /// - <see cref="EnableAutoFuncInjection"/> (via <see cref="Configure{TOption}"/>)
        /// - <see cref="EnableEnumerableInjection"/> (also via <see cref="Configure{TOption}"/>)
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<DependencyMetadata> GetDependenciesBase()
        {
            // we also support enumerable injection...
            return new[] 
            {
                CreateOptionDependency<EnableAutoFuncInjection>(false),
                CreateOptionDependency<EnableEnumerableInjection>(false)
            };
        }
    }
}
