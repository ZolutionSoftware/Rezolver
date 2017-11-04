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


    public static class TargetContainerExtensions
    {
        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets)
        {
            RegisterProjection<TFrom, TTo, TTo>(targets);
        }

        public static void RegisterProjection<TFrom, TTo, TImplementation>(this IRootTargetContainer targets)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), typeof(TImplementation));
        }

        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets, Func<IRootTargetContainer, ITarget, Type> implementationTypeSelector)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), implementationTypeSelector);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromService, Type toService)
        {
            RegisterProjection(targets, fromService, toService, toService);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Type implementationType)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            RegisterProjection(targets, fromType, toType, (r, t) => implementationType);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, Type> implementationTypeSelector)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (fromType == null)
                throw new ArgumentNullException(nameof(fromType));
            if (toType == null)
                throw new ArgumentNullException(nameof(toType));

            if (implementationTypeSelector == null)
                implementationTypeSelector = (r, t) => toType;

            RegisterProjectionInternal(targets, fromType, toType, (r, t) =>
            {
                var implementationType = implementationTypeSelector(r, t);
                if (implementationType == null)
                    throw new InvalidOperationException($"Implementation type returned for projection from { fromType } to { toType } for target { t } returned null");
                // REVIEW: Cache the .ForType result on a per-type basis? It's container-agnostic.
                var target = r.Fetch(implementationType);
                return target != null && !target.UseFallback ? target : Target.ForType(implementationType);
            });
        }

        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), implementationTargetFactory);
        }

        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjectionInternal(targets ?? throw new ArgumentNullException(nameof(targets)),
                fromType ?? throw new ArgumentNullException(nameof(fromType)),
                toType ?? throw new ArgumentNullException(nameof(toType)),
                implementationTargetFactory ?? throw new ArgumentNullException(nameof(implementationTargetFactory)));
        }

        private static void RegisterProjectionInternal(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            targets.RegisterContainer(
                typeof(IEnumerable<>).MakeGenericType(toType),
                new ProjectionTargetContainer(targets, fromType, toType, implementationTargetFactory));
        }
    }
}
