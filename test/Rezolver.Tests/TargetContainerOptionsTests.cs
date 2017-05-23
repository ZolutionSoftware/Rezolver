using Rezolver.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainerOptionsTests
    {
        private class TestOption : ContainerOption<string>
        {
            public static implicit operator TestOption(string value)
            {
                return new TestOption() { Value = value };
            }
        }

        [Fact]
        public void ShouldGetAndSetOptions()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("first")
                .SetOption<int, TestOption>("second")
                .SetOption<TestOption>(typeof(IEnumerable<>), "third")
                .SetOption<IEnumerable<int>, TestOption>("fourth");

            Assert.Equal("first", targets.GetOption<TestOption>());
            Assert.Equal("second", targets.GetOption<int, TestOption>());
            Assert.Equal("third", targets.GetOption<TestOption>(typeof(IEnumerable<>)));
            Assert.Equal("fourth", targets.GetOption<IEnumerable<int>, TestOption>());
        }

        [Fact]
        public void ShouldFallBackToGlobalOptionByDefault()
        {
            // setting a global option should apply for all types that don't have
            // that option explicitly set.
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("global");

            Assert.Equal("global", targets.GetOption<int, TestOption>());
            Assert.Equal("global", targets.GetOption<IEnumerable<int>, TestOption>());
            Assert.Equal("global", targets.GetOption<TestOption>(typeof(IEnumerable<>)));
        }

        [Fact]
        public void ShouldUseDefaultInsteadOfGlobalWhenConfigured()
        {
            var targets = new TargetContainer();

            // you can control how the options functionality works by using options :)
            targets.SetOption<UseGlobalsForUnsetServiceOptions>(false);
            targets.SetOption<TestOption>("global");

            Assert.Equal("default", targets.GetOption<int, TestOption>("default"));
            Assert.Equal("default", targets.GetOption<IEnumerable<int>, TestOption>("default"));
            Assert.Equal("default", targets.GetOption<TestOption>(typeof(IEnumerable<>), "default"));
        }
    }
}
