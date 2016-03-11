using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class ConstructorTargetTests : TestsBase
    {
		//these tests will only work with the delegate compiler

		private class ConstructorTestClass
		{
			public int Value { get; protected set; }
		}

		private class DefaultConstructor : ConstructorTestClass
		{
			public const int ExpectedValue = -1;
			public DefaultConstructor()
			{
				Value = ExpectedValue;
			}
		}

		private class ConstructorWithDefaults : ConstructorTestClass
		{
			public const int ExpectedValue = 1;
			public ConstructorWithDefaults(int value = ExpectedValue)
			{
				Value = value;
			}
		}

		private class NoDefaultConstructor : ConstructorTestClass
		{
			public const int ExpectedRezolvedValue = 101;
			public const int ExpectedComplexNamedRezolveCall = 102;
			public const int ExpectedComplexNamedRezolveCallDynamic = 103;
			public const int ExpectedValue = 100;
			public const int ExpectedDynamicExpressionMultiplier = 5;
			public NoDefaultConstructor(int value)
			{
				Value = value;
			}
		}

		private class HasProperty
		{
			public int Value { get; set; }
		}

		private class HasField
		{
			public string StringField;
		}

		private class ShouldIgnorePropertyAndField
		{
			private int IgnoredField;
			public int GetIgnoredField()
			{
				return IgnoredField;
			}

			private int _ignoredProperty1;
			//ignorede as it's readonly
			public int IgnoredProperty1 { get { return _ignoredProperty1; } }

			public int IgnoredProperty2 { get; private set; }

			public ShouldIgnorePropertyAndField()
			{
				IgnoredField = 1;
				_ignoredProperty1 = 2;
				IgnoredProperty2 = 3;
			}
		}

		private class NestedPropertiesAndFields
		{
			public HasProperty Field_HasProperty;
			public HasField Property_HasField { get; set; }
		}

		[Fact]
		public void ShouldAutomaticallyFindDefaultConstructor()
		{
			var target = ConstructorTarget.For<DefaultConstructor>();
			var result = GetValueFromTarget<DefaultConstructor>(target);
			Assert.Equal(DefaultConstructor.ExpectedValue, result.Value);
			var result2 = GetValueFromTarget<ConstructorTestClass>(target);
			Assert.NotSame(result, result2);
		}

		[Fact]
		public void ShouldThrowArgumentExceptionIfNoDefaultConstructor()
		{
			Assert.Throws<ArgumentException>(() => ConstructorTarget.For<NoDefaultConstructor>());
		}

		[Fact]
		public void ShouldFindConstructorWithOptionalParameters()
		{
			//This test demonstrates whether a constructor with all-default parameters will be treated equally
			//to a default constructor if no default constructor is present on the type.
			var target = ConstructorTarget.For<ConstructorWithDefaults>();
			var result = GetValueFromTarget<ConstructorWithDefaults>(target);
			Assert.Equal(ConstructorWithDefaults.ExpectedValue, result.Value);
			var result2 = GetValueFromTarget<ConstructorTestClass>(target);
			Assert.NotSame(result, result2);
		}

		//this test now moves into specifically selecting a constructor and extracting the parameter bindings directly
		//from the caller.  We get to automatically deriving parameter bindings for required parameters later.
		[Fact]
		public void ShouldAllowAllConstructorParametersToBeProvided()
		{
			var target = ConstructorTarget.For(builder => new NoDefaultConstructor(NoDefaultConstructor.ExpectedValue));
			var result = GetValueFromTarget<NoDefaultConstructor>(target);
			Assert.Equal(NoDefaultConstructor.ExpectedValue, result.Value);
			var result2 = GetValueFromTarget<ConstructorTestClass>(target);
			Assert.NotSame(result, result2);
		}

		[Fact]
		public void ShouldAllowAConstructorParameterToBeExplicitlyRezolved()
		{
			//NOTE - used to use mocks, but in the end there is greater value in examining the class' behaviour with
			//the toher types in the library...

			//this is where the action starts to heat up!
			//if we can get explicitly resolved arguments to work, then we can get easily get
			//automatically injected arguments - by simply emitting the correct expression to do the same.
			//note that we pass the RezolveTargetAdapter singleton instance here to ensure consistent expression
			//parsing.  However - note that if any tests in the RezolveTargetAdapterTests suite are failing, then
			//tests like this might also fail.  I probably should isolate that - but I actually want to test ConstructorTarget's
			//integration with the default adapter here.
			var target = ConstructorTarget.For(context => new NoDefaultConstructor(context.Resolve<int>()), RezolveTargetAdapter.Instance);
			var intTarget = NoDefaultConstructor.ExpectedRezolvedValue.AsObjectTarget();
			var rezolver = CreateADefaultRezolver();
			rezolver.Register(CreateRezolverEntryForTarget(intTarget, typeof(int)));
			var result = GetValueFromTarget<NoDefaultConstructor>(target, rezolver);
			Assert.Equal(NoDefaultConstructor.ExpectedRezolvedValue, result.Value);
		}

		[Fact]
		public void ShouldAutoRezolveAConstructor()
		{
			//basically the same as above - except this doesn't provide the constructor call explicitly.
			var target = ConstructorTarget.Auto<NoDefaultConstructor>();
			var intTarget = NoDefaultConstructor.ExpectedRezolvedValue.AsObjectTarget();
			var rezolver = CreateADefaultRezolver();
			rezolver.Register(CreateRezolverEntryForTarget(intTarget, typeof(int)));
			var result = GetValueFromTarget<NoDefaultConstructor>(target, rezolver);
			Assert.Equal(NoDefaultConstructor.ExpectedRezolvedValue, result.Value);
		}

		[Fact]
		public void ShouldBindConstructorJIT()
		{
			//instead of binding the constructor with the most parameters, it'll bind the constructor just-in-time based
			//on the services that are actually available from the container when the target is compiled.


		}

		[Fact]
		public void ShouldRezolveTheStringArgumentForARezolveCall()
		{
			//complicated test, this.  And, yes, it's largely pointless - but it
			//proves that our expression parsing is recursive and can handle complex constructs
			var target = ConstructorTarget.For(context => new NoDefaultConstructor(context.Resolve<int>(context.Resolve<string>())));
			var intTarget = NoDefaultConstructor.ExpectedComplexNamedRezolveCall.AsObjectTarget();
			const string rezolveName = "ThisIsComplicated";
			var stringTarget = rezolveName.AsObjectTarget();
			var rezolver = CreateADefaultRezolver();
			rezolver.Register(CreateRezolverEntryForTarget(stringTarget, typeof(string)));
			rezolver.Register(CreateRezolverEntryForTarget(intTarget, typeof(int)));
			//so the expression demands that a new instance of NoDefaultConstructor is built. with an
			//integer constructor argument that is, in turn, resolved by a name which is also resolved.
			var result = GetValueFromTarget<NoDefaultConstructor>(target, rezolver);
			Assert.Equal(NoDefaultConstructor.ExpectedComplexNamedRezolveCall, result.Value);
		}

		[Fact]
		public void ShouldRezolveTheStringArgumentForARezolveCallFromDynamicRezolver()
		{
			//unlike the above test, this uses a real rezolver and combined rezolver
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For(context => new NoDefaultConstructor(context.Resolve<int>(context.Resolve<string>()))));
			rezolver.Register("ThisIsComplicated".AsObjectTarget());
			rezolver.Register(NoDefaultConstructor.ExpectedComplexNamedRezolveCall.AsObjectTarget(), path: "ThisIsComplicated");
			CombinedRezolver rezolver2 = new CombinedRezolver(rezolver);
			rezolver2.Register(NoDefaultConstructor.ExpectedComplexNamedRezolveCallDynamic.AsObjectTarget(), path: "ThisIsComplicated");

			//var result = (NoDefaultConstructor)rezolver.Resolve(typeof(NoDefaultConstructor));
			//Assert.Equal(NoDefaultConstructor.ExpectedComplexNamedRezolveCall, result.Value);

			var result2 = (NoDefaultConstructor)rezolver2.Resolve(typeof(NoDefaultConstructor));

			Assert.Equal(NoDefaultConstructor.ExpectedComplexNamedRezolveCallDynamic, result2.Value);
		}

		public static int ReturnsInt(int input)
		{
			return NoDefaultConstructor.ExpectedDynamicExpressionMultiplier * input;
		}

		[Fact]
		public void ShouldAllowAPropertyToBeSet()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.RegisterExpression(c => new HasProperty() { Value = 1 });

			var result = (HasProperty)rezolver.Resolve(typeof(HasProperty));
			Assert.Equal(1, result.Value);
		}

		[Fact]
		public void ShouldAllowAPropertyToBeResolved()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((10).AsObjectTarget());
			rezolver.RegisterExpression(c => new HasProperty() { Value = c.Resolve<int>() });
			var result = (HasProperty)rezolver.Resolve(typeof(HasProperty));
			Assert.Equal(10, result.Value);
		}

		[Fact]
		public void ShouldAutoDiscoverProperties()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((25).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasProperty>(DefaultPropertyBindingBehaviour.Instance));
			var result = (HasProperty)rezolver.Resolve(typeof(HasProperty));
			Assert.Equal(25, result.Value);
		}

		[Fact]
		public void ShouldAutoDiscoverFields()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register("Hello world".AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasField>(DefaultPropertyBindingBehaviour.Instance));
			var result = (HasField)rezolver.Resolve(typeof(HasField));
			Assert.Equal("Hello world", result.StringField);
		}

		[Fact]
		public void ShouldIgnoreFieldsAndProperties()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((100).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<ShouldIgnorePropertyAndField>(DefaultPropertyBindingBehaviour.Instance));
			var result = (ShouldIgnorePropertyAndField)rezolver.Resolve(typeof(ShouldIgnorePropertyAndField));
			Assert.Equal(1, result.GetIgnoredField());
			Assert.Equal(2, result.IgnoredProperty1);
			Assert.Equal(3, result.IgnoredProperty2);
		}

		[Fact]
		public void ShouldChainAutoDiscoveredPropertiesAndFields()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register("hello universe".AsObjectTarget());
			rezolver.Register((500).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasField>(DefaultPropertyBindingBehaviour.Instance));
			rezolver.Register(ConstructorTarget.Auto<HasProperty>(DefaultPropertyBindingBehaviour.Instance));
			rezolver.Register(ConstructorTarget.Auto<NestedPropertiesAndFields>(DefaultPropertyBindingBehaviour.Instance));

			var result = (NestedPropertiesAndFields)rezolver.Resolve(typeof(NestedPropertiesAndFields));

			Assert.NotNull(result.Field_HasProperty);
			Assert.NotNull(result.Property_HasField);
			Assert.Equal(500, result.Field_HasProperty.Value);
			Assert.Equal("hello universe", result.Property_HasField.StringField);
		}

		[Fact]
		public void ShouldBindExplicitParameters()
		{
			var example = new System.Net.NetworkCredential("hello", "world");
			var userName = "username".AsObjectTarget();
			var password = "password".AsObjectTarget();
			var args = new Dictionary<string, IRezolveTarget>();
			args["userName"] = userName;
			args["password"] = password;
			var target = ConstructorTarget.WithArgs<NetworkCredential>(args);
			var result = GetValueFromTarget<NetworkCredential>(target);

			Assert.Equal("username", result.UserName);
			Assert.Equal("password", result.Password);
		}
	}
}
