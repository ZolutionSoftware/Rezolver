using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class EnumerableExamples
    {
        [Fact]
        public void ShouldInjectEmptyEnumerable()
        {
            //<example1>
            var container = new Container();
            container.RegisterType<RequiresEnumerableOfServices>();

            var result = container.Resolve<RequiresEnumerableOfServices>();
            Assert.Empty(result.Services);
            //</example1>
        }

        [Fact]
        public void ShouldInjectEnumerableWithThreeItems()
        {
            //<example2>
            var container = new Container();
            var expectedTypes = new[] {
                typeof(MyService1), typeof(MyService2), typeof(MyService3)
            };
            foreach(var t in expectedTypes)
            {
                container.RegisterType(t, typeof(IMyService));
            }
            container.RegisterType<RequiresEnumerableOfServices>();

            var result = container.Resolve<RequiresEnumerableOfServices>();
            Assert.Equal(3, result.Services.Count());
            Assert.All(
                result.Services.Zip(
                    expectedTypes, 
                    (s, t) => (service: s, expectedType: t)
                ),
                t => Assert.IsType(t.expectedType, t.service));
            //</example2>
        }

        [Fact]
        public void ShouldInjectEnumerableWithItemsFromDifferentTargets()
        {
            //<example3>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterDelegate<IMyService>(() => new MyService2());
            container.RegisterObject<IMyService>(new MyService3());

            //shows also that injection of IEnumerables holds wherever injection
            //is normally supported - such as here, with delegate argument injection
            container.RegisterDelegate((IEnumerable<IMyService> services) =>
            {
                if (!services.OfType<MyService4>().Any())
                    services = services.Concat(new[] { new MyService4() });
                return new RequiresEnumerableOfServices(services);
            });

            var result = container.Resolve<RequiresEnumerableOfServices>();

            Assert.Equal(4, result.Services.Count());
            //just check they're all different types this time.
            Assert.Equal(4, result.Services.Select(s => s.GetType()).Distinct().Count());
            //</example3>
        }
    }
}
