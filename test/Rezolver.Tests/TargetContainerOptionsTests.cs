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
            //simple example of how to implement your own custom option

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
                .SetOption<TestOption, int>("second")
                .SetOption<TestOption>(typeof(IEnumerable<>), "third")
                .SetOption<TestOption, IEnumerable<int>>("fourth");

            Assert.Equal("first", targets.GetOption<TestOption>());
            Assert.Equal("second", targets.GetOption<TestOption, int>());
            Assert.Equal("third", targets.GetOption<TestOption>(typeof(IEnumerable<>)));
            Assert.Equal("fourth", targets.GetOption<TestOption, IEnumerable<int>>());
        }

        [Fact]
        public void ShouldUseOpenGenericServiceOptionForClosed()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>(typeof(IEnumerable<>), "open");

            Assert.Equal("open", targets.GetOption<TestOption, IEnumerable<int>>());
        }

        [Fact]
        public void ClosedGenericServiceOptionShouldOverrideOpenGeneric()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>(typeof(IEnumerable<>), "open");
            targets.SetOption<TestOption, IEnumerable<int>>("int");

            Assert.Equal("int", targets.GetOption<TestOption, IEnumerable<int>>());
        }

        [Fact]
        public void ShouldFallBackToGlobalOptionByDefault()
        {
            // setting a global option should apply for all types that don't have
            // that option explicitly set.
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("global");

            Assert.Equal("global", targets.GetOption<TestOption, int>());
            Assert.Equal("global", targets.GetOption<TestOption, IEnumerable<int>>());
            Assert.Equal("global", targets.GetOption<TestOption>(typeof(IEnumerable<>)));
        }

        [Fact]
        public void ShouldUseDefaultInsteadOfGlobalWhenConfigured()
        {
            var targets = new TargetContainer();

            // you can control how the options functionality works by using options :)
            targets.SetOption<UseGlobalsForUnsetServiceOptions>(false);
            targets.SetOption<TestOption>("global");

            Assert.Equal("default", targets.GetOption<TestOption, int>("default"));
            Assert.Equal("default", targets.GetOption<TestOption, IEnumerable<int>>("default"));
            Assert.Equal("default", targets.GetOption<TestOption>(typeof(IEnumerable<>), "default"));
        }

        [Fact]
        public void OptionShouldBeInheritedByOverridingTargetContainer()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("base");

            var overriding = new OverridingTargetContainer(targets);

            Assert.Equal("base", overriding.GetOption<TestOption>());
        }

        [Fact]
        public void OptionShouldBeOverridenByOverridingTargetContainer()
        {
            var targets = new TargetContainer();
            targets.SetOption<TestOption>("base");

            var overriding = new OverridingTargetContainer(targets);
            overriding.SetOption<TestOption>("overriden");

            Assert.Equal("overriden", overriding.GetOption<TestOption>());
        }
    }
}
