using Rezolver.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Abstract base class for an <see cref="ITargetContainerConfig"/> that is dependent upon a particular type of option
    /// having been set in an <see cref="ITargetContainer"/> before being able to <see cref="Configure(ITargetContainer)"/>
    /// that target container.
    /// </summary>
    /// <typeparam name="TOption">The type of option upon which this config object depends.  The default dependency created and 
    /// returned by this class' implementation of <see cref="Dependencies"/> will actually be dependent upon the config type 
    /// <see cref="ITargetContainerConfig{TOption}"/>, which is the standard contract expected by a configuration object that configures
    /// a aprticular option.</typeparam>
    /// <remarks>If you are developing an <see cref="ITargetContainerConfig"/> that you want to be configurable via the use of 
    /// a single option type (see the extension methods in the <see cref="OptionsTargetContainerExtensions"/> class), then inheriting from this
    /// abstract class instead of directly implementing <see cref="ITargetContainerConfig"/> and <see cref="IDependant"/> is a good idea.
    /// 
    /// The default implementation of <see cref="Dependencies"/> will return a single dependency on the type <see cref="ITargetContainerConfig{TOption}"/>
    /// specialised for the option type <typeparamref name="TOption"/>.
    /// 
    /// You can also specify that the dependency is not *required* in cases where an option has a reasonable default value - thus allowing
    /// applications to omit any up-front configuration for that option except where absolutely necessary.
    /// 
    /// 
    /// The <see cref="InjectEnumerables"/> config inherits from this class - passing <see cref="Options.EnableEnumerableInjection"/> as 
    /// <typeparamref name="TOption"/>, with the constructor marking the dependency as optional. This ensures that it is executed after the option
    /// has been configured by any <see cref="ITargetContainerConfig{T}"/> objects specialised for the option type.</remarks>
    public abstract class OptionDependentConfig<TOption> : ITargetContainerConfig, IDependant
        where TOption : class
    {
        private readonly DependencyMetadata[] _baseDependencies;

        /// <summary>
        /// The base implementation returns an enumerable containing a single dependency on the type <see cref="ITargetContainerConfig{T}"/>
        /// specialised for the type <typeparamref name="TOption" />.
        /// </summary>
        public virtual IEnumerable<DependencyMetadata> Dependencies => _baseDependencies;

        /// <summary>
        /// Constructs a new instance of the type <see cref="OptionDependentConfig{TOption}"/> which starts off with a required or optional
        /// dependency (controlled by the argument passed <paramref name="optionConfigurationRequired"/> parameter)
        /// </summary>
        /// <param name="optionConfigurationRequired"></param>
        public OptionDependentConfig(bool optionConfigurationRequired)
        {
            _baseDependencies = new[] { this.CreateTypeDependency<Configure<TOption>>(optionConfigurationRequired) };
        }

        /// <summary>
        /// Abstract implementation of the <see cref="ITargetContainerConfig"/> interface
        /// </summary>
        /// <param name="targets"></param>
        public abstract void Configure(ITargetContainer targets);
    }

    /// <summary>
    /// Extension to the <see cref="OptionDependentConfig{TOption}"/> generic which can be used by config types which also want to target
    /// a specific type for configuration (<typeparamref name="T"/>)
    /// </summary>
    /// <typeparam name="T">The type of service/behaviour/option being configured</typeparam>
    /// <typeparam name="TOption">The type of option upon which this config object depends.  The default dependency created and 
    /// returned by this class' implementation of <see cref="IDependant.Dependencies"/> will actually be dependent upon the config type 
    /// <see cref="ITargetContainerConfig{TOption}"/>, which is the standard contract expected by a configuration object that configures
    /// a particular option.</typeparam>
    public abstract class OptionDependentConfig<T, TOption> : OptionDependentConfig<TOption>, ITargetContainerConfig<T>
        where TOption : class
    {
        /// <summary>
        /// Constructs a new instance of the type <see cref="OptionDependentConfig{T, TOption}"/> which starts off with a required or optional
        /// dependency (controlled by the argument passed <paramref name="optionConfigurationRequired"/> parameter)
        /// </summary>
        /// <param name="optionConfigurationRequired"></param>
        public OptionDependentConfig(bool optionConfigurationRequired)
            : base(optionConfigurationRequired)
        {

        }
    }
}
