using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rezolver.Tests
{
    public partial class TargetContainerTests
    {
        [Fact]
        public void Projection_ShouldAutoProject()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterProjection<From, To>();

            // Act
            var result = targets.Fetch(typeof(IEnumerable<To>));

            // Assert
            var enumTarget = Assert.IsType<EnumerableTarget>(result);

            Assert.Collection(enumTarget, new Action<ITarget>[]{
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.Equal(typeof(To), projTarget.DeclaredType);
                    Assert.IsType<ConstructorTarget>(projTarget.OutputTarget);
                    Assert.Equal(typeof(From1), projTarget.InputTarget.DeclaredType);
                },
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.Equal(typeof(To), projTarget.DeclaredType);
                    Assert.IsType<ConstructorTarget>(projTarget.OutputTarget);
                    Assert.Equal(typeof(From2), projTarget.InputTarget.DeclaredType);
                }
            });
        }

        [Fact]
        public void Projection_ShouldAutoProject_SpecificImplementationType()
        {
            // Projecting a different implementation type from the type that we're projecting to

            // Arrange
            var targets = new TargetContainer();
            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterProjection<From, ITo, To>();

            // Act
            var result = targets.Fetch(typeof(IEnumerable<ITo>));

            // Assert
            var enumTarget = Assert.IsType<EnumerableTarget>(result);

            Assert.Collection(enumTarget, new Action<ITarget>[]{
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.IsType<ConstructorTarget>(projTarget.OutputTarget);
                    Assert.Equal(typeof(To), projTarget.OutputTarget.DeclaredType);
                    Assert.Equal(typeof(From1), projTarget.InputTarget.DeclaredType);
                },
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.IsType<ConstructorTarget>(projTarget.OutputTarget);
                    Assert.Equal(typeof(To), projTarget.OutputTarget.DeclaredType);
                    Assert.Equal(typeof(From2), projTarget.InputTarget.DeclaredType);
                }
            });
        }
        [Fact]
        public void Projection_ShouldProject_FromRegistration()
        {
            // This time, projecting to an interface for which we have one registration
            // Demonstrates that the container will use a specific registration instead 
            // of auto-binding the implementation type.

            // Arrange
            var targets = new TargetContainer();
            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            var expectedTarget = Target.ForType<To>();
            targets.Register(expectedTarget, typeof(ITo));
            targets.RegisterProjection<From, ITo>();

            // Act
            var result = targets.Fetch(typeof(IEnumerable<ITo>));

            // Assert
            var enumTarget = Assert.IsType<EnumerableTarget>(result);

            Assert.Collection(enumTarget, new Action<ITarget>[]{
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.Same(expectedTarget, projTarget.OutputTarget);
                    Assert.Equal(typeof(From1), projTarget.InputTarget.DeclaredType);
                },
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.Same(expectedTarget, projTarget.OutputTarget);
                    Assert.Equal(typeof(From2), projTarget.InputTarget.DeclaredType);
                }
            });
        }

        [Fact]
        public void Projection_ShouldProject_FromTypeSelector()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.RegisterType<From1>();
            targets.RegisterType<From2>();
            targets.RegisterProjection<From, ITo<From>>((r, t) => typeof(To<>).MakeGenericType(t.DeclaredType));

            // Act
            var result = targets.Fetch(typeof(IEnumerable<ITo<From>>));

            // Assert
            var enumTarget = Assert.IsType<EnumerableTarget>(result);

            Assert.Collection(enumTarget, new Action<ITarget>[]{
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.IsType<ConstructorTarget>(projTarget.OutputTarget);
                    Assert.Equal(typeof(To<From1>), projTarget.OutputTarget.DeclaredType);
                    Assert.Equal(typeof(From1), projTarget.InputTarget.DeclaredType);
                },
                t => {
                    var projTarget = Assert.IsType<ProjectionTarget>(t);
                    Assert.IsType<ConstructorTarget>(projTarget.OutputTarget);
                    Assert.Equal(typeof(To<From2>), projTarget.OutputTarget.DeclaredType);
                    Assert.Equal(typeof(From2), projTarget.InputTarget.DeclaredType);
                }
            });
        }
    }
}
