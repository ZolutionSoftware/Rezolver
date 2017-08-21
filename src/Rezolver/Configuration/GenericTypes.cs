using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// This configuration, when applied to a compatible <see cref="ITargetContainer"/>
    /// (usually a <see cref="TargetContainer"/> or <see cref="OverridingTargetContainer"/>
    /// instance) enables all the documented functionality that is connected with generic
    /// type resolving across Rezolver.
    /// 
    /// This class has an optional dependency on the <see cref="Options.EnableGenericTypes"/>
    /// option - checking that it is equivalent to <c>true</c> before performing any configuration
    /// on the container passed to the <see cref="GenericTypes.Configure(ITargetContainer)"/> 
    /// override.
    /// </summary>
    public class GenericTypes : OptionDependentConfig<Options.EnableGenericTypes>
    {
        internal class GenericContainerTypeResolver : ITargetContainerTypeResolver
        {
            public Type GetContainerType(Type serviceType)
            {
                if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

                if (TypeHelpers.IsGenericType(serviceType) &&
                    !TypeHelpers.IsGenericTypeDefinition(serviceType))
                    return serviceType.GetGenericTypeDefinition();
                return null;
            }
        }

        internal class GenericTargetContainerFactory : ITargetContainerFactory
        {
            public ITargetContainer CreateContainer(Type serviceType, ITarget target, ITargetContainer targets, ITargetContainer rootTargets)
            {
                if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
                if (rootTargets == null) throw new ArgumentNullException(nameof(rootTargets));

                if (!TypeHelpers.IsGenericTypeDefinition(serviceType))
                    throw new ArgumentException($"The type { serviceType } must be an open generic", nameof(serviceType));
                return new GenericTargetContainer(rootTargets, serviceType);
            }
        }
        /// <summary>
        /// Default constructor - chains to the base class constructor setting an optional
        /// dependency on the <see cref="Options.EnableGenericTypes"/> option.
        /// </summary>
        public GenericTypes() : base(false)
        {
        }

        /// <summary>
        /// Implementation of the base <see cref="OptionDependentConfig{TOption}.Configure(ITargetContainer)"/>
        /// method.  If reading the <see cref="Options.EnableGenericTypes"/> option yields a
        /// result equivalent to <c>true</c>, then a set of further options are set which enables
        /// the host container to handle generics using the built-in handlers.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(ITargetContainer targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption(Options.EnableGenericTypes.Default))
                return;

            targets.SetOption(new GenericContainerTypeResolver());
            targets.SetGenericServiceOption(new GenericTargetContainerFactory());
        }
    }
}
