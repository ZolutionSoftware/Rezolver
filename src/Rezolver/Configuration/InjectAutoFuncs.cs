using Rezolver.Options;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Controls whether the injection of <see cref="Func{TResult}"/> will automatically be
    /// available *without* having to use the <see cref="AutoFactoryRegistrationExtensions.RegisterAutoFunc{TResult}(IRootTargetContainer)"/>
    /// method or explicitly register <see cref="AutoFactoryTarget"/> targets.
    /// </summary>
    /// <remarks>
    /// If this is applied to an <see cref="IRootTargetContainer"/> and the <see cref="Options.EnableAutoFuncInjection"/>
    /// option has been configured to be <c>true</c>, then whenever a target is registered against a particular service type,
    /// a second registration will automatically be made against a <see cref="Func{TResult}"/> type with TResult equal
    /// to the registered service type.</remarks>
    public class InjectAutoFuncs : OptionDependentConfig<Options.EnableAutoFuncInjection>
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

        public static InjectAutoFuncs Instance { get; } = new InjectAutoFuncs();

        private InjectAutoFuncs() : base(false)
        {
        }

        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption(EnableAutoFuncInjection.Default))
                return;

            targets.TargetRegistered += Targets_TargetRegistered;
        }

        private void Targets_TargetRegistered(object sender, Events.TargetRegisteredEventArgs e)
        {
            if (e.Target is AutoFactoryTarget ||
                (TypeHelpers.IsAssignableFrom(typeof(Delegate), e.Type)
                && TypeHelpers.IsGenericType(e.Type)
                && ((TypeHelpers.IsGenericTypeDefinition(e.Type) && FuncTypes.Contains(e.Type))
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
                root.Register(new AutoFactoryTarget(funcType, e.Type, TypeHelpers.EmptyTypes));
            }
        }
    }
}
