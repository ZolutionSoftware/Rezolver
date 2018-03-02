using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        //tests in here are based on similar scenarios to the Binding tests
        //for ConstructorTarget in the main test suite - except there are fewer, because
        //there are actually very few unique cases for a compiler to deal with when it comes
        //to executing constructors.

        //Most of the complexity comes from things like RezolvedTargets etc.

        [Fact]
        public void Constructor_ImplicitCtor()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<NoCtor>();
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<NoCtor>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Constructor_OneParamCtor_WithResolvedArg()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<OneCtor>();
            targets.RegisterObject(OneCtor.ExpectedValue);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<OneCtor>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OneCtor.ExpectedValue, result.Value);
        }

        [Fact]
        public void Constructor_TwoParamCtor_WithResolvedArgs()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<TwoCtors>();
            targets.RegisterObject(10);
            targets.RegisterObject("hello world");
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<TwoCtors>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.I);
            Assert.Equal("hello world", result.S);
        }

        [Fact]
        public void Constructor_ShouldAutoInjectIResolveContext()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<RequiresResolveContext>();

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<RequiresResolveContext>();

            // Assert
            Assert.NotNull(result.Context);

        }

        [Fact]
        public void Constructor_CtorSelectedByNamedArgs()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var target = Target.ForType<TwoCtors>(new { s = Target.ForObject("hello world") });
            targets.Register(target);

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<TwoCtors>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("hello world", result.S);

        }
    }
}
