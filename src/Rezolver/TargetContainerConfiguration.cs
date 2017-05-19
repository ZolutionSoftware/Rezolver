using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    // THIS IS A SKETCH OF THE TARGET CONTAINER FACTORY STUFF TO ALLOW CREATION OF 
    // TARGET CONTAINERS BASED ON CONFIGURATION

    public interface ITargetContainerConfiguration
    {
        /// <summary>
        /// Registers a callback to create a new instance of the <typeparamref name="TOptions"/> options type.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="optionsFactory"></param>
        void RegisterOptionsFactory<TOptions>(Func<TOptions> optionsFactory)
            where TOptions : TargetContainerOptions;

        /// <summary>
        /// Registers a callback to configure a new container options instance
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="configure"></param>
        void ConfigureOptions<TOptions>(Func<TOptions, TOptions> configure)
            where TOptions : TargetContainerOptions;

        void RegisterTargetContainerFactory<TOptions>(Func<TOptions, ITargetContainer> containerFactory);

        /// <summary>
        /// A gets a (hopefully) new instance of the <typeparamref name="TOptions"/> options type
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <returns></returns>
        TOptions GetOptions<TOptions>()
            where TOptions : TargetContainerOptions, new();

        /// <summary>
        /// Creates a container for the given options
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        ITargetContainer CreateContainer<TOptions>(TOptions options = null)
            where TOptions : TargetContainerOptions, new();
    }

    public class TargetContainerConfiguration : ITargetContainerConfiguration
    {
        readonly ITargetContainer _root;
        public TargetContainerConfiguration(ITargetContainer root)
        {
            _root = root;
        }
        public void RegisterOptionsFactory<TOptions>(Func<TOptions> optionsFactory)
            where TOptions : TargetContainerOptions
        {
            _root.RegisterObject(optionsFactory);
        }

        public void RegisterTargetContainerFactory<TOptions>(Func<TOptions, ITargetContainer> factory)
        {
            _root.RegisterObject(factory);
        }

        public void ConfigureOptions<TOptions>(Func<TOptions, TOptions> configure)
            where TOptions : TargetContainerOptions
        {
            _root.RegisterObject(configure);
        }

        public TOptions GetOptions<TOptions>()
            where TOptions : TargetContainerOptions, new()
        {
            TOptions toReturn = null;
            var matching = _root.Fetch(typeof(Func<TOptions>));
            if (matching is IDirectTarget matchingObject)
                toReturn = ((Func<TOptions>)matchingObject.GetValue())();
            else
                toReturn = new TOptions();

            toReturn.Configuration = this;

            var configureTargets = _root.FetchAll(typeof(Func<TOptions, TOptions>));
            foreach (var configureCallback in configureTargets.OfType<IDirectTarget>())
            {
                toReturn = ((Func<TOptions, TOptions>)configureCallback.GetValue())(toReturn);
            }

            return toReturn;
        }

        public ITargetContainer CreateContainer<TOptions>(TOptions options = null)
            where TOptions : TargetContainerOptions, new()
        {
            if (options == null)
                options = GetOptions<TOptions>();

            options.Configuration = this;

            var factory = _root.Fetch(typeof(Func<TOptions, ITargetContainer>));
            if (factory == null) throw new ArgumentException($"No target container type has been associated with the options type { typeof(TOptions) }", nameof(options));

            if (factory is IDirectTarget directFactory)
                return ((Func<TOptions, ITargetContainer>)directFactory.GetValue())(options);
            else
                throw new InvalidOperationException($"The registration for the target container options type { typeof(TOptions) } cannot be used to create a target container outside ");
        }
    }

    /// <summary>
    /// Common options for all target containers
    /// 
    /// The derived type determines the type of target container required.
    /// </summary>
    public class TargetContainerOptions
    {
        public ITargetContainerBehaviour Behaviour { get; set; }
        public ITargetContainerConfiguration Configuration { get; set; }

        public virtual void Validate()
        {
            if (Configuration == null) throw new InvalidOperationException("Configuration must not be null");
        }
    }

    /// <summary>
    /// Options for a target container which is stored within another
    /// </summary>
    public class ChildTargetContainerOptions : TargetContainerOptions
    {
        public Type Type { get; set; }

        public override void Validate()
        {
            base.Validate();
            if (Type == null) throw new InvalidOperationException("Type must not be null");
        }
    }

    /// <summary>
    /// Indicates an intent to create an ITargetContainer that can store a list of targets
    /// by the same type.
    /// </summary>
    public class TargetListOptions : ChildTargetContainerOptions
    {
        public bool DisableMultiple { get; set; } = false;
    }

    /// <summary>
    /// Indicates an intent to create an ITargetContainer that can store multiple lists of targets
    /// of non-related types.  This is typically the root of most target container 'trees'
    /// </summary>
    public class TargetDictionaryOptions : TargetContainerOptions { }

    /// <summary>
    /// Indicates an intent to create an ITargetContainer that is bound to a specific open generic type
    /// </summary>
    public class GenericTargetContainerOptions : ChildTargetContainerOptions
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
