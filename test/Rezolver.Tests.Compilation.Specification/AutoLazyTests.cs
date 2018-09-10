using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        private IRootTargetContainer CreateAutoLazyAndAutoFactoryContainer()
        {
            // requires the Func functionality
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterContainer(typeof(Lazy<>), new LazyTargetContainer(targets));
            return targets;
        }

        private IRootTargetContainer CreateAutoLazyContainer()
        {
            var targets = CreateTargetContainer();
            targets.RegisterContainer(typeof(Lazy<>), new LazyTargetContainer(targets));
            return targets;
        }

        [Fact]
        public void AutoLazy_ShouldCreateSimpleUsingSuppliedFactory()
        {
            // validate that it works when using a user-supplied factory

            // arrange
            var targets = CreateAutoLazyContainer();
            targets.RegisterObject<Func<RequiresInt>>(() => new RequiresInt(25));
            var container = CreateContainer(targets);

            // act
            var lazy = container.Resolve<Lazy<RequiresInt>>();
            var instance = lazy.Value;

            // assert
            Assert.Equal(25, instance.IntValue);
        }

        [Fact]
        public void AutoLazy_ShouldCreateSimpleUsingAutoFactory()
        {
            // arrange
            var targets = CreateAutoLazyAndAutoFactoryContainer();
            targets.RegisterObject(15);
            targets.RegisterType<RequiresInt>();
            var container = CreateContainer(targets);

            // act
            var lazy = container.Resolve<Lazy<RequiresInt>>();
            var instance = lazy.Value;

            // assert
            Assert.Equal(15, instance.IntValue);
        }

        [Fact]
        public void AutoLazy_ShouldCreateSingletonUsingAutoFactory()
        {
            // Arrange
            var targets = CreateAutoLazyAndAutoFactoryContainer();
            targets.RegisterSingleton<NoCtor>();
            var container = CreateContainer(targets);

            // Act
            var lazy1 = container.Resolve<Lazy<NoCtor>>();
            var lazy2 = container.Resolve<Lazy<NoCtor>>();

            // Assert
            Assert.NotSame(lazy1, lazy2);
            Assert.Same(lazy1.Value, lazy2.Value);
        }

        [Fact]
        public void AutoLazy_ShouldHonourImplicitScopes()
        {
            // Arrange
            var targets = CreateAutoLazyAndAutoFactoryContainer();
            targets.RegisterType<Disposable>();
            var container = CreateContainer(targets);

            // Act
            Disposable outerInstance, innerInstance;
            using (var outerScope = container.CreateScope())
            {
                var outerLazy = outerScope.Resolve<Lazy<Disposable>>();
                outerInstance = outerLazy.Value;
                using (var innerScope = outerScope.CreateScope())
                {
                    var innerLazy = innerScope.Resolve<Lazy<Disposable>>();
                    innerInstance = innerLazy.Value;

                    // Assert
                    Assert.NotSame(outerLazy, innerLazy);
                    Assert.NotSame(outerInstance, innerInstance);   // shouldn't be necessary, but we're relying on complex functionality here.
                }
                Assert.True(innerInstance.Disposed);
                Assert.False(outerInstance.Disposed);
            }
            Assert.True(outerInstance.Disposed);
        }

        [Fact]
        public void AutoLazy_ShouldHonourSingletonScope()
        {
            // Arrange
            var targets = CreateAutoLazyAndAutoFactoryContainer();
            targets.RegisterSingleton<Disposable>();
            var container = CreateContainer(targets);

            // Act
            Disposable instance;
            using (var rootScope = container.CreateScope())
            {
                using (var childScope = rootScope.CreateScope())
                {
                    var lazy = childScope.Resolve<Lazy<Disposable>>();
                    instance = lazy.Value;
                }
                // Assert
                Assert.False(instance.Disposed);
            }
            Assert.True(instance.Disposed);
        }

        [Fact]
        public void AutoLazy_MultipleLaziesShouldStillHonourExplicitScoping()
        {
            // Arrange
            var targets = CreateAutoLazyAndAutoFactoryContainer();
            targets.RegisterScoped<Disposable>();
            var container = CreateContainer(targets);

            // Act
            Disposable instance1, instance2, instance3;
            using (var scope = container.CreateScope())
            {
                var lazy1 = scope.Resolve<Lazy<Disposable>>();
                var lazy2 = scope.Resolve<Lazy<Disposable>>();
                var lazy3 = scope.Resolve<Lazy<Disposable>>();

                instance1 = lazy1.Value;
                instance2 = lazy2.Value;
                instance3 = lazy3.Value;

                // Assert
                Assert.NotSame(lazy1, lazy2);
                Assert.NotSame(lazy2, lazy3);

                Assert.Same(instance1, instance2);
                Assert.Same(instance2, instance3);
            }
            Assert.True(instance1.Disposed);
        }

        internal class LazyTargetContainer : GenericTargetContainer
        {
            private ConcurrentDictionary<Type, ConstructorTarget> _cachedTargets = new ConcurrentDictionary<Type, ConstructorTarget>();
            internal LazyTargetContainer(IRootTargetContainer root)
                : base(root, typeof(Lazy<>))
            {

            }

            public override ITarget Fetch(Type type)
            {
                Type genericType;
                if (!TypeHelpers.IsGenericType(type) || (genericType = type.GetGenericTypeDefinition()) != GenericType)
                {
                    throw new ArgumentException($"Only {GenericType} is supported by this container", nameof(type));
                }

                // just like EnumerableTargetContainer, we allow for specific registrations
                var result = base.Fetch(type);

                if (result != null)
                    return result;


                return _cachedTargets.GetOrAdd(type, BuildTarget);

                ConstructorTarget BuildTarget(Type t)
                {
                    // we need a ConstructorTarget that binds to the correct Lazy<T> constructor (for the specific type 
                    // passed as the generic type argument)
                    // for that, we need the generic type argument
                    var innerLazyType = TypeHelpers.GetGenericArguments(t)[0];
                    var constructor = TypeHelpers.GetConstructor(t, new[] { typeof(Func<>).MakeGenericType(innerLazyType) });

                    if (constructor == null)
                        throw new InvalidOperationException($"Could not get Func callback constructor for {t}");

                    return new ConstructorTarget(constructor);
                }
            }
        }
    }
}
