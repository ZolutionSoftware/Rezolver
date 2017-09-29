using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class ConfigureOptionsTests
    {
        // SEE ALSO: TargetContainerOptionsTests - for tests of the underlying GetOption/SetOption API

        [Fact]
        public void ShouldConfigureGlobalOption()
        {
            CombinedTargetContainerConfig config = new CombinedTargetContainerConfig();
            config.ConfigureOption((TestOption)"global");

            var targets = new TargetContainer(config);
            Assert.Equal("global", targets.GetOption<TestOption>());
        }

        [Fact]
        public void ShouldConfigureGlobalOptionByCallback()
        {
            CombinedTargetContainerConfig config = new CombinedTargetContainerConfig();
            config.ConfigureOption(tc =>
            {
                Assert.NotNull(tc);
                return (TestOption)"global (callback)";
            });

            var targets = new TargetContainer(config);

            Assert.Equal("global (callback)", targets.GetOption<TestOption>());
        }

        [Fact]
        public void ShouldConfigureServiceSpecicOption()
        {
            CombinedTargetContainerConfig config = new CombinedTargetContainerConfig();
            config.ConfigureOption<TestOption, string>("for System.String");

            var targets = new TargetContainer(config);

            Assert.Equal("for System.String", targets.GetOption<TestOption, string>());
            Assert.Null(targets.GetOption<TestOption>());
        }


        [Fact]
        public void ShouldConfigureServiceSpecificOptionByCallback()
        {
            CombinedTargetContainerConfig config = new CombinedTargetContainerConfig();
            config.ConfigureOption(typeof(string), (tc, t) =>
            {
                Assert.NotNull(tc);
                Assert.Equal(typeof(string), t);
                return (TestOption)$"for { t.FullName } (callback)";
            });

            var targets = new TargetContainer(config);

            Assert.Equal($"for { typeof(string).FullName } (callback)", targets.GetOption<TestOption, string>());
            Assert.Null(targets.GetOption<TestOption>());
        }
    }
}
