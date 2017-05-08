using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ConstructorExamples
    {

        [Fact]
        public void ShouldBuildRequiresMyService()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<MyService>();
            container.RegisterType<RequiresMyService>();

            var result = container.Resolve<RequiresMyService>();

            Assert.NotNull(result.Service);
            // </example1>
        }

        [Fact]
        public void ShouldBuildRequiresMyServiceWithIMyService()
        {
            // <example2>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            container.RegisterType<RequiresMyService>();

            var result = container.Resolve<RequiresMyService>();

            Assert.NotNull(result.Service);
            // </example2>
        }

        [Fact]
        public void ShouldRejectIncompatibleIMyService()
        {
            // <example3>
            var container = new Container();
            container.RegisterType<MyAlternateService, IMyService>();
            container.RegisterType<RequiresMyService>();
            // Proves that the ConstructorTarget is selecting the constructor
            // based on the available services.
            Assert.Throws<ArgumentException>("service",
                () => container.Resolve<RequiresMyService>());
            // </example3>
        }

        [Fact]
        public void ShouldBindToSmallestConstructor()
        {
            // <example4>
            var container = new Container();
            // Building ConstructorTargets directly here,
            // and using the batch-registration 'RegisterAll' method,
            // which registers against the target's DeclaredType
            container.RegisterAll(
                Target.ForType<RequiresMyServices>(),
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>());

            var result = container.Resolve<RequiresMyServices>();

            // Because we didn't have MyService3 registered, the
            // container will bind the first constructor, which defaults
            // services 2 & 3:
            Assert.NotNull(result.Service1);
            Assert.Same(RequiresMyServices.Default2, result.Service2);
            Assert.Same(RequiresMyServices.Default3, result.Service3);
            // </example4>
        }

        [Fact]
        public void ShouldBindToConstructorWithDefaults()
        {
            // <example5>
            var container = new Container();
            container.RegisterAll(
                // Note - using .As<T> here to create a ChangeTypeTarget
                // which makes our .ForType<T> target appear
                // to be for RequiresMyService
                // Could also have done:
                // container.RegisterType<RequiresMyServicesWithDefaults, RequiresMyServices>()
                Target.ForType<RequiresMyServicesWithDefaults>()
                    .As<RequiresMyServices>(),
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>());

            var result = container.Resolve<RequiresMyServices>();

            // this time all but Service3 will have been injected
            Assert.NotNull(result.Service1);
            Assert.NotNull(result.Service2);

            Assert.NotSame(RequiresMyServices.Default2, result.Service2);
            Assert.Same(RequiresMyServices.Default3, result.Service3);
            // </example5>
        }

        [Fact]
        public void ShouldBindSecondConstructorButFailToCreate()
        {
            // <example6>
            var container = new Container();
            container.RegisterType<MyService2>();
            container.RegisterType<Requires2MyServices>();

            var exception = Assert.Throws<InvalidOperationException>(
                () => container.Resolve<Requires2MyServices>());
            // the InvalidOperationException contains the name of the type that
            // couldn't be resolved
            Assert.Contains("MyService3", exception.Message);
            // </example6>
        }

        [Fact]
        public void ShouldBindSecondConstructorAndCompleteWithOverridingContainer()
        {
            // <example6_1>
            var container = new Container();
            container.RegisterType<MyService2>();
            container.RegisterType<Requires2MyServices>();

            // create an overriding container
            var containerOverride = new OverridingContainer(container);
            containerOverride.RegisterType<MyService3>();

            // resolve instance via containerOverride
            var result = containerOverride.Resolve<Requires2MyServices>();

            Assert.NotNull(result.First);
            Assert.NotNull(result.Second);
            // </example6_1>
        }

        [Fact]
        public void ShouldBindSecondConstructorAndCompleteWithOverridingTargetContainer()
        {
            // <example6_2>
            var targets = new TargetContainer();
            targets.RegisterType<MyService2>();
            targets.RegisterType<Requires2MyServices>();

            var childTargets = new OverridingTargetContainer(targets);
            childTargets.RegisterType<MyService3>();

            // pass the childTargets ITargetContainer to 
            // the container on construction
            var container = new Container(childTargets);

            var result = container.Resolve<Requires2MyServices>();

            Assert.NotNull(result.First);
            Assert.NotNull(result.Second);
            // </example6_2>
        }

        [Fact]
        public void ShouldSelect2ParamConstructorBecauseOfNamedArgBinding()
        {
            // <example7>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            // Currently a couple of different ways to do this - use the 
            // ConstructorTarget's constructor directly with a dictionary, 
            // or use this static method which creates a dictionary from an object
            // TODO: Will add object overload to the constructor in the future
            container.Register(Target.ForType<RequiresIMyServiceAndDateTime>(
                new
                {
                    // each member of this object must be an ITarget
                    startDate = Target.ForObject(DateTime.UtcNow.AddDays(1))
                }
            ));

            var result = container.Resolve<RequiresIMyServiceAndDateTime>();

            // if the datetime was used, then StartDate will be in the future
            Assert.True(result.StartDate > DateTime.UtcNow);
            // </example7>
        }

        [Fact]
        public void ShouldForceUseOfDefaultConstructorDespiteRegisteredService()
        {
            // docfx website notes: explicit examples start at #100 to allow for more to be
            // added later :)

            // <example100>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();

            // under best-match, the container would select the greedy constructor,
            // but we're going to force it to use the default constructor
            container.Register(new ConstructorTarget(
                typeof(AcceptsOptionalIMyService).GetConstructor(Type.EmptyTypes)
            ));

            var result = container.Resolve<AcceptsOptionalIMyService>();

            Assert.Null(result.Service);
            // </example100>
        }

        [Fact]
        public void ShouldForceUseOfSingleParamConstructorDespiteNoServices()
        {
            // <example101>
            var container = new Container();
            // get the constructor:
            var ctor = typeof(AcceptsOptionalIMyService).GetConstructor(new[]
            {
                typeof(IMyService)
            });
            // create parameter bindings
            var bindings = new[] {
                new ParameterBinding(ctor.GetParameters()[0],
                    Target.ForType<MyService>()
                )
            };

            container.Register(new ConstructorTarget(ctor, parameterBindings: bindings));

            var result = container.Resolve<AcceptsOptionalIMyService>();

            Assert.NotNull(result.Service);
            // </example101>
        }
    }
}
