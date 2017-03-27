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
            Assert.Collection(result.Services,
                expectedTypes.Select(
                    t => new Action<IMyService>((i) => Assert.Equal(t, i.GetType()))).ToArray());
            //</example2>
        }
    }
}
