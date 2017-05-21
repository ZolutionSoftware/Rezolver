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
        public void ShouldGetTargetListContainer()
        {
            var configuration = new Rezolver.TargetContainerBuilder();
            
            configuration.ForConfigUse((TargetListConfig o) => new TargetListContainer(o.Type));

            var list = Assert.IsType<TargetListContainer>(configuration.CreateContainer<TargetListConfig>());

            Assert.Equal(typeof(string), list.RegisteredType);
        }
    }
}
