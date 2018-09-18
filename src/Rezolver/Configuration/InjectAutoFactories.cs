using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Configuration which enables the registration of automatically-produced Func-based factories for registered types.
    /// 
    /// Controlled by the <see cref="Options.EnableAutoFactoryInjection"/> option which is, by default, equivalent to <c>true</c>
    /// </summary>
    /// <remarks>
    /// **NOTE** This does not automatically enable factory injection for all types, instead it merely makes the functionality
    /// available via additional registration APIs.  <see cref="Func{TResult}"/> (and related types) injection is deliberately opt-in
    /// to </remarks>
    public class InjectAutoFactories : OptionDependentConfig<Options.EnableAutoFactoryInjection>
    {
        /// <summary>
        /// The one and only instance of <see cref="InjectAutoFactories"/>
        /// </summary>
        public static InjectAutoFactories Instance { get; } = new InjectAutoFactories();

        private static readonly Type[] FuncTypes =
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
        };

        /// <summary>
        /// Constructs a new instance of the <see cref="InjectAutoFactories"/> configuration class.
        /// </summary>
        private InjectAutoFactories() : base(false)
        {
        }

        /// <summary>
        /// Applies the configuration to the passed <paramref name="targets"/>
        /// 
        /// Specifically, this registers the <see cref="AutoFactoryTargetContainer"/> type for each of the 
        /// <see cref="Func{TResult}"/> delegate types (including those which support parameters)
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption(Options.EnableAutoFactoryInjection.Default))
                return;

            foreach(var type in FuncTypes)
            {
                targets.RegisterContainer(type, new AutoFactoryTargetContainer(targets, type));
            }
        }
    }
}
