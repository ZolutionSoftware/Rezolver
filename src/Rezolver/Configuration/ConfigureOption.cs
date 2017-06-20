using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> implementation specialised for setting options in an <see cref="ITargetContainer"/> when
    /// <see cref="Configure(ITargetContainer)"/> is called.
    /// </summary>
    /// <typeparam name="TOption">The type of option to be set.  Will ultimately be passed through to the 
    /// <see cref="OptionsTargetContainerExtensions.SetOption{TOption}(ITargetContainer, Type, TOption)"/> method when the option is set.</typeparam>
    /// <remarks>This class is most often used to modify the options which are set in the <see cref="TargetContainer.DefaultConfig"/> of the 
    /// <see cref="TargetContainer"/> class - but can, of course, be used to configure any <see cref="ITargetContainer"/>.
    /// 
    /// The class implements the generic <see cref="ITargetContainerConfig{T}"/> interface, so that it's possible for other configuration
    /// objects to declare a (required or optional) dependency on anything which configures that option - so that it can ensure that those 
    /// options are set beforehand.</remarks>
    public class ConfigureOption<TOption> : ITargetContainerConfig<TOption>
        where TOption: class
    {
        Type ServiceType { get; set; }
        Func<ITargetContainer, Type, TOption> OptionFactory { get; set; }
        public ConfigureOption(TOption optionValue, Type serviceType = null)
        {
            if(optionValue == null) throw new ArgumentNullException(nameof(optionValue));
            OptionFactory = (tc, t) => optionValue;
            ServiceType = serviceType;
        }

        public ConfigureOption(Func<ITargetContainer, Type, TOption> optionFactory, Type serviceType = null)
        {
            OptionFactory = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));
            ServiceType = serviceType;
        }

        public void Configure(ITargetContainer targets)
        {
            targets.SetOption(ServiceType, OptionFactory(targets, ServiceType));
        }
    }
}
