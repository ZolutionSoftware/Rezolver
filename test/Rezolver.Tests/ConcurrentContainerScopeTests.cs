using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class ConcurrentContainerScopeTests
    {
        //this is testing the container scope's low level behaviour, which
        //does not require a container to work - just a delegate that creates
        //an object - which, in normal operation, is taken care of by the compiler.

        [Fact]
        public void ShouldDisposeImplicitIDisposable()
        {
            Disposable disposable;

            using (var scope = new ConcurrentContainerScope(new Container()))
            {
                disposable = scope.ActivateImplicit(new Disposable());
            }

            Assert.True(disposable.Disposed);
            Assert.Equal(1, disposable.DisposedCount);
        }

        [Fact]
        public void ShouldDisposeExplicitIDisposable()
        {
            Disposable disposable;

            using (var scope = new ConcurrentContainerScope(new Container()))
            {
                disposable = scope.ActivateExplicit(
                    new ResolveContext(scope, typeof(Disposable)),
                    new TestTarget().Id,
                    c => new Disposable());
            }

            Assert.True(disposable.Disposed);
            Assert.Equal(1, disposable.DisposedCount);
        }

        [Fact]
        public void ChildScopeShouldDisposeItsOwnImplicitDisposable()
        {
            using (var scope = new ConcurrentContainerScope(new Container()))
            {
                Disposable inner1, inner2;

                using (var childScope = scope.CreateScope())
                {
                    inner1 = childScope.ActivateImplicit(new Disposable());
                }

                Assert.True(inner1.Disposed);
                Assert.Equal(1, inner1.DisposedCount);

                using (var childScope = scope.CreateScope())
                {
                    inner2 = childScope.ActivateImplicit(new Disposable());
                }

                Assert.True(inner2.Disposed);
                Assert.Equal(1, inner2.DisposedCount);
            }
        }

        [Fact]
        public void ShouldCreateOnlyOneExplicitNonDisposable()
        {
            // note - this works because we pass the same target both times
            using (var scope = new ConcurrentContainerScope(new Container()))
            {
                Func<ResolveContext, NoCtor> factory = c => new NoCtor();
                var target = new TestTarget();
                var result = scope.ActivateExplicit(
                    new ResolveContext(scope, typeof(NoCtor)),
                    target.Id,
                    factory);

                Assert.NotNull(result);

                var result2 = scope.ActivateExplicit(
                    new ResolveContext(scope, typeof(NoCtor)),
                    target.Id,
                    factory);

                Assert.Same(result, result2);
            }
        }

        [Fact]
        public void ChildScopeShouldGetItsOwnExplicitInstance()
        {
            using (var scope = new ConcurrentContainerScope(new Container()))
            {
                Func<ResolveContext, NoCtor> factory = c => new NoCtor();
                var target = new TestTarget();
                var result = scope.ActivateExplicit(
                    new ResolveContext(scope, typeof(NoCtor)),
                    target.Id,
                    factory);

                Assert.NotNull(result);

                using (var childScope = scope.CreateScope())
                {
                    var result2 = childScope.ActivateExplicit(
                        new ResolveContext(childScope, typeof(NoCtor)),
                        target.Id,
                        factory);

                    Assert.NotSame(result, result2);
                }
            }
        }

        [Fact]
        public void ImplicitlyScopedObjectShouldNotBeSameAsExplicitlyScopedObject()
        {
            Disposable explicitlyScoped, implicitlyScoped;
            using (var scope = new ConcurrentContainerScope(new Container()))
            {
                Func<ResolveContext, Disposable> factory = c => new Disposable();
                explicitlyScoped = scope.ActivateExplicit(
                    new ResolveContext(scope, typeof(Disposable)),
                    new TestTarget().Id,
                    factory);

                implicitlyScoped = scope.ActivateImplicit(
                    new Disposable());

                Assert.NotSame(explicitlyScoped, implicitlyScoped);
            }
            Assert.True(explicitlyScoped.Disposed);
            Assert.True(implicitlyScoped.Disposed);
        }

        [Fact]
        public void ParentScopeShouldDisposeAllChildScopesAndObjects()
        {
            Disposable parentObj, childObj, grandChildObj, siblingObj;

            using (var parent = new ConcurrentContainerScope(new Container()))
            {
                var child = parent.CreateScope();
                var grandChild = child.CreateScope();
                var sibling = parent.CreateScope();

                parentObj = parent.ActivateImplicit(new Disposable());

                childObj = child.ActivateImplicit(new Disposable());

                grandChildObj = grandChild.ActivateImplicit(new Disposable());

                siblingObj = sibling.ActivateImplicit(new Disposable());
            }

            Assert.All(new[] { parentObj, childObj, grandChildObj, siblingObj },
                d =>
                {
                    Assert.True(d.Disposed);
                    Assert.Equal(1, d.DisposedCount);
                });
        }
    }
}
