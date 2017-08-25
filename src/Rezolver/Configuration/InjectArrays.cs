using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;
using System.Linq;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Configuration (enabled by default in the <see cref="TargetContainer.DefaultConfig"/> configuration collection)
    /// which enables the automatic injection of arrays by converting automatically injected enumerables into array
    /// instances via the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> method from Linq.
    /// </summary>
    /// <remarks>Note that this configuration requires that the <see cref="InjectEnumerables"/> configuration is
    /// also applied.</remarks>
    public class InjectArrays : OptionDependentConfig<Options.EnableArrayInjection>
    {
        /// <summary>
        /// The one and only instance of the <see cref="InjectArrays"/> configuration object
        /// </summary>
        public static InjectArrays Instance { get; } = new InjectArrays();

        internal class ArrayTypeResolver : ITargetContainerTypeResolver
        {
            public Type GetContainerType(Type serviceType)
            {
                if (TypeHelpers.IsArray(serviceType))
                    return typeof(Array);
                return null;
            }
        }
        private IEnumerable<DependencyMetadata> _dependencies;

        /// <summary>
        /// Overrides the <see cref="OptionDependentConfig{TOption}"/> implementation to include a required dependency
        /// on the <see cref="InjectEnumerables"/> configuration.
        /// </summary>
        public override IEnumerable<DependencyMetadata> Dependencies => _dependencies;

        private InjectArrays() : base(false)
        {
            _dependencies = base.Dependencies.Concat(new[] { this.CreateTypeDependency<InjectEnumerables>(true) }).ToArray();
        }

        /// <summary>
        /// Implements the <see cref="OptionDependentConfig{TOption}.Configure(ITargetContainer)"/> abstract method
        /// by configuring the passed <paramref name="targets"/> so it can produce targets for any array type, regardless
        /// of whether a single object has been registered for the array's element type.
        /// 
        /// After enabling, the ability to register specific targets for concrete array types will still be present.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(ITargetContainer targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption(Options.EnableArrayInjection.Default))
                return;

            targets.RegisterContainer(typeof(Array), new ArrayTargetContainer(targets));
            targets.SetOption<ITargetContainerTypeResolver, Array>(new ArrayTypeResolver());
        }
    }
}
