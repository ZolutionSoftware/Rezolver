using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        // similar to the AutoFactoryTests, except this specifically tests whether the automatic injection of Func<TResult>
        // (i.e. without explicit registration) works correctly. This does not re-test things like scoping, etc, because that's
        // already covered by the AutoFactoryTests.

        protected IRootTargetContainer CreateAutoFuncTargetContainer([CallerMemberName]string testName = null)
        {
            var config = GetDefaultTargetContainerConfig();
            config.ConfigureOption<Options.EnableAutoFuncInjection>(true);
            config.Add(Configuration.InjectAutoFuncs.Instance);
            return new TargetContainer(config);
        }

        [Fact]
        public void AutoInjectedFunc_ShouldResolveSingle()
        {
            // Arrange
            var expectedInstance = new NoCtor();
            var targets = CreateAutoFuncTargetContainer();
            targets.RegisterObject(expectedInstance);
            var container = CreateContainer(targets);

            // Act
            var func = container.Resolve<Func<NoCtor>>();

            // Assert
            Assert.NotNull(func);
            Assert.Same(expectedInstance, func());
        }

        [Fact]
        public void AutoInjectedFunc_ShouldResolveMultipleViaEnumerable()
        {
            // Arrange
            var instances = new NoCtor[] { new NoCtor(), new NoCtor(), new NoCtor() };
            var targets = CreateAutoFuncTargetContainer();
            targets.RegisterObject(instances[0]);
            targets.RegisterObject(instances[1]);
            targets.RegisterObject(instances[2]);
            var container = CreateContainer(targets);

            // Act
            var funcs = container.Resolve<IEnumerable<Func<NoCtor>>>();

            // Assert
            Assert.Equal(instances, funcs.Select(f => f()));
        }

        [Fact]
        public void AutoInjectedFunc_ShouldResolveAnEnumerable()
        {
            // Arrange
            var instances = new NoCtor[] { new NoCtor(), new NoCtor(), new NoCtor() };
            var targets = CreateAutoFuncTargetContainer();
            targets.RegisterObject(instances[0]);
            targets.RegisterObject(instances[1]);
            targets.RegisterObject(instances[2]);
            var container = CreateContainer(targets);

            // Act
            var func = container.Resolve<Func<IEnumerable<NoCtor>>>();

            // Assert
            Assert.Equal(instances, func());
        }

        [Fact]
        public void AutoInjectedFunc_ShouldCreateGeneric()
        {
            // Arrange
            var targets = CreateAutoFuncTargetContainer();
            targets.RegisterType(typeof(Generic<>));
            var container = CreateContainer(targets);

            // Act
            var func = container.Resolve<Func<Generic<int>>>();
            var result = func();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AutoInjectedFunc_ShouldCreateGenericViaInterface()
        {
            // Arrange
            var targets = CreateAutoFuncTargetContainer();
            targets.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
            var container = CreateContainer(targets);

            // Act4
            var func = container.Resolve<Func<IGeneric<int>>>();
            var result = func();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Generic<int>>(result);
        }
    }
}
