using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext.Compilation
{
	public abstract partial class RezolveTargetCompilerTestsBase : TestsBase, IDisposable
	{
		//Both a unit test and integration test suite for implementations of IRezolveTargetCompiler.

		//Derive from this class and implement the abstract methods, and the test suite will then be run.

		//Look in the sub-files .Supporting.cs and .TestTypes.cs for the rest.

		//NOTES ON THE TESTS 
		//Most of these tests focus on one type of IRezolveTarget implementation to be compiled. That target 
		//is created directly and passed to the compiler directly.  
		//But a CompileContext is then created which passes the in-scope Rezolver to the compiler for dependency lookup.

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
			var result = target.GetObject(CreateRezolveContext<RequiresInt>());
			Assert.IsType<RequiresInt>(result);
			Assert.Equal(IntForObjectTarget, ((RequiresInt)result).Int);
		}

		[Fact]
		public void ShouldCompileRequiresNullableIntConstructorTarget()
		{
			AddNullableIntTarget();
			var target = CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>());
			var result = target.GetObject(CreateRezolveContext<RequiresNullableInt>());
			Assert.IsType<RequiresNullableInt>(result);
			Assert.Equal(NullableIntForObjectTarget, ((RequiresNullableInt)result).NullableInt);
		}

		[Fact]
		public void ShouldCompileRequiresNullableIntConstructorTarget_WithInt()
		{
			AddIntTarget(typeof(int?));

			var target = CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>());
			var result = target.GetObject(CreateRezolveContext<RequiresNullableInt>());
			Assert.IsType<RequiresNullableInt>(result);
			Assert.Equal(IntForObjectTarget, ((RequiresNullableInt)result).NullableInt);
		}

		[Fact]
		public void ShouldCompileRequiresIntConstructorTarget_WithNullable()
		{
			//I think we should be able to register a nullable int for an int
			AddNullableIntTarget(typeof(int));

			var target = CompileTarget(ConstructorTarget.Auto<RequiresInt>());
			var result = target.GetObject(CreateRezolveContext<RequiresInt>());
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
			var target = CompileTarget(RezolveTargetAdapter.Instance.GetRezolveTarget(c => new RequiresInt(MultiplyAnInt(c.Resolve<int>()))));
			var result = target.GetObject(CreateRezolveContext<RequiresInt>());
			Assert.IsType<RequiresInt>(result);
			Assert.Equal(IntForObjectTarget * MultipleForIntObjectTarget, ((RequiresInt)result).Int);
		}

		[Fact]
		public void ShouldCompileConstructorCallWithPropertyResolved()
		{
			AddIntTarget();
			AddTarget(ConstructorTarget.Auto<RequiresInt>(), typeof(IRequiresInt));
			var target = CompileTarget(RezolveTargetAdapter.Instance.GetRezolveTarget(c => new HasProperty() { RequiresInt = c.Resolve<IRequiresInt>() }));
			var result = target.GetObject(CreateRezolveContext<HasProperty>());
			Assert.IsType<HasProperty>(result);
			Assert.NotNull(((HasProperty)result).RequiresInt);
			Assert.Equal(IntForObjectTarget, ((HasProperty)result).RequiresInt.Int);
		}

		[Fact]
		public void ShouldCompileTransientConstructorTarget()
		{
			//TODO: This test won't work as written because XUnit runner overlaps tests.
			var target = CompileTarget(ConstructorTarget.Auto<Transient>());
			var lastCount = Transient.Counter;
			var result = target.GetObject(CreateRezolveContext<Transient>());
			Assert.NotNull(result);
			Assert.IsType<Transient>(result);
			Assert.Equal(lastCount + 1, Transient.Counter);
		}

		[Fact]
		public void ShouldCompileSingletonConstructorTarget()
		{
			IRezolveTargetCompiler compiler = GetCompiler();
			var target = compiler.CompileTarget(new SingletonTarget(ConstructorTarget.Auto<Singleton>()),
				CreateCompileContext(compiler));
			Assert.NotNull(target);

			var lastCount = Singleton.Counter = 0;
			var result = target.GetObject();
			Assert.NotNull(result);
			Assert.IsInstanceOfType(result, typeof(Singleton));
			Assert.Equal(lastCount + 1, Singleton.Counter);
			var result2 = target.GetObject();
			Assert.Equal(lastCount + 1, Singleton.Counter);
		}

		[Fact]
		public void ShouldCompileCompositeConstructorTarget()
		{
			IRezolveTargetCompiler compiler = GetCompiler();
			//need a special mock for this
			var mocks = CreateDefaultMockForRezolver(compiler);
			AddSingletonTargetToMocks(mocks);
			AddTransientTargetToMocks(mocks);
			var target = compiler.CompileTarget(ConstructorTarget.Auto<Composite>(), CreateCompileContext(mocks));
			Assert.NotNull(target);
			var lastSingletonCount = Singleton.Counter = 0;
			var lastTransientCount = Transient.Counter;
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(Composite));
			var result2 = (IComposite)result;
			Assert.IsInstanceOfType(result2.Singleton, typeof(Singleton));
			Assert.IsInstanceOfType(result2.Transient, typeof(Transient));
			Assert.Equal(++lastSingletonCount, Singleton.Counter);
			Assert.Equal(++lastTransientCount, Transient.Counter);
			var result3 = target.GetObject();
			Assert.AreNotSame(result, result3);
			Assert.Equal(lastSingletonCount, Singleton.Counter); //this one shouldn't increment
			Assert.Equal(++lastTransientCount, Transient.Counter);
		}

		[Fact]
		public void ShouldCompileSuperComplexConstructorTarget()
		{
			IRezolveTargetCompiler compiler = GetCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);

			AddIntTargetToMocks(mocks);
			AddNullableIntTargetToMocks(mocks);
			AddStringTargetToMocks(mocks);
			AddTransientTargetToMocks(mocks);
			AddSingletonTargetToMocks(mocks);
			AddCompositeTargetToMocks(mocks);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<SuperComplex>(), CreateCompileContext(mocks));
			Assert.NotNull(target);
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(SuperComplex));
			var result2 = (ISuperComplex)result;
			Assert.Equal(IntForObjectTarget, result2.Int);
			Assert.Equal(NullableIntForObjectTarget, result2.NullableInt);
			Assert.Equal(StringForObjectTarget, result2.String);
			Assert.NotNull(result2.Transient);
			Assert.NotNull(result2.Singleton);
			Assert.NotNull(result2.Composite);
			Assert.NotNull(result2.Composite.Transient);
			Assert.AreNotSame(result2.Transient, result2.Composite.Transient);
			Assert.NotNull(result2.Composite.Singleton);
			Assert.AreSame(result2.Singleton, result2.Composite.Singleton);
		}

		[Fact]
		public void ShouldCompileScopedSingletonTarget()
		{
			IRezolveTargetCompiler compiler = GetCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);
			mocks.RezolverMock.Setup(r => r.CreateLifetimeScope()).Returns(() => new CombinedLifetimeScopeRezolver(null, inner: mocks.RezolverMock.Object));

			var target = compiler.CompileTarget(new ScopedTarget(ConstructorTarget.Auto<ScopedSingletonTestClass>()), CreateCompileContext(mocks)); ;

			ScopedSingletonTestClass obj1;
			ScopedSingletonTestClass obj2;

			using (var scope = new CombinedLifetimeScopeRezolver(null, inner: mocks.RezolverMock.Object))
			{
				target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope));

				obj1 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope));
				obj2 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope));
				Assert.NotNull(obj1);
				Assert.AreSame(obj1, obj2);
				using (var scope2 = scope.CreateLifetimeScope())
				{
					obj2 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope2));

					Assert.AreNotSame(obj1, obj2);
				}
			}
		}

		public static int IntForStaticExpression = 15;
		[Fact]
		public void ShouldCompileTargetForStaticProperty()
		{
			IRezolveTargetCompiler compiler = GetCompiler();

			var target = compiler.CompileTarget(RezolveTargetAdapter.Default.GetRezolveTarget(c => IntForStaticExpression), CreateCompileContext(CreateDefaultMockForRezolver(compiler)));
			int result = (int)target.GetObject();
			Assert.Equal(IntForStaticExpression, result);
		}
	}
}
