using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        [Fact]
        public void Projection_ShouldAutoProject()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);
            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterProjection<From, To>();

            // Act
            var result = container.ResolveMany<To>();

            // Assert
            Assert.Collection(result,
                r => Assert.IsType<From1>(r.From),
                r => Assert.IsType<From2>(r.From));
        }

        [Fact]
        public void Projection_ShouldAutoProject_SpecificImplementationType()
        {
            // Arramge
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);

            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterProjection<From, ITo, To>();

            // Act
            var result = container.ResolveMany<ITo>();

            // Assert
            Assert.Collection(result,
                r => Assert.IsType<From1>(r.From),
                r => Assert.IsType<From2>(r.From));
        }

        [Fact]
        public void Projection_ShouldProject_FromRegistration()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);

            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterType<To, ITo>();
            targets.RegisterProjection<From, ITo>();

            // Act
            var result = container.ResolveMany<ITo>();

            // Assert
            Assert.Collection(result,
                r => Assert.IsType<From1>(Assert.IsType<To>(r).From),
                r => Assert.IsType<From2>(Assert.IsType<To>(r).From));
        }

        [Fact]
        public void Projection_ShouldProject_ConcreteGenerics_FromTypeSelector()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);

            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterProjection<From, ITo<From>>((r, t) => typeof(To<>).MakeGenericType(t.DeclaredType));

            // Act
            var result = container.ResolveMany<ITo<From>>();

            // Assert
            Assert.Collection(result, new Action<ITo<From>>[]
            {
                r => Assert.IsType<To<From1>>(r),
                r => Assert.IsType<To<From2>>(r)
            });
        }

        [Fact]
        public void Projection_ShouldProject_UniqueSingletons_FromOneRegistration()
        {
            // The challenge here is that we have a *single* registration - a singleton - that
            // will be used to create the projection for each element.  Singletons inherently
            // track a single instance - so this is testing that the compiler is able to detect 
            // the projection and override the singleton so that it creates different instances
            // for each one.

            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);

            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterSingleton<To, ITo>();
            targets.RegisterProjection<From, ITo>();

            // Act
            var result = container.ResolveMany<ITo>();

            // Assert
            List<ITo> firstResults = new List<ITo>();
            Assert.All(result, r =>
            {
                Assert.IsType<To>(r);
                Assert.DoesNotContain(r, firstResults);
                firstResults.Add(r);
            });
            // second enumeration should yield same results
            // (Remember: enumerables are lazy by default, so usually every time
            // you enumerate you get a new instance).
            Assert.All(result, r => Assert.Contains(r, firstResults));
        }

        [Fact]
        public void Projection_ShouldProject_UniqueScoped_FromOneRegistration()
        {
            // similar again - this time for scoped objects

            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);

            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterScoped<To, ITo>();
            targets.RegisterProjection<From, ITo>();

            // Act
            using (var scope = container.CreateScope())
            {
                var result = scope.ResolveMany<ITo>();

                // Assert
                List<ITo> firstResults = new List<ITo>();
                Assert.All(result, r =>
                {
                    Assert.IsType<To>(r);
                    Assert.DoesNotContain(r, firstResults);
                    firstResults.Add(r);
                });

                Assert.All(result, r => Assert.Contains(r, firstResults));
            }
        }

        [Fact]
        public void Projection_ShouldProject_WithDecoratedRegistration()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);

            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterType(typeof(To<>), typeof(ITo<>));
            targets.RegisterDecorator(typeof(ToDecorator<>), typeof(ITo<>));
            targets.RegisterProjection(typeof(From), typeof(ITo), (r, t) => typeof(ITo<>).MakeGenericType(t.DeclaredType));

            // Act
            var result = container.ResolveMany<ITo>();

            // Assert
            Assert.Collection(result,
                new Action<ITo>[]
                {
                    r =>
                    {
                        var decorator = Assert.IsType<ToDecorator<From1>>(r);
                        Assert.IsType<To<From1>>(decorator.Inner);
                    },
                    r =>
                    {
                        var decorator = Assert.IsType<ToDecorator<From2>>(r);
                        Assert.IsType<To<From2>>(decorator.Inner);
                    }
                });
        }
    }
}
