using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Compilation.Expressions.Tests
{
  public abstract partial class TargetCompilerTestsBase
  {
    //contains supporting code for all the tests in the main file.
    private readonly Lazy<ObjectTarget> _stringObjectTarget = new Lazy<ObjectTarget>(() => StringForObjectTarget.AsObjectTarget());
    private const string StringForObjectTarget = "hello";

    private readonly Lazy<ObjectTarget> _intObjectTarget = new Lazy<ObjectTarget>(() => IntForObjectTarget.AsObjectTarget());
    private const int IntForObjectTarget = 2;

    private const int MultipleForIntObjectTarget = 5;

    private readonly Lazy<ObjectTarget> _nullableIntObjectTarget =
      new Lazy<ObjectTarget>(() => NullableIntForObjectTarget.AsObjectTarget());
    private static readonly int? NullableIntForObjectTarget = 1;

    private readonly Lazy<ITarget> _requiresIntTarget
      = new Lazy<ITarget>(() => ConstructorTarget.Auto<RequiresInt>());

    private readonly Lazy<ITarget> _transientConstructorTarget
      = new Lazy<ITarget>(() => ConstructorTarget.Auto<Transient>());

    private readonly Lazy<SingletonTarget> _singletonConstructorTarget
      = new Lazy<SingletonTarget>(() => new SingletonTarget(ConstructorTarget.Auto<Singleton>()));

    private readonly Lazy<ITarget> _compositeConstructorTarget
      = new Lazy<ITarget>(() => ConstructorTarget.Auto<Composite>());

    private readonly Lazy<ITarget> _superComplexConstructorTarget
      = new Lazy<ITarget>(() => ConstructorTarget.Auto<SuperComplex>());

    private readonly Lazy<ITarget> _scopedSingletonTestTypeConstructorTarget
      = new Lazy<ITarget>(() => ConstructorTarget.Auto<ScopedSingletonTestClass>());


    protected abstract ITargetCompiler CreateCompilerBase(string callingMethod);
    protected abstract void ReleaseCompiler(ITargetCompiler compiler);

    private ITargetCompiler _compiler;
    private ITargetContainer _currentBuilder;
    private IContainer _currentRezolver;

    public TargetCompilerTestsBase()
    {

    }

    public void Dispose()
    {
      if (_compiler != null)
        ReleaseCompiler(_compiler);
    }

    protected ITargetCompiler GetCompiler([CallerMemberName]string callingMethod = null)
    {
      if (_compiler != null) return _compiler;
      return _compiler = CreateCompilerBase(callingMethod);
    }

    protected IContainer GetContainer([CallerMemberName]string callingMethod = null)
    {
      if (_currentRezolver != null) return _currentRezolver;
      return _currentRezolver = new Container(GetTargetContainer(callingMethod), GetCompiler(callingMethod));
    }

    protected ITargetContainer GetTargetContainer([CallerMemberName]string callingMethod = null)
    {
      if (_currentBuilder != null) return _currentBuilder;
      return _currentBuilder = new TargetContainer();
    }

    /// <summary>
    /// Shortcut for compiling a given container target using the test's compiler and the current container/builder for
    /// dpendency resolution.
    /// 
    /// Note - the function asserts the result is not null before returning.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    protected ICompiledTarget CompileTarget(ITarget target, [CallerMemberName]string callingMethod = null)
    {
      var result = GetCompiler(callingMethod).CompileTarget(target, CreateCompileContext(callingMethod));
      Assert.NotNull(result);
      return result;
    }

    protected ResolveContext CreateRezolveContext(Type rezolveType, IScopedContainer scope = null, [CallerMemberName]string callingMethod = null)
    {
      return new ResolveContext(GetContainer(callingMethod), rezolveType, scope);
    }

    protected ResolveContext CreateRezolveContext<TRezolve>(IScopedContainer scope = null, [CallerMemberName]string callingMethod = null)
    {
      return CreateRezolveContext(typeof(TRezolve), scope, callingMethod);
    }

    protected CompileContext CreateCompileContext([CallerMemberName]string callingMethod = null)
    {
      return new CompileContext(GetContainer(callingMethod), GetTargetContainer(callingMethod));
    }

    protected void AddIntTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_intObjectTarget.Value, forType ?? typeof(int));
    }

    protected void AddNullableIntTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_nullableIntObjectTarget.Value, forType ?? typeof(int?));
    }

    protected void AddStringTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_stringObjectTarget.Value, forType ?? typeof(string));
    }

    protected void AddTransientTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_transientConstructorTarget.Value, forType ?? typeof(ITransient));
    }

    protected void AddSingletonTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_singletonConstructorTarget.Value, forType ?? typeof(ISingleton));
    }

    protected void AddCompositeTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_compositeConstructorTarget.Value, forType ?? typeof(IComposite));
    }

    protected void AddSuperComplexTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_superComplexConstructorTarget.Value, forType ?? typeof(ISuperComplex));
    }

    protected void AddScopedSingletonTestType(Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      GetTargetContainer(callingMethod).Register(_scopedSingletonTestTypeConstructorTarget.Value, forType ?? typeof(ScopedSingletonTestClass));
    }

    protected void AddTarget(ITarget target, Type forType = null, [CallerMemberName]string callingMethod = null)
    {
      Assert.NotNull(target);
      GetTargetContainer(callingMethod).Register(target, forType);
    }
  }
}
