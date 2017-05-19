using Rezolver;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainerFactoryTests
    {

        [Fact]
        public void ShouldConfigureTargetListOptions()
        {
            var configuration = new TargetContainerConfiguration(new TargetContainer());
            configuration.ConfigureOptions<TargetListOptions>(c =>
            {
                c.DisableMultiple = true;
                return c;
            });

            var result = configuration.GetOptions<TargetListOptions>();
            Assert.True(result.DisableMultiple);
        }

        [Fact]
        public void ShouldGetTargetListContainer()
        {
            var configuration = new TargetContainerConfiguration(new TargetContainer());
            configuration.ConfigureOptions<TargetListOptions>(c =>
            {
                c.DisableMultiple = true;
                return c;
            });
            configuration.RegisterTargetContainerFactory((TargetListOptions o) => new TargetListContainer(o.Type));

            var options = configuration.GetOptions<TargetListOptions>();
            options.Type = typeof(string);

            var list = Assert.IsType<TargetListContainer>(configuration.CreateContainer<TargetListOptions>());

            Assert.Equal(typeof(string), list.RegisteredType);
        }
    }
}
