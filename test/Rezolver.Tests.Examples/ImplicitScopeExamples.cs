using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ImplicitScopeExamples
    {
        [Fact]
        public void ShouldDisposeWhenScopeDisposed()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<DisposableType>();

            DisposableType result;
            using(var scope = container.CreateScope())
            {
                result = scope.Resolve<DisposableType>();
            }

            Assert.True(result.Disposed);
            // </example1>
        }

        [Fact]
        public void ChildScopesShouldOnlyDisposeTheirOwnObjects()
        {
            // <example2>
            var container = new Container();
            container.RegisterType<DisposableType>();

            DisposableType rootResult;
            using(var rootScope = container.CreateScope())
            {
                rootResult = rootScope.Resolve<DisposableType>();
                DisposableType childResult;
                using (var childScope = rootScope.CreateScope())
                {
                    childResult = childScope.Resolve<DisposableType>();
                    DisposableType grandChildResult;
                    using(var grandChildScope = childScope.CreateScope())
                    {
                        grandChildResult = grandChildScope.Resolve<DisposableType>();
                    }
                    Assert.True(grandChildResult.Disposed);
                    Assert.False(childResult.Disposed);
                    Assert.False(rootResult.Disposed);
                }
                Assert.True(childResult.Disposed);
                Assert.False(rootResult.Disposed);
            }
            Assert.True(rootResult.Disposed);
            // </example2>
        }

        [Fact]
        public void DisposableDependencyShouldBeDisposedByScope()
        {
            // <example3>
            var container = new Container();
            container.RegisterType<RequiresDisposableType>();
            container.RegisterType<DisposableType>();

            RequiresDisposableType result;
            using(var scope = container.CreateScope())
            {
                result = scope.Resolve<RequiresDisposableType>();
            }

            Assert.True(result.Disposable.Disposed);
            // </example3>
        }

        [Fact]
        public void DisposableSingletonShouldBeDisposedByRootScope()
        {
            // <example4>
            // In this example we use the disposable ScopedContainer, which
            // supports all the same functionality as 'Container' except it
            // also has its own scope, and is therefore disposable.
            DisposableType result;
            using (var container = new ScopedContainer())
            {
                container.RegisterSingleton<DisposableType>();
                using (var scope = container.CreateScope())
                {
                    // singletons force tracking in the 
                    // rootmost scope of a scope hierarchy
                    result = scope.Resolve<DisposableType>();
                }
                Assert.False(result.Disposed);
            }
            Assert.True(result.Disposed);
            // </example4>
        }

        [Fact]
        public void DisposableDependencyOfASingletonShouldBeDisposedByRootScope()
        {
            // <example5>
            // THIS EXAMPLE FAILS < 1.2
            var container = new Container();
            container.RegisterSingleton<RequiresDisposableType>();
            container.RegisterType<DisposableType>();

            RequiresDisposableType result;
            using (var scope = container.CreateScope())
            {
                using (var childScope = scope.CreateScope())
                {
                    result = childScope.Resolve<RequiresDisposableType>();
                }
                Assert.False(result.Disposable.Disposed);
            }

            Assert.True(result.Disposable.Disposed);
            // </example5>
        }
    }
}
