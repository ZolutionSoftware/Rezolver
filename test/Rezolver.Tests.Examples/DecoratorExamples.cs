using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class DecoratorExamples
    {
        [Fact]
        public void ShouldResolveOneDecoratedService()
        {
            // <example1>
            var targets = new TargetContainer();
            targets.RegisterType<MyService, IMyService>();
            targets.RegisterDecorator<MyServiceDecorator1, IMyService>();

            var container = new Container(targets);

            var result = container.Resolve<IMyService>();
            var decorator = Assert.IsType<MyServiceDecorator1>(result);

            Assert.IsType<MyService>(decorator.Inner);
            // </example1>
        }

        [Fact]
        public void ShouldResolveOneReDecoratedService()
        {
            // <example2>
            // same as before - but two decorators
            // note that the order they're registered determines the order in which 
            // the decoration occurs.
            var targets = new TargetContainer();
            targets.RegisterType<MyService, IMyService>();
            targets.RegisterDecorator<MyServiceDecorator2, IMyService>();
            targets.RegisterDecorator<MyServiceDecorator1, IMyService>();

            var container = new Container(targets);

            var result = container.Resolve<IMyService>();
            var decorator = Assert.IsType<MyServiceDecorator1>(result);
            var innerDecorator = Assert.IsType<MyServiceDecorator2>(decorator.Inner);
            Assert.IsType<MyService>(innerDecorator.Inner);
            // </example2>
        }
    }
}
