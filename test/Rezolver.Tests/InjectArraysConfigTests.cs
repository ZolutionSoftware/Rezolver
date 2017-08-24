using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class InjectArraysConfigTests
    {
        

        protected TargetContainer GetArrayEnabledContainer()
        {
            return new TargetContainer(new CombinedTargetContainerConfig()
            {
                Configuration.InjectEnumerables.Instance,
                Configuration.InjectArrays.Instance
            });
        }

        [Fact]
        public void ShouldFetchArrayTarget()
        {
            var targets = GetArrayEnabledContainer();
            var result = targets.Fetch(typeof(int[]));
            Assert.NotNull(result);
        }

        [Fact]
        public void ShouldFetchExplicitlyRegisteredArray()
        {
            var targets = GetArrayEnabledContainer();
            var myArray = Target.ForObject(new[] { "hello world" });
            targets.Register(myArray);
            var result = targets.Fetch(typeof(string[]));
            Assert.Same(myArray, result);
        }
    }
}
