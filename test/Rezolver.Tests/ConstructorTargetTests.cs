using Rezolver.Tests.TestTypes;
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
    [Fact]
    public void ShouldAutomaticallyFindDefaultConstructor()
    {
      var target = ConstructorTarget.Auto<DefaultConstructor>();
      var result = GetValueFromTarget<DefaultConstructor>(target);
      Assert.Equal(DefaultConstructor.ExpectedValue, result.Value);
    }

    [Fact]
    public void ShouldFindConstructorWithOptionalParameters()
    {
      //This test demonstrates whether a constructor with all-default parameters will be treated equally
      //to a default constructor if no default constructor is present on the type.
      var target = ConstructorTarget.Auto<ConstructorWithDefaults>();
      var result = GetValueFromTarget<ConstructorWithDefaults>(target);
      Assert.Equal(ConstructorWithDefaults.ExpectedValue, result.Value);
    }

    //this test now moves into specifically selecting a constructor and extracting the parameter bindings directly
    //from the caller.  We get to automatically deriving parameter bindings for required parameters later.
    [Fact]
    public void ShouldAllowAllConstructorParametersToBeProvided()
    {
      var container = CreateADefaultRezolver();
      container.RegisterExpression(context => new NoDefaultConstructor(NoDefaultConstructor.ExpectedValue));
      var result = container.Resolve<NoDefaultConstructor>();
      Assert.Equal(NoDefaultConstructor.ExpectedValue, result.Value);
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
      var intTarget = NoDefaultConstructor.ExpectedRezolvedValue.AsObjectTarget();
      var container = CreateADefaultRezolver();
      container.RegisterExpression(context => new NoDefaultConstructor(context.Resolve<int>()));
      container.Register(intTarget, typeof(int));
      var result = container.Resolve<NoDefaultConstructor>();
      Assert.Equal(NoDefaultConstructor.ExpectedRezolvedValue, result.Value);
    }
    [Fact]
    public void ShouldAutoRezolveAConstructor()
    {
      //basically the same as above - except this doesn't provide the constructor call explicitly.
      var target = ConstructorTarget.Auto<NoDefaultConstructor>();
      var intTarget = NoDefaultConstructor.ExpectedRezolvedValue.AsObjectTarget();
      var container = CreateADefaultRezolver();
      container.Register(intTarget, typeof(int));
      var result = GetValueFromTarget<NoDefaultConstructor>(target, container);
      Assert.Equal(NoDefaultConstructor.ExpectedRezolvedValue, result.Value);
    }

    [Fact]
    public void ShouldBindConstructorJIT()
    {
      //instead of binding the constructor with the most parameters, it'll bind the constructor just-in-time based
      //on the services that are actually available from the container when the target is compiled.

      var target = ConstructorTarget.Auto<NoDefaultConstructor2>();
      var intTarget = NoDefaultConstructor2.ExpectedBestValue.AsObjectTarget();
      var container = CreateADefaultRezolver();
      container.Register(intTarget);
      container.Register(target);
      var result = container.Resolve<NoDefaultConstructor2>();
      Assert.Equal(NoDefaultConstructor2.ExpectedBestValue, result.Value);
      Assert.Equal(NoDefaultConstructor2.ExpectedDefaultMessage, result.Message);
    }

    public static int ReturnsInt(int input)
    {
      return NoDefaultConstructor.ExpectedDynamicExpressionMultiplier * input;
    }

    [Fact]
    public void ShouldAllowAPropertyToBeSet()
    {
      Container container = new Container(compiler: new TargetDelegateCompiler());
      container.RegisterExpression(c => new HasProperty() { Value = 1 });

      var result = (HasProperty)container.Resolve(typeof(HasProperty));
      Assert.Equal(1, result.Value);
    }

    [Fact]
    public void ShouldAllowAPropertyToBeResolved()
    {
      Container container = new Container(compiler: new TargetDelegateCompiler());
      container.Register((10).AsObjectTarget());
      container.RegisterExpression(c => new HasProperty() { Value = c.Resolve<int>() });
      var result = (HasProperty)container.Resolve(typeof(HasProperty));
      Assert.Equal(10, result.Value);
    }

    [Fact]
    public void ShouldAutoDiscoverProperties()
    {
      Container container = new Container(compiler: new TargetDelegateCompiler());
      container.Register((25).AsObjectTarget());
      container.Register(ConstructorTarget.Auto<HasProperty>(DefaultPropertyBindingBehaviour.Instance));
      var result = (HasProperty)container.Resolve(typeof(HasProperty));
      Assert.Equal(25, result.Value);
    }

    [Fact]
    public void ShouldAutoDiscoverFields()
    {
      Container container = new Container(compiler: new TargetDelegateCompiler());
      container.Register("Hello world".AsObjectTarget());
      container.Register(ConstructorTarget.Auto<HasField>(DefaultPropertyBindingBehaviour.Instance));
      var result = (HasField)container.Resolve(typeof(HasField));
      Assert.Equal("Hello world", result.StringField);
    }

    [Fact]
    public void ShouldIgnoreFieldsAndProperties()
    {
      Container container = new Container(compiler: new TargetDelegateCompiler());
      container.Register((100).AsObjectTarget());
      container.RegisterType<IgnoredPropertyAndField>(DefaultPropertyBindingBehaviour.Instance);
      var result = container.Resolve<IgnoredPropertyAndField>();
      Assert.Equal(1, result.GetIgnoredField());
      Assert.Equal(2, result.IgnoredProperty1);
      Assert.Equal(3, result.IgnoredProperty2);
    }

    [Fact]
    public void ShouldChainAutoDiscoveredPropertiesAndFields()
    {
      Container container = new Container(compiler: new TargetDelegateCompiler());
      container.Register("hello universe".AsObjectTarget());
      container.Register((500).AsObjectTarget());
      container.Register(ConstructorTarget.Auto<HasField>(DefaultPropertyBindingBehaviour.Instance));
      container.Register(ConstructorTarget.Auto<HasProperty>(DefaultPropertyBindingBehaviour.Instance));
      container.Register(ConstructorTarget.Auto<NestedPropertiesAndFields>(DefaultPropertyBindingBehaviour.Instance));

      var result = (NestedPropertiesAndFields)container.Resolve(typeof(NestedPropertiesAndFields));

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
      var args = new Dictionary<string, ITarget>();
      args["userName"] = userName;
      args["password"] = password;
      var target = ConstructorTarget.WithArgs<NetworkCredential>(args);
      var result = GetValueFromTarget<NetworkCredential>(target);

      Assert.Equal("username", result.UserName);
      Assert.Equal("password", result.Password);
    }
  }
}
