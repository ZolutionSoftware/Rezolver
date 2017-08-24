using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;
using System.Linq;

namespace Rezolver.Configuration
{
    public class InjectArrays : OptionDependentConfig<Options.EnableArrayInjection>
    {
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
        public override IEnumerable<DependencyMetadata> Dependencies => _dependencies;

        private InjectArrays() : base(false)
        {
            _dependencies = base.Dependencies.Concat(new[] { this.CreateTypeDependency<InjectEnumerables>(false) });
        }

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
