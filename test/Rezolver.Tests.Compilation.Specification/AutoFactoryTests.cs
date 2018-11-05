using Rezolver.Compilation.Expressions;
using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{ 
    public partial class CompilerTestsBase
    { 
        [Fact]
        public void AutoFactory_ShouldCreateSimple()
        {
            // simplest scenario - auto-creating a Func<T> instead of producing a T

            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<NoCtor>();
            targets.EnableAutoFactory<NoCtor>();
            var container = CreateContainer(targets);

            // Act
            Func<NoCtor> result = container.Resolve<Func<NoCtor>>();

            // Assert
            Assert.NotNull(result);
            var instance = result();
            var instance2 = result();
            Assert.NotNull(instance);
            Assert.NotNull(instance2);
            Assert.NotSame(instance, instance2);
        }

        [Fact]
        public void AutoFactory_DisposableShouldHonourImplicitScope()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<Disposable>();
            targets.EnableAutoFactory<Disposable>();
            var container = CreateContainer(targets);

            // When a factory is created, it is *bound* to the scope from which you created it,
            // so will only create objects while that scope is alive - and the same is true for all
            // disposable instances that it creates.
            // In this case, we're testing objects which *happen* to be disposable and which are, therefore, implicitly scoped.

            Disposable outer1, outer2, inner1, inner2;
            using (var outerScope = container.CreateScope())
            {
                // Act
                var outerFactory = outerScope.Resolve<Func<Disposable>>();
                outer1 = outerFactory();
                outer2 = outerFactory();

                using (var innerScope = outerScope.CreateScope())
                {
                    var innerFactory = innerScope.Resolve<Func<Disposable>>();
                    inner1 = innerFactory();
                    inner2 = innerFactory();

                    // Assert
                    Assert.NotSame(outer1, inner1);
                    Assert.NotSame(inner1, inner2);
                }
                Assert.True(inner1.Disposed);
                Assert.True(inner2.Disposed);
                Assert.False(outer1.Disposed);
                Assert.False(outer2.Disposed);
            }

            Assert.True(outer1.Disposed);
            Assert.True(outer2.Disposed);
        }


        [Fact]
        public void AutoFactory_DisposableShouldHonourExplicitScope()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterScoped<Disposable>();
            targets.EnableAutoFactory<Disposable>();
            var container = CreateContainer(targets);

            // When a factory is created, it is *bound* to the scope from which you created it,
            // so will only create objects while that scope is alive - and the same is true for all
            // disposable instances that it creates.  

            // This test looks specifically at explicitly scoped objects and, therefore, tests that 
            // the factory produces the same instance according to the scope that the factory was built from.

            Disposable outer, inner;
            using (var outerScope = container.CreateScope())
            {
                // Act
                var outerFactory = outerScope.Resolve<Func<Disposable>>();
                outer = outerFactory();
                var outer2 = outerFactory();

                using (var innerScope = outerScope.CreateScope())
                {
                    var innerFactory = innerScope.Resolve<Func<Disposable>>();
                    inner = innerFactory();
                    var inner2 = innerFactory();

                    // Assert
                    Assert.NotSame(outer, inner);
                    Assert.Same(inner, inner2);
                }
                Assert.Same(outer, outer2);
                Assert.True(inner.Disposed);
                Assert.False(outer.Disposed);
            }
            Assert.True(outer.Disposed);
            Assert.Equal(1, outer.DisposedCount);
            Assert.Equal(1, inner.DisposedCount);
        }

        [Fact]
        public void AutoFactory_SingletonShouldHonourRootScope()
        {
            // Arrange
            var targets = CreateTargetContainer();

            targets.RegisterSingleton<Disposable>();
            targets.EnableAutoFactory<Disposable>();
            var container = CreateContainer(targets);

            // this time, different factories should bind to the same singleton

            Disposable outer, inner;
            using (var outerScope = container.CreateScope())
            {
                // Act
                var outerFactory = outerScope.Resolve<Func<Disposable>>();
                outer = outerFactory();
                using (var innerScope = outerScope.CreateScope())
                {
                    var innerFactory = innerScope.Resolve<Func<Disposable>>();
                    inner = innerFactory();

                    // Assert
                    Assert.Same(outer, inner);
                }
                Assert.False(outer.Disposed);
            }
            Assert.True(outer.Disposed);
        }

        [Fact]
        public void AutoFactory_MultipleFactoriesShouldStillHonourExplicitScoping()
        {
            // Arrange
            var targets = CreateTargetContainer();

            // this time we'll register a scoped object and create one scope,
            // then we'll resolve multiple factories (verifying that they are different instances)
            // then try to resolve instances from them.  Each factory should produce the same instance.

            targets.RegisterScoped<Disposable>();
            targets.EnableAutoFactory<Disposable>();
            var container = CreateContainer(targets);

            // Act
            Disposable instance1, instance2, instance3;
            using (var scope = container.CreateScope())
            {
                // resolve multiple factories
                var fact1 = scope.Resolve<Func<Disposable>>();
                var fact2 = scope.Resolve<Func<Disposable>>();
                var fact3 = scope.Resolve<Func<Disposable>>();

                instance1 = fact1();
                instance2 = fact2();
                instance3 = fact3();

                // Assert
                Assert.NotSame(fact1, fact2);
                Assert.NotSame(fact2, fact3);

                Assert.Same(instance1, instance2);
                Assert.Same(instance2, instance3);
            }

            Assert.True(instance1.Disposed);
        }

        [Fact]
        public void AutoFactory_ShouldAcceptResolvedDependencyAsArgument()
        {
            // Arrange
            var targets = CreateTargetContainer();

            targets.RegisterType<RequiresInt>();
            targets.EnableAutoFactory<int, RequiresInt>();
            var container = CreateContainer(targets);

            // Act
            var factory = container.Resolve<Func<int, RequiresInt>>();

            // Assert
            var instance = factory(10);

            Assert.NotNull(instance);
            Assert.Equal(10, instance.IntValue);
            var instance2 = factory(20);
            Assert.NotSame(instance, instance2);
            Assert.NotNull(instance2);
            Assert.Equal(20, instance2.IntValue);
        }

        [Fact]
        public void AutoFactory_ShouldUseArgumentEvenWhenDependencyRegistered()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<RequiresInt>();
            targets.RegisterObject(10);
            targets.EnableAutoFactory<int, RequiresInt>();
            var container = CreateContainer(targets);

            // Act
            var factory = container.Resolve<Func<int, RequiresInt>>();
            var normalInstance = container.Resolve<RequiresInt>();

            // Assert
            Assert.Equal(10, normalInstance.IntValue);
            Assert.Equal(20, factory(20).IntValue);
        }

        [Fact]
        public void AutoFactory_ShouldSupportEnumerableViaEnumerableContainer()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();
            //targets.EnableAutoFactory<IEnumerable<BaseClass>>();
            var container = CreateContainer(targets);

            // Act
            var factory = container.Resolve<Func<IEnumerable<BaseClass>>>();
            var instance = factory().ToArray();

            // Assert
            Assert.Collection(instance, i => Assert.IsType<BaseClass>(i), i => Assert.IsType<BaseClassChild>(i), i => Assert.IsType<BaseClassGrandchild>(i));
        }

        [Fact]
        public void AutoFactory_ShouldBeAbleToResolveAnEnumerableOfAutoFactoriesViaCovariance()
        {
            // Similar to above, but the implementation here we're registering the individual types and then
            // getting an IEnumerable<Func<BaseClass>>.
            // Key things being tested here are:
            // 1) That the implementation honours the covariance required by IEnumerable<out T>
            // 2) That the order of the generated delegates reflects the registration order of the underlying targets

            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();
            targets.EnableAutoFactory<BaseClass>();
            var container = CreateContainer(targets);

            // Act
            var factories = container.ResolveMany<Func<BaseClass>>();
            var instances = factories.Select(f => f());

            // Assert
            Assert.Collection(instances, i => Assert.IsType<BaseClass>(i), i => Assert.IsType<BaseClassChild>(i), i => Assert.IsType<BaseClassGrandchild>(i));
        }

        [Fact]
        public void AutoFactory_ShouldBeAbleToResolveAnEumerableOfParameterisedAutoFactoriesViaCovariance()
        {
            // this time, it's like above, but we're also doing an enumerable of automatic 
            // parameterised factories, and we want to test that the parameters work correctly

            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterType<OneCtor>();
            targets.RegisterType<OneCtorAlt1>();
            targets.RegisterType<OneCtorAlt2>();
            targets.EnableAutoFactory<int, OneCtor>();
            var container = CreateContainer(targets);

            // Act
            var factories = container.ResolveMany<Func<int, NoCtor>>();
            var instances = factories.Select(f => f(50));

            // Assert
            Assert.Collection(instances, i => Assert.IsType<OneCtor>(i), i => Assert.IsType<OneCtorAlt1>(i), i => Assert.IsType<OneCtorAlt2>(i));
            Assert.Equal(new[] { 50, 50, 50 }, instances.Select(i => i.Value));
        }
    }
}
