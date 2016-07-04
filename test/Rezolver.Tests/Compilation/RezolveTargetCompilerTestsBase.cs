using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation
{
  public abstract partial class RezolveTargetCompilerTestsBase : TestsBase, IDisposable
  {
    //Both a unit test and integration test suite for implementations of IRezolveTargetCompiler.

    //Derive from this class and implement the abstract methods, and the test suite will then be run.

    //Look in the sub-files .Supporting.cs and .TestTypes.cs for the rest.

    //NOTES ON THE TESTS 
    //Most of these tests focus on one type of IRezolveTarget implementation to be compiled *and* executed afterwards. 
    //That target is created directly and passed to the compiler directly.
    //But a CompileContext is then created which passes the in-scope Rezolver to the compiler for dependency lookup -
    //that Rezolver will have other targets required for the main target to compile correctly registered within.

    //Where expression-based targets are being compiled (not as dependencies) then the framework's default 
    //RezolveTargetAdapter class is used directly through the RezolveTargetAdapter.Instance property, rather 
    //than going through the .Default property.  This is to prevent other failing tests that play with that property
    //from creating inconsistencies in these tests.

    [Fact]
    public void ShouldCompileStringObjectTarget()
    {
      var target = CompileTarget(_stringObjectTarget.Value);
      Assert.Equal(StringForObjectTarget, target.GetObject(CreateRezolveContext(typeof(string))));
    }

    [Fact]
    public void ShouldCompileIntObjectTarget()
    {
      var target = CompileTarget(_intObjectTarget.Value);
      Assert.Equal(IntForObjectTarget, target.GetObject(CreateRezolveContext(typeof(int))));
    }

    [Fact]
    public void ShouldCompileNullableIntObjectTarget()
    {
      var target = CompileTarget(_nullableIntObjectTarget.Value);
      Assert.Equal(NullableIntForObjectTarget, target.GetObject(CreateRezolveContext(typeof(int?))));
    }

    [Fact]
    public void ShouldCompileRequiresIntConstructorTarget()
    {
      AddIntTarget();
      var target = CompileTarget(ConstructorTarget.Auto<RequiresInt>());
      var result = target.GetObject(CreateRezolveContext<IRequiresInt>());
      Assert.IsType<RequiresInt>(result);
      Assert.Equal(IntForObjectTarget, ((RequiresInt)result).Int);
    }

    [Fact]
    public void ShouldCompileRequiresNullableIntConstructorTarget()
    {
      AddNullableIntTarget();
      var target = CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>());
      var result = target.GetObject(CreateRezolveContext<IRequiresNullableInt>());
      Assert.IsType<RequiresNullableInt>(result);
      Assert.Equal(NullableIntForObjectTarget, ((RequiresNullableInt)result).NullableInt);
    }

    [Fact]
    public void ShouldCompileRequiresNullableIntConstructorTarget_WithInt()
    {
      AddIntTarget(typeof(int?));

      var target = CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>());
      var result = target.GetObject(CreateRezolveContext<IRequiresNullableInt>());
      Assert.IsType<RequiresNullableInt>(result);
      Assert.Equal(IntForObjectTarget, ((RequiresNullableInt)result).NullableInt);
    }

    [Fact]
    public void ShouldCompileRequiresIntConstructorTarget_WithNullable()
    {
      //I think we should be able to register a nullable int for an int
      AddNullableIntTarget(typeof(int));

      var target = CompileTarget(ConstructorTarget.Auto<RequiresInt>());
      var result = target.GetObject(CreateRezolveContext<IRequiresInt>());
      Assert.IsType<RequiresInt>(result);
      Assert.Equal(NullableIntForObjectTarget, ((IRequiresInt)result).Int);
    }


    /// <summary>
    /// Used as the source for a constructor argument.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int MultiplyAnInt(int input)
    {
      return input * MultipleForIntObjectTarget;
    }

    [Fact]
    public void ShouldCompileRequiresIntConstructorTarget_WithExtantMethodCallMixedIn()
    {
      AddIntTarget();
      //tests whether the compiler can handle calling the static method above with an
      //injected integer input argument.
      var target = CompileTarget(TargetAdapter.Instance.CreateTarget(c => new RequiresInt(MultiplyAnInt(c.Resolve<int>()))));
      var result = target.GetObject(CreateRezolveContext<IRequiresInt>());
      Assert.IsType<RequiresInt>(result);
      Assert.Equal(IntForObjectTarget * MultipleForIntObjectTarget, ((RequiresInt)result).Int);
    }

    [Fact]
    public void ShouldCompileConstructorCallWithPropertyResolved()
    {
      AddIntTarget();
      AddTarget(ConstructorTarget.Auto<RequiresInt>(), typeof(IRequiresInt));
      var target = CompileTarget(TargetAdapter.Instance.CreateTarget(c => new HasProperty() { RequiresInt = c.Resolve<IRequiresInt>() }));
      var result = target.GetObject(CreateRezolveContext<HasProperty>());
      Assert.IsType<HasProperty>(result);
      Assert.NotNull(((HasProperty)result).RequiresInt);
      Assert.Equal(IntForObjectTarget, ((HasProperty)result).RequiresInt.Int);
    }

    [Fact]
    public void ShouldCompileTransientConstructorTarget()
    {
      using (var session = Transient.NewSession())
      {
        var target = CompileTarget(ConstructorTarget.Auto<Transient>());
        var result = target.GetObject(CreateRezolveContext<ITransient>());
        Assert.NotNull(result);
        Assert.IsType<Transient>(result);
        //second call must create a new instance.
        var result2 = target.GetObject(CreateRezolveContext<ITransient>());
        //don't bother re-checking not null/type etc.
        Assert.Equal(session.InitialInstanceCount + 2, Transient.InstanceCount);
      }
    }

    [Fact]
    public void ShouldCompileSingletonConstructorTarget()
    {
      using (var session = Singleton.NewSession())
      {
        var target = CompileTarget(ConstructorTarget.Auto<Singleton>().Singleton());
        var result = target.GetObject(CreateRezolveContext<ISingleton>());
        var result2 = target.GetObject(CreateRezolveContext<ISingleton>());

        Assert.NotNull(result);
        Assert.IsType<Singleton>(result);
        Assert.Same(result, result2);
      }
    }

    [Fact]
    public void ShouldCompileCompositeConstructorTarget()
    {
      using (ITestSession singletonSession = Singleton.NewSession(),
                          transientSession = Transient.NewSession())
      {
        AddSingletonTarget();
        AddTransientTarget();
        var target = CompileTarget(ConstructorTarget.Auto<Composite>());
        var result = target.GetObject(CreateRezolveContext<IComposite>());
        Assert.IsType<Composite>(result);
        var result2 = (IComposite)result;
        Assert.Equal(singletonSession.InitialInstanceCount + 1, Singleton.InstanceCount);
        Assert.Equal(transientSession.InitialInstanceCount + 1, Transient.InstanceCount);

        var result3 = target.GetObject(CreateRezolveContext<IComposite>());
        Assert.NotSame(result, result3);
        Assert.Equal(singletonSession.InitialInstanceCount + 1, Singleton.InstanceCount); //this one shouldn't increment
        Assert.Equal(transientSession.InitialInstanceCount + 2, Transient.InstanceCount);
      }
    }

    [Fact]
    public void ShouldCompileSuperComplexConstructorTarget()
    {
      AddIntTarget();
      AddNullableIntTarget();
      AddStringTarget();
      AddTransientTarget();
      AddSingletonTarget();
      AddCompositeTarget();

      using (ITestSession singletonSession = Singleton.NewSession(),
                          transientSession = Transient.NewSession())
      {
        var target = CompileTarget(ConstructorTarget.Auto<SuperComplex>());
        var result = target.GetObject(CreateRezolveContext<ISuperComplex>());
        Assert.IsType<SuperComplex>(result);
        var result2 = (ISuperComplex)result;
        Assert.Equal(IntForObjectTarget, result2.Int);
        Assert.Equal(NullableIntForObjectTarget, result2.NullableInt);
        Assert.Equal(StringForObjectTarget, result2.String);
        Assert.NotNull(result2.Transient);
        Assert.NotNull(result2.Singleton);
        Assert.NotNull(result2.Composite);
        Assert.NotNull(result2.Composite.Transient);
        Assert.NotSame(result2.Transient, result2.Composite.Transient);
        Assert.NotNull(result2.Composite.Singleton);
        Assert.Same(result2.Singleton, result2.Composite.Singleton);
      }
    }

    [Fact]
    public void ShouldCompileScopedSingletonTarget()
    {
      using (var session = ScopedSingletonTestClass.NewSession())
      {
        var target = CompileTarget(ConstructorTarget.Auto<ScopedSingletonTestClass>().Scoped()); ;

        ScopedSingletonTestClass obj1;
        ScopedSingletonTestClass obj2;

        using (var scope = GetRezolver().CreateLifetimeScope())
        {
          obj1 = (ScopedSingletonTestClass)target.GetObject(CreateRezolveContext<ScopedSingletonTestClass>(scope));
          obj2 = (ScopedSingletonTestClass)target.GetObject(CreateRezolveContext<ScopedSingletonTestClass>(scope));
          Assert.NotNull(obj1);
          Assert.Same(obj1, obj2);
          using (var scope2 = scope.CreateLifetimeScope())
          {
            obj2 = (ScopedSingletonTestClass)target.GetObject(CreateRezolveContext<ScopedSingletonTestClass>(scope2));

            Assert.NotSame(obj1, obj2);
          }
        }
      }
    }

    public static int IntForStaticExpression = 15;
    [Fact]
    public void ShouldCompileTargetForStaticProperty()
    {
      var target = CompileTarget(TargetAdapter.Instance.CreateTarget(c => IntForStaticExpression));
      int result = (int)target.GetObject(CreateRezolveContext<int>());
      Assert.Equal(IntForStaticExpression, result);
    }
  }
}
