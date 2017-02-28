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
			//<example1>
			var container = new Container();
			container.RegisterType<MyService>();
			container.RegisterType<RequiresMyService>();

			var result = container.Resolve<RequiresMyService>();

			Assert.NotNull(result.Service);
			//</example1>
		}

		[Fact]
		public void ShouldBuildRequiresMyServiceWithIMyService()
		{
			//<example2>
			var container = new Container();
			container.RegisterType<MyService, IMyService>();
			container.RegisterType<RequiresMyService>();

			var result = container.Resolve<RequiresMyService>();

			Assert.NotNull(result.Service);
			//</example2>
		}

		[Fact]
		public void ShouldRejectIncompatibleIMyService()
		{
			//<example3>
			var container = new Container();
			container.RegisterType<MyAlternateService, IMyService>();
			container.RegisterType<RequiresMyService>();
			// Proves that the ConstructorTarget is selecting the constructor
			// based on the available services.
			Assert.Throws<ArgumentException>("service",
				() => container.Resolve<RequiresMyService>());
			//</example3>
		}

		[Fact]
		public void ShouldBindToSmallestConstructor()
		{
			//<example4>
			var container = new Container();
			// Building ConstructorTargets directly here,
			// and using the batch-registration 'RegisterAll' method,
			// which registers against the target's DeclaredType
			container.RegisterAll(
				ConstructorTarget.Auto<RequiresMyServices>(),
				ConstructorTarget.Auto<MyService1>(),
				ConstructorTarget.Auto<MyService2>(),
				ConstructorTarget.Auto<MyService3>(),
				ConstructorTarget.Auto<MyService4>(),
				ConstructorTarget.Auto<MyService5>());

			var result = container.Resolve<RequiresMyServices>();

			// Because we didn't have MyService6 registered, the
			// container will bind the first constructor, which defaults
			// services 2-6:
			Assert.NotNull(result.Service1);
			Assert.Same(RequiresMyServices.Default2, result.Service2);
			Assert.Same(RequiresMyServices.Default3, result.Service3);
			Assert.Same(RequiresMyServices.Default4, result.Service4);
			Assert.Same(RequiresMyServices.Default5, result.Service5);
			Assert.Same(RequiresMyServices.Default6, result.Service6);
			//</example4>
		}

		[Fact]
		public void ShouldBindToConstructorWithDefaults()
		{
			//<example5>
			var container = new Container();
			container.RegisterAll(
				// Note - using ChangeTypeTarget to make the container think
				// our ConstructorTarget is still RequiresMyService
				// Could also have done:
				// container.RegisterType<RequiresMyServicesWithDefaults, RequiresMyServices>()
				new ChangeTypeTarget(
					ConstructorTarget.Auto<RequiresMyServicesWithDefaults>(),
					typeof(RequiresMyServices)
				),
				ConstructorTarget.Auto<MyService1>(),
				ConstructorTarget.Auto<MyService2>(),
				ConstructorTarget.Auto<MyService3>(),
				ConstructorTarget.Auto<MyService4>(),
				ConstructorTarget.Auto<MyService5>());

			var result = container.Resolve<RequiresMyServices>();

			//this time all but Service6 will have been injected
			Assert.NotNull(result.Service1);
			Assert.NotNull(result.Service2);
			Assert.NotNull(result.Service3);
			Assert.NotNull(result.Service4);
			Assert.NotNull(result.Service5);

			Assert.NotSame(RequiresMyServices.Default2, result.Service2);
			Assert.NotSame(RequiresMyServices.Default3, result.Service3);
			Assert.NotSame(RequiresMyServices.Default4, result.Service4);
			Assert.NotSame(RequiresMyServices.Default5, result.Service5);
			Assert.Same(RequiresMyServices.Default6, result.Service6);
			//</example5>
		}

		[Fact]
		public void ShouldBindSecondConstructorButFailToCreate()
		{
			//<example6>
			var container = new Container();
			container.RegisterType<MyService2>();
			container.RegisterType<Requires2MyServices>();

			var exception = Assert.Throws<InvalidOperationException>(
				() => container.Resolve<Requires2MyServices>());
			//the InvalidOperationException contains the name of the type that
			//couldn't be resolved
			Assert.Contains("MyService3", exception.Message);
			//</example6>
		}

		[Fact]
		public void ShouldBindSecondConstructorAndCompleteWithOverridingContainer()
		{
			//<example6_1>
			var container = new Container();
			container.RegisterType<MyService2>();
			container.RegisterType<Requires2MyServices>();

			//create an overriding container
			var containerOverride = new OverridingContainer(container);
			containerOverride.RegisterType<MyService3>();

			//resolve instance via containerOverride
			var result = containerOverride.Resolve<Requires2MyServices>();

			Assert.NotNull(result.First);
			Assert.NotNull(result.Second);
			//</example6_1>
		}

		[Fact]
		public void ShouldBindSecondConstructorAndCompleteWithChildTargetContainer()
		{
			//<example6_2>
			var targets = new TargetContainer();
			targets.RegisterType<MyService2>();
			targets.RegisterType<Requires2MyServices>();

			var childTargets = new ChildTargetContainer(targets);
			childTargets.RegisterType<MyService3>();

			//pass the childTargets ITargetContainer to 
			//the container on construction
			var container = new Container(childTargets);

			var result = container.Resolve<Requires2MyServices>();

			Assert.NotNull(result.First);
			Assert.NotNull(result.Second);
			//</example6_2>
		}

		[Fact]
		public void ShouldSelect2ParamConstructorBecauseOfNamedArgBinding()
		{
			//<example7>
			var container = new Container();
			container.RegisterType<MyService, IMyService>();
			//Currently a couple of different ways to do this - use the 
			//ConstructorTarget's constructor directly with a dictionary, 
			//or use this static method which creates a dictionary from an object
			//TODO: Will add object overload to the constructor in the future
			container.Register(ConstructorTarget.WithArgs<RequiresIMyServiceAndDateTime>(
				new
				{
					//each member of this object must be an ITarget
					startDate = DateTime.UtcNow.AddDays(1).AsObjectTarget()
				}
			));

			var result = container.Resolve<RequiresIMyServiceAndDateTime>();

			//if the datetime was used, then StartDate will be in the future
			Assert.True(result.StartDate > DateTime.UtcNow);
			//</example7>
		}

		[Fact]
		public void ShouldForceUseOfDefaultConstructorDespiteRegisteredService()
		{
			//docfx website notes: explicit examples start at #100 to allow for more to be
			//added later :)

			//<example100>
			var container = new Container();
			container.RegisterType<MyService, IMyService>();

			//under best-match, the container would select the greedy constructor,
			//but we're going to force it to use the default constructor
			container.Register(new ConstructorTarget(
				typeof(AcceptsOptionalIMyService).GetConstructor(Type.EmptyTypes)
			));

			var result = container.Resolve<AcceptsOptionalIMyService>();

			Assert.Null(result.Service);
			//</example100>
		}
	}
}
