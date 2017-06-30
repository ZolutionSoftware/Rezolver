using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class AutoListsConfigTests
    {
        [Fact]
        public void ShouldAddRegistrationsByDefault()
        {
            var configs = new CombinedTargetContainerConfig
            {
                Configuration.AutoLists.Instance
            };

            var targets = new TargetContainer(configs);

            var concreteTarget = targets.Fetch(typeof(List<string>));
            var interfaceTarget = targets.Fetch(typeof(IList<string>));
            Assert.NotNull(concreteTarget);
            Assert.False(concreteTarget.UseFallback);
            Assert.NotNull(interfaceTarget);
            Assert.False(interfaceTarget.UseFallback);
        }

        [Fact]
        public void ShouldNotAddRegistrationsIfDisabledByOption()
        {
            var configs = new CombinedTargetContainerConfig
            {
                Configuration.AutoLists.Instance,
                Configuration.ConfigureOption.With<Options.EnableAutoLists>(false)
            };

            var targets = new TargetContainer(configs);

            Assert.Null(targets.Fetch(typeof(List<string>)));
            Assert.Null(targets.Fetch(typeof(IList<string>)));
        }
    }
}
