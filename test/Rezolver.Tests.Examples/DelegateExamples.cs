using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class DelegateExamples
    {

        [Fact]
        public void ShouldGetMyService()
        {
            //<example1>
            var container = new Container();
            container.RegisterDelegate(() => new MyService());

            var result = container.Resolve<MyService>();
            Assert.NotNull(result);
            //</example1>
        }

        [Fact]
        public void ShouldGetMyServiceAsASingleton()
        {
            //<example2>
            var container = new Container();
            container.Register(
                //RegisterSingleton specialises for types only, so
                //we create the target manually and apply this .Singleton 
                //extension method to to it before registering
                Target.ForDelegate(() => new MyService()).Singleton()
            );

            var result = container.Resolve<MyService>();
            var result2 = container.Resolve<MyService>();

            Assert.Same(result, result2);
            //</example2>
        }

        [Fact]
        public void ScopeShouldDisposeDelegateResult()
        {
            //<example3>
            var container = new Container();
            container.RegisterDelegate(() => new DisposableType());

            DisposableType result, result2;

            using (var scope = container.CreateScope())
            {
                result = scope.Resolve<DisposableType>();
                using (var childScope = scope.CreateScope())
                {
                    result2 = scope.Resolve<DisposableType>();
                }
                Assert.True(result2.Disposed);
            }
            Assert.True(result.Disposed);
            //</example3>
        }
    }
}
