using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    // THIS IS A SKETCH OF THE TARGET CONTAINER FACTORY STUFF TO ALLOW CREATION OF 
    // TARGET CONTAINERS BASED ON CONFIGURATION

    /// <summary>
    /// Represents an intent to create an ITargetContainer for a specific purpose.
    /// 
    /// The derived type determines the type of target container required.
    /// </summary>
    public class TargetContainerOptions
    {
        public ITargetContainer Owner { get; set; }

        public virtual void Validate()
        {

        }
    }

    /// <summary>
    /// Options for a target container 
    /// </summary>
    public class ChildTargetContainerOptions : TargetContainerOptions
    {
        public Type Type { get; set; }

        public override void Validate()
        {
            if (Type == null) throw new InvalidOperationException("Type must not be null");
            if (Owner == null) throw new InvalidOperationException("Owner must not be null");
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
    
    public static class ContainerOptionsTargetContainerExtensions
    {
        public static void RegisterContainerOptions<TOptions>(this ITargetContainer targets, Func<TOptions> optionsFactory)
            where TOptions : TargetContainerOptions
        {
            targets.RegisterObject(optionsFactory);
        }

        public static void ConfigureContainerOptions<TOptions>(this ITargetContainer targets, Func<TOptions, TOptions> configure)
            where TOptions: TargetContainerOptions
        {
            targets.RegisterObject(configure);
        }

        public static TOptions GetContainerOptions<TOptions>(this ITargetContainer targets)
            where TOptions : TargetContainerOptions, new()
        {
            TOptions toReturn = null;
            var matching = targets.Fetch(typeof(Func<TOptions>));
            if (matching is ObjectTarget matchingObject)
                toReturn = ((Func<TOptions>)matchingObject.Value)();
            else
                toReturn = new TOptions();

            var configureTargets = targets.FetchAll(typeof(Func<TOptions, TOptions>));
            foreach(var configureCallbackTarget in configureTargets)
            {
                if(configureCallbackTarget is ObjectTarget configureCallback)
                {
                    toReturn = ((Func<TOptions, TOptions>)configureCallback.Value)(toReturn);
                }
            }
            return toReturn;
        }

        public static ITargetContainer CreateContainer<TOptions>(this ITargetContainer targets, TOptions options = null)
            where TOptions : TargetContainerOptions, new()
        {
            if (options == null)
                options = GetContainerOptions<TOptions>(targets);

            throw new NotImplementedException();
        }
    }


    public class TargetContainerFactoryTests
    {
        
        [Fact]
        public void ShouldConfigureTargetListOptions()
        {
            var targets = new TargetContainer();
            targets.ConfigureContainerOptions<TargetListOptions>(c => {
                c.DisableMultiple = true;
                return c;
            });

            var result = targets.GetContainerOptions<TargetListOptions>();
            Assert.True(result.DisableMultiple);
        }

        [Fact]
        public void ShouldGetTargetListContainer()
        {
            var targets = new TargetContainer();
            targets.ConfigureContainerOptions<TargetListOptions>(c => {
                c.DisableMultiple = true;
                return c;
            });

            ITargetContainer list = targets.CreateContainer<TargetListOptions>();
        }
    }
}
