using Rezolver.Configuration;
using Rezolver.Sdk;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class OptionDependentConfigurationTests
    {
        public class ConfiguredByTestOptionConfig : Configuration.OptionDependentConfig<TestOption>
        {
            private readonly Action<IRootTargetContainer> _verify;

            public ConfiguredByTestOptionConfig(Action<IRootTargetContainer> verify, bool required)
                : base(required)
            {
                _verify = verify;
            }

            public override void Configure(IRootTargetContainer targets)
            {
                _verify(targets);
                targets.RegisterObject(this);
            }
        }

        // So these tests use the CombinedTargetContainerConfig - which uses the dependant/dependency ordering algorithm
        // to apply configurations in the correct order - which what Rezolver does by default with its global configs.

        [Fact]
        public void ShouldConfigureAfterOptionSet()
        {
            CombinedTargetContainerConfig coll = new CombinedTargetContainerConfig
            {
                // add the dependant first
                new ConfiguredByTestOptionConfig(t => Assert.Equal("configured!", t.GetOption<TestOption>()), true)
            };
            // then add the config that configures the option :)
            coll.ConfigureOption<TestOption>("configured!");

            var targets = new TargetContainer(coll);
            // this just ensures that the configure method was actually called
            Assert.IsType<Rezolver.Targets.ObjectTarget>(targets.Fetch(typeof(ConfiguredByTestOptionConfig)));
        }

        [Fact]
        public void ShouldConfigureAfterAllOptionsSet()
        {
            CombinedTargetContainerConfig coll = new CombinedTargetContainerConfig
            {
                new ConfiguredByTestOptionConfig(t =>
                {
                    Assert.Equal("global configured!", t.GetOption<TestOption>());
                    Assert.Equal("IEnumerable configured!", t.GetOption<TestOption>(typeof(IEnumerable<>)));
                    Assert.Equal("IEnumerable<string> configured!", t.GetOption<TestOption, IEnumerable<string>>());
                    Assert.Equal("IEnumerable configured!", t.GetOption<TestOption, IEnumerable<double>>());
                    Assert.Equal("global configured!", t.GetOption<TestOption, int>());
                }, true)
            };

            coll.ConfigureOption<TestOption, IEnumerable<string>>("IEnumerable<string> configured!");
            coll.ConfigureOption<TestOption>(typeof(IEnumerable<>), "IEnumerable configured!");
            coll.ConfigureOption<TestOption>("global configured!");

            var targets = new TargetContainer(coll);

            Assert.IsType<Rezolver.Targets.ObjectTarget>(targets.Fetch(typeof(ConfiguredByTestOptionConfig)));
        }
    }
}
