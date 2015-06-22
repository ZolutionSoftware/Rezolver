using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	public abstract partial class RezolveTargetCompilerTestsBase
	{
		[TestCleanup]
		public void TestCleanup()
		{
			if (_currentCompiler != null)
				ReleaseCompiler(_currentCompiler);
		}

		[TestMethod]
		public void ShouldCompileStringObjectTarget()
		{
			var compiler = CreateCompiler();
			var target = compiler.CompileTarget(_stringObjectTarget.Value, CreateCompileContext(compiler));
			Assert.IsNotNull(target);
			Assert.AreEqual(StringForObjectTarget, target.GetObject(new RezolveContext(Mock.Of<IRezolver>(), typeof(string))));
		}

		private CompileContext CreateCompileContext(IRezolveTargetCompiler compiler)
		{
			return new CompileContext(CreateRezolverMock(compiler));
		}

		private CompileContext CreateCompileContext(IRezolver rezolver)
		{
			return new CompileContext(rezolver);
		}

		private CompileContext CreateCompileContext(Mocks mocks)
		{
			return new CompileContext(mocks.RezolverMock.Object);
		}

		[TestMethod]
		public void ShouldCompileIntObjectTarget()
		{
			var compiler = CreateCompiler();
			var target = compiler.CompileTarget(_intObjectTarget.Value, CreateCompileContext(compiler));
			Assert.IsNotNull(target);
			Assert.AreEqual(IntForObjectTarget, target.GetObject(new RezolveContext(Mock.Of<IRezolver>(), typeof(int))));

		}

		[TestMethod]
		public void ShouldCompileNullableIntObjectTarget()
		{
			var compiler = CreateCompiler();
			var target = compiler.CompileTarget(_nullableIntObjectTarget.Value, CreateCompileContext(compiler));
			Assert.IsNotNull(target);
			Assert.AreEqual(NullableIntForObjectTarget, target.GetObject());
		}

		[TestMethod]
		public void ShouldCompileRequiresIntConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);
			AddIntTargetToMocks(mocks);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresInt>(), CreateCompileContext(mocks));
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof (RequiresInt));
			Assert.AreEqual(IntForObjectTarget, ((IRequiresInt) result).Int);
		}

		[TestMethod]
		public void ShouldCompileRequiresNullableIntConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);
			AddNullableIntTargetToMocks(mocks);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>(), CreateCompileContext(mocks));
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(RequiresNullableInt));
			Assert.AreEqual(NullableIntForObjectTarget, ((IRequiresNullableInt)result).NullableInt);
		}

		[TestMethod]
		public void ShouldCompileRequiresIntConstructorTarget_WithNullable()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);
			//I think we should be able to register a nullable int for an int
			AddNullableIntTargetToMocks(mocks, typeof(int));

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresInt>(), CreateCompileContext(mocks));
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(RequiresInt));
			Assert.AreEqual(NullableIntForObjectTarget, ((IRequiresInt)result).Int);
		}

		[TestMethod]
		public void ShouldCompileRequiresNullableIntConstructorTarget_WithInt()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);
			AddIntTargetToMocks(mocks, typeof(int?));

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>(), CreateCompileContext(mocks));
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(RequiresNullableInt));
			Assert.AreEqual(IntForObjectTarget, ((IRequiresNullableInt)result).NullableInt);
		}

		[TestMethod]
		public void ShouldCompileRequiresIntConstructorTarget_WithExtantMethodCallMixedIn()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: CreateCompiler());
			rezolver.Register(_intObjectTarget.Value);
			rezolver.Register(ConstructorTarget.For(context => new RequiresInt(MultiplyAnInt(context.Resolve<int>()))));
			var result = (RequiresInt)rezolver.Resolve(typeof(RequiresInt));

			Assert.AreEqual(IntForObjectTarget * MultipleForIntObjectTarget, result.Int);
		}

		public static int MultiplyAnInt(int input)
		{
			return input * MultipleForIntObjectTarget;
		}

		[TestMethod]
		public void ShouldCompileConstructorCallWithPropertyResolved()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: CreateCompiler());
			rezolver.Register(_intObjectTarget.Value);
			rezolver.Register(ConstructorTarget.Auto<RequiresInt>(), typeof(IRequiresInt));
			rezolver.RegisterExpression(c => new HasProperty() { RequiresInt = c.Resolve<IRequiresInt>() });

			var result = (HasProperty)rezolver.Resolve(typeof(HasProperty));
			Assert.IsNotNull(result.RequiresInt);

			Assert.AreEqual(IntForObjectTarget, result.RequiresInt.Int);
		}

		[TestMethod]
		public void ShouldCompileTransientConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var target = compiler.CompileTarget(ConstructorTarget.Auto<Transient>(), CreateCompileContext(compiler));
			Assert.IsNotNull(target);
			var lastCount = Transient.Counter;
			var result = target.GetObject();
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(Transient));
			Assert.AreEqual(lastCount + 1, Transient.Counter);
		}

		[TestMethod]
		public void ShouldCompileSingletonConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var target = compiler.CompileTarget(new SingletonTarget(ConstructorTarget.Auto<Singleton>()),
				CreateCompileContext(compiler));
			Assert.IsNotNull(target);

			var lastCount = Singleton.Counter = 0;
			var result = target.GetObject();
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(Singleton));
			Assert.AreEqual(lastCount + 1, Singleton.Counter);
			var result2 = target.GetObject();
			Assert.AreEqual(lastCount + 1, Singleton.Counter);
		}

		[TestMethod]
		public void ShouldCompileCompositeConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			//need a special mock for this
			var mocks = CreateDefaultMockForRezolver(compiler);
			AddSingletonTargetToMocks(mocks);
			AddTransientTargetToMocks(mocks);
			var target = compiler.CompileTarget(ConstructorTarget.Auto<Composite>(), CreateCompileContext(mocks));
			Assert.IsNotNull(target);
			var lastSingletonCount = Singleton.Counter = 0;
			var lastTransientCount = Transient.Counter;
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof (Composite));
			var result2 = (IComposite)result;
			Assert.IsInstanceOfType(result2.Singleton, typeof (Singleton));
			Assert.IsInstanceOfType(result2.Transient, typeof (Transient));
			Assert.AreEqual(++lastSingletonCount, Singleton.Counter);
			Assert.AreEqual(++lastTransientCount, Transient.Counter);
			var result3 = target.GetObject();
			Assert.AreNotSame(result, result3);
			Assert.AreEqual(lastSingletonCount, Singleton.Counter); //this one shouldn't increment
			Assert.AreEqual(++lastTransientCount, Transient.Counter);
		}

		[TestMethod]
		public void ShouldCompileSuperComplexConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);

			AddIntTargetToMocks(mocks);
			AddNullableIntTargetToMocks(mocks);
			AddStringTargetToMocks(mocks);
			AddTransientTargetToMocks(mocks);
			AddSingletonTargetToMocks(mocks);
			AddCompositeTargetToMocks(mocks);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<SuperComplex>(), CreateCompileContext(mocks));
			Assert.IsNotNull(target);
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof (SuperComplex));
			var result2 = (ISuperComplex)result;
			Assert.AreEqual(IntForObjectTarget, result2.Int);
			Assert.AreEqual(NullableIntForObjectTarget, result2.NullableInt);
			Assert.AreEqual(StringForObjectTarget, result2.String);
			Assert.IsNotNull(result2.Transient);
			Assert.IsNotNull(result2.Singleton);
			Assert.IsNotNull(result2.Composite);
			Assert.IsNotNull(result2.Composite.Transient);
			Assert.AreNotSame(result2.Transient, result2.Composite.Transient);
			Assert.IsNotNull(result2.Composite.Singleton);
			Assert.AreSame(result2.Singleton, result2.Composite.Singleton);
		}

		[TestMethod]
		public void ShouldCompileScopedSingletonTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var mocks = CreateDefaultMockForRezolver(compiler);
			mocks.RezolverMock.Setup(r => r.CreateLifetimeScope()).Returns(() => new CombinedLifetimeScopeRezolver(mocks.RezolverMock.Object));

			var target = compiler.CompileTarget(new ScopedSingletonTarget(ConstructorTarget.Auto<ScopedSingletonTestClass>()), CreateCompileContext(mocks)); ;

			ScopedSingletonTestClass obj1;
			ScopedSingletonTestClass obj2;

			using(var scope = new CombinedLifetimeScopeRezolver(mocks.RezolverMock.Object))
			{
				target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope));

				obj1 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope));
				obj2 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope));
				Assert.IsNotNull(obj1);
				Assert.AreSame(obj1, obj2);
				using (var scope2 = scope.CreateLifetimeScope())
				{
					obj2 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), scope2));

					Assert.AreSame(obj1, obj2);
				}

				//create another top-level scope - this should create a new instance
				using (var siblingScope = new CombinedLifetimeScopeRezolver(mocks.RezolverMock.Object))
				{
					obj2 = (ScopedSingletonTestClass)target.GetObject(new RezolveContext(mocks.RezolverMock.Object, typeof(ScopedSingletonTestClass), siblingScope));
					Assert.AreNotSame(obj1, obj2);
				}
			}
		}

		public static int IntForStaticExpression = 15;
		[TestMethod]
		public void ShouldCompileTargetForStaticProperty()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();

			var target = compiler.CompileTarget(RezolveTargetAdapter.Default.GetRezolveTarget(c => IntForStaticExpression), CreateCompileContext(CreateDefaultMockForRezolver(compiler)));
			int result = (int)target.GetObject();
			Assert.AreEqual(IntForStaticExpression, result);
		}
	}
}