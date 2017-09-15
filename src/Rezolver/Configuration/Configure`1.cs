using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> implementation specialised for setting options in an <see cref="ITargetContainer"/> when
    /// <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/> is called.
    /// 
    /// This class' implementation of that method is actually handled by the <see cref="ConfigureOption(ITargetContainer)"/> method.
    /// </summary>
    /// <typeparam name="TOption">The type of option to be set.  Will ultimately be passed through to the 
    /// <see cref="OptionsTargetContainerExtensions.SetOption{TOption}(ITargetContainer, TOption, Type)"/> method when the option is set.</typeparam>
    /// <remarks>This class is most often used to modify the options which are set in the <see cref="TargetContainer.DefaultConfig"/> of the 
    /// <see cref="TargetContainer"/> class - but can, of course, be used to configure any <see cref="ITargetContainer"/>.
    /// 
    /// The class implements the generic <see cref="ITargetContainerConfig{T}"/> interface, so that it's possible for other configuration
    /// objects to declare a (required or optional) dependency on anything which configures that option - so that it can ensure that those 
    /// options are set beforehand.
    /// 
    /// The <see cref="InjectEnumerables"/> configuration type, for example, expresses an optional dependency on an <see cref="ITargetContainerConfig{T}"/>
    /// which has a <typeparamref name="TOption"/> type equal to <see cref="Options.EnableEnumerableInjection"/>.</remarks>
    public class Configure<TOption> : ITargetContainerConfig<TOption>
        where TOption: class
    {
        Type ServiceType { get; set; }
        Func<ITargetContainer, Type, TOption> OptionFactory { get; set; }

        /// <summary>
        /// Constructs a new instance of the <see cref="Configure{TOption}"/> class which, when 
        /// <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/> is called with a particular <see cref="ITargetContainer"/>,
        /// will set the option to the <paramref name="optionValue"/>, optionally for the given <paramref name="serviceType"/>
        /// </summary>
        /// <param name="optionValue">The value to set the option to when the configuration is applied to the target container</param>
        /// <param name="serviceType">Optional - service type for which the option is to be set (use of this is option-dependent - not all options
        /// are read in a service-specific manner)</param>
        public Configure(TOption optionValue, Type serviceType = null)
        {
            if(optionValue == null) throw new ArgumentNullException(nameof(optionValue));
            OptionFactory = (tc, t) => optionValue;
            ServiceType = serviceType;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Configure{TOption}"/> class which, when 
        /// <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/> is called with a particular <see cref="ITargetContainer"/>,
        /// will set the option to the value returned by <paramref name="optionFactory"/>, optionally for the given <paramref name="serviceType"/>
        /// </summary>
        /// <param name="optionFactory">The factory to be executed to obtain the option value</param>
        /// <param name="serviceType">Optional - service type for which the option is to be set (use of this is option-dependent - not all options
        /// are read in a service-specific manner)</param>
        public Configure(Func<ITargetContainer, Type, TOption> optionFactory, Type serviceType = null)
        {
            OptionFactory = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));
            ServiceType = serviceType;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/> - uses the
        /// <see cref="ConfigureOption(ITargetContainer)"/> method.
        /// </summary>
        /// <param name="targets">The target container into which the option is to be set.</param>
        void ITargetContainerConfig.Configure(ITargetContainer targets)
        {
            // note - the explicit implementation is required because 
            // otherwise the method name is the same as the enclosing class
            ConfigureOption(targets);
        }

        /// <summary>
        /// Sets the option value (either passed as a constant
        /// reference on construction, or obtained via a callback) in the <paramref name="targets"/> target container.
        /// 
        /// This is used as the implementation of <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/>.
        /// </summary>
        /// <param name="targets">The target container into which the option is to be set.</param>
        public void ConfigureOption(ITargetContainer targets)
        {
            targets.SetOption(OptionFactory(targets, ServiceType), ServiceType);
        }
    }
}
