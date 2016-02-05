using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext.Compilation
{
	public abstract partial class RezolveTargetCompilerTestsBase 
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

		private readonly Lazy<IRezolveTarget> _requiresIntTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<RequiresInt>());

		private readonly Lazy<IRezolveTarget> _transientConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<Transient>());

		private readonly Lazy<SingletonTarget> _singletonConstructorTarget
			= new Lazy<SingletonTarget>(() => new SingletonTarget(ConstructorTarget.Auto<Singleton>()));

		private readonly Lazy<IRezolveTarget> _compositeConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<Composite>());

		private readonly Lazy<IRezolveTarget> _superComplexConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<SuperComplex>());

		private readonly Lazy<IRezolveTarget> _scopedSingletonTestTypeConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<ScopedSingletonTestClass>());


		protected abstract IRezolveTargetCompiler CreateCompilerBase(string callingMethod);
		protected abstract void ReleaseCompiler(IRezolveTargetCompiler compiler);

		private IRezolveTargetCompiler _compiler;
		private IRezolverBuilder _currentBuilder;
		private IRezolver _currentRezolver;

		public RezolveTargetCompilerTestsBase()
		{

		}

		public void Dispose()
		{
			if (_compiler != null)
				ReleaseCompiler(_compiler);
		}

		protected IRezolveTargetCompiler GetCompiler([CallerMemberName]string callingMethod = null)
		{
			if (_compiler != null) return _compiler;
			return _compiler = CreateCompilerBase(callingMethod);
		}

		protected IRezolver GetRezolver([CallerMemberName]string callingMethod = null)
		{
			if (_currentRezolver != null) return _currentRezolver;
			return _currentRezolver = new DefaultRezolver(GetDefaultBuilder(callingMethod), GetCompiler(callingMethod));
		}

		protected IRezolverBuilder GetDefaultBuilder([CallerMemberName]string callingMethod = null)
		{
			if (_currentBuilder != null) return _currentBuilder;
			return _currentBuilder = new RezolverBuilder();
		}

		/// <summary>
		/// Shortcut for compiling a given rezolver target using the test's compiler and the current rezolver/builder for
		/// dpendency resolution.
		/// 
		/// Note - the function asserts the result is not null before returning.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected ICompiledRezolveTarget CompileTarget(IRezolveTarget target, [CallerMemberName]string callingMethod = null)
		{
			var result = GetCompiler(callingMethod).CompileTarget(target, CreateCompileContext(callingMethod));
			Assert.NotNull(result);
			return result;
		}

		protected RezolveContext CreateRezolveContext(Type rezolveType, ILifetimeScopeRezolver scope = null, [CallerMemberName]string callingMethod = null)
		{
			return new RezolveContext(GetRezolver(callingMethod), rezolveType, scope);
		}

		protected RezolveContext CreateRezolveContext<TRezolve>(ILifetimeScopeRezolver scope = null, [CallerMemberName]string callingMethod = null)
		{
			return CreateRezolveContext(typeof(TRezolve), scope, callingMethod);
		}

		protected CompileContext CreateCompileContext([CallerMemberName]string callingMethod = null)
		{
			return new CompileContext(GetRezolver(callingMethod));
		}

		protected void AddIntTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_intObjectTarget.Value, forType ?? typeof(int));
		}

		protected void AddNullableIntTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_nullableIntObjectTarget.Value, forType ?? typeof(int?));
		}

		protected void AddStringTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_stringObjectTarget.Value, forType ?? typeof(string));
		}

		protected void AddTransientTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_transientConstructorTarget.Value, forType ?? typeof(ITransient));
		}

		protected void AddSingletonTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_singletonConstructorTarget.Value, forType ?? typeof(ISingleton));
		}

		protected void AddCompositeTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_compositeConstructorTarget.Value, forType ?? typeof(IComposite));
		}

		protected void AddSuperComplexTarget(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_superComplexConstructorTarget.Value, forType ?? typeof(ISuperComplex));
		}

		protected void AddScopedSingletonTestType(Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			GetRezolver(callingMethod).Register(_scopedSingletonTestTypeConstructorTarget.Value, forType ?? typeof(ScopedSingletonTestClass));
		}

		protected void AddTarget(IRezolveTarget target, Type forType = null, [CallerMemberName]string callingMethod = null)
		{
			Assert.NotNull(target);
			GetRezolver(callingMethod).Register(target, forType);
		}
	}
}
