using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    // THIS IS A SKETCH OF THE TARGET CONTAINER FACTORY STUFF TO ALLOW CREATION OF 
    // TARGET CONTAINERS BASED ON CONFIGURATION

    public interface ITargetContainerBuilder
    {
        ///// <summary>
        ///// Registers a callback to create a new instance of the <typeparamref name="TConfig"/> options type.
        ///// </summary>
        ///// <typeparam name="TConfig"></typeparam>
        ///// <param name="configFactory"></param>
        //void UseConfig<TConfig>(Func<TConfig> configFactory)
        //    where TConfig : TargetContainerOptions;

        /// <summary>
        /// Registers a callback to configure a new container options instance
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="configureCallback"></param>
        void Configure<TConfig>(Func<TConfig, TConfig> configureCallback)
            where TConfig : TargetContainerConfig;

        /// <summary>
        /// ForConfig
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="containerFactory"></param>
        void ForConfigUse<TConfig>(Func<TConfig, ITargetContainer> containerFactory)
            where TConfig : TargetContainerConfig;

        TConfig BuildConfig<TConfig>(TConfig config = null)
            where TConfig : TargetContainerConfig, new();

        /// <summary>
        /// Creates a container for the given configuration type.  If <paramref name="config"/> is passed, then
        /// that will be passed to the target container.  If it's not, then a new instance will be created
        /// and optionally configured via any call backs previously registered through 
        /// <see cref="Configure{TConfig}(Func{TConfig, TConfig})"/>
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        ITargetContainer CreateContainer<TConfig>(TConfig config = null)
            where TConfig : TargetContainerConfig, new();
    }

    public class TargetContainerBuilder : ITargetContainerBuilder
    {
        readonly ITargetContainerBuilder _parent;
        private readonly Dictionary<Type, Delegate> _containerFactory;
        private readonly Dictionary<Type, Delegate> _configureCallbacks;
        public TargetContainerBuilder(ITargetContainerBuilder parent = null)
        {
            _parent = parent;
            _containerFactory = new Dictionary<Type, Delegate>();
            _configureCallbacks = new Dictionary<Type, Delegate>();
        }
  
        public void ForConfigUse<TConfig>(Func<TConfig, ITargetContainer> factory)
            where TConfig : TargetContainerConfig
        {
            if (_containerFactory.ContainsKey(typeof(TConfig)))
                throw new ArgumentException($"A factory has already been registered for the configuration type { typeof(TConfig) }");
            _containerFactory[typeof(TConfig)] = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void Configure<TConfig>(Func<TConfig, TConfig> configure)
            where TConfig : TargetContainerConfig
        {
            if (_configureCallbacks.ContainsKey(typeof(TConfig)))
                throw new ArgumentException($"A configuration callback has already been registered for the configuration type { typeof(TConfig) }");

            _configureCallbacks[typeof(TConfig)] = configure;
        }

        public TConfig BuildConfig<TConfig>(TConfig config = null)
            where TConfig : TargetContainerConfig, new()
        {
            if (config == null)
                config = new TConfig();

            Delegate callBack = null;

            if (_parent != null)
                _parent.BuildConfig(config);

            // auto assign the builder to a new one which uses this one as a fallback.
            config.Builder = new TargetContainerBuilder(this);

            if (_configureCallbacks.TryGetValue(typeof(TConfig), out callBack))
                config = ((Func<TConfig, TConfig>)callBack)(config);

            return config;
        }

        public ITargetContainer CreateContainer<TOptions>(TOptions options = null)
            where TOptions : TargetContainerConfig, new()
        {
            if (options == null)
                options = BuildConfig<TOptions>();

            Delegate factory;
            if(_containerFactory.TryGetValue(typeof(TOptions), out factory))
                return ((Func<TOptions, ITargetContainer>)factory)(options);
            else
                throw new InvalidOperationException($"The registration for the target container options type { typeof(TOptions) } cannot be used to create a target container outside ");
        }
    }

    /// <summary>
    /// Common configuration for all target containers
    /// 
    /// The derived type determines the type of target container required.
    /// </summary>
    public class TargetContainerConfig
    {
        /// <summary>
        /// The behaviour to be used by any target container that attaches to this configuration
        /// 
        /// Initialised to <see cref="GlobalBehaviours.TargetContainerBehaviour"/> by default.
        /// </summary>
        public ITargetContainerBehaviour Behaviour { get; set; } = GlobalBehaviours.TargetContainerBehaviour;

        /// <summary>
        /// Used to create target containers for the different registrations that can be added to the
        /// container that uses this configuration.
        /// </summary>
        public ITargetContainerBuilder Builder { get; set; }

        public virtual void Validate()
        {
            if (Builder == null) throw new InvalidOperationException("Configuration must not be null");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    public class TargetContainerConfig<TConfig>
        where TConfig : TargetContainerConfig, new()
    {
        private static Func<TConfig> _default = () => new TConfig();
        /// <summary>
        /// Gets the default factory for the configuration type <typeparamref name="TConfig" />
        /// </summary>
        public static Func<TConfig> Default {
            get => _default;
            set
            {
                _default = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }

    /// <summary>
    /// Options for a target container which is stored within another
    /// </summary>
    public class ChildTargetContainerConfig : TargetContainerConfig
    {
        /// <summary>
        /// Any target container belonging to another will have a type associated with it.
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Target container to which the new child target container belongs
        /// </summary>
        public ITargetContainer Parent { get; set; }

        public override void Validate()
        {
            base.Validate();
            if (Type == null) throw new InvalidOperationException("Type must not be null");
        }
    }

    /// <summary>
    /// Configuration for a target container that will contain zero or more other targets
    /// all grouped by the same <see cref="ChildTargetContainerConfig.Type"/>.
    /// 
    /// By default the <see cref="TargetListContainer"/> is mapped to this configuration type via
    /// the default builder (accessible via the <see cref="TargetContainerConfig.Builder"/>) property).
    /// </summary>
    public class TargetListConfig : ChildTargetContainerConfig
    {
        /// <summary>
        /// A boolean indicating whether a target container using this configuration should prevent
        /// multiple targets from being registered against the common <see cref="ChildTargetContainerConfig.Type"/>
        /// </summary>
        public bool DisableMultiple { get; set; } = false;
    }

    /// <summary>
    /// Indicates an intent to create an ITargetContainer that can store multiple lists of targets
    /// of non-related types.  This is typically the root of most target container 'trees'
    /// </summary>
    public class TargetDictionaryConfig : TargetContainerConfig { }

    /// <summary>
    /// Indicates an intent to create an ITargetContainer that is bound to a specific open generic type
    /// </summary>
    public class GenericTargetContainerConfig : ChildTargetContainerConfig
    {
        /// <summary>
        /// Enables matching interface or delegate registrations when a matching type parameter is covariant and
        /// a registration for a closed generic type uses a type argument for the same parameter which is derived 
        /// from, or which implements the type passed.
        /// 
        /// Note - this feature is currently unsupported until a working design can be achieved
        /// </summary>
        internal bool EnableCovariance { get; set; } = false;

        /// <summary>
        /// Enables matching interface or delegate registrations where a matching type parameter is contravariant
        /// and a registration for a closed generic type uses a type argument for the same parameter which is a 
        /// base or interface of the type.
        /// </summary>
        public bool EnableContravariance { get; set; } = false;
        /// <summary>
        /// If true, then calling <see cref="ITargetContainer.FetchAll(Type)"/> with a method should return
        /// all targets registered against all possible matching combinations of 
        /// </summary>
        public bool AlwaysFetchAllMatchingTargets { get; set; } = true;

        public override void Validate()
        {
            if (EnableCovariance) throw new NotSupportedException("Covariance is not supported at this time");
            if (!TypeHelpers.IsGenericTypeDefinition(Type)) throw new InvalidOperationException("Type must be an open generic");
        }
    }

    // TODO: should be able to resolve an Action<Base> when requesting Action<Derived> (contravariance)
    // TODO: should be able to resolve a Func<Derived> when requesting Func<Base> (covariance)

    


}
