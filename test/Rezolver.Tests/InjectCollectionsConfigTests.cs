using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class InjectCollectionsConfigTests
    {
        // see also InjectionListsConfigTests

        [Fact]
        public void ShouldAddRegistrations()
        {
            CombinedTargetContainerConfig config = new CombinedTargetContainerConfig
            {
                Configuration.InjectCollections.Instance
            };

            var targets = new TargetContainer(config);

            var rwConcreteTarget = targets.Fetch(typeof(Collection<string>));
            var rwInterfaceTarget = targets.Fetch(typeof(ICollection<string>));
            var roConcreteTarget = targets.Fetch(typeof(ReadOnlyCollection<string>));
            var roInterfaceTarget = targets.Fetch(typeof(IReadOnlyCollection<string>));

            Assert.NotNull(rwConcreteTarget);
            Assert.NotNull(rwInterfaceTarget);
            Assert.NotNull(roConcreteTarget);
            Assert.NotNull(roInterfaceTarget);

            Assert.False(rwConcreteTarget.UseFallback);
            Assert.False(rwInterfaceTarget.UseFallback);
            Assert.False(roConcreteTarget.UseFallback);
            Assert.False(roInterfaceTarget.UseFallback);
        }

        [Fact]
        public void ShouldNotAddRegistrationsIfDisabledByOption()
        {
            // note input order reversed from the preferred: tests that InjectCollections config
            // expresses dependency on 
            CombinedTargetContainerConfig config = new CombinedTargetContainerConfig()
            {
                Configuration.InjectCollections.Instance,
                Configuration.ConfigureOption.With<Options.CollectionInjection>(false)
            };

            var targets = new TargetContainer(config);

            Assert.Null(targets.Fetch(typeof(Collection<string>)));
            Assert.Null(targets.Fetch(typeof(ICollection<string>)));
            Assert.Null(targets.Fetch(typeof(ReadOnlyCollection<string>)));
            Assert.Null(targets.Fetch(typeof(IReadOnlyCollection<string>)));
        }
    }
}
