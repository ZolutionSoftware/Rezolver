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
			var target = compiler.CompileTarget(_stringObjectTarget.Value, CreateContainerScopeMock(compiler));
			Assert.IsNotNull(target);
			Assert.AreEqual(StringForObjectTarget, target.GetObject());
		}

		[TestMethod]
		public void ShouldCompileIntObjectTarget()
		{
			var compiler = CreateCompiler();
			var target = compiler.CompileTarget(_intObjectTarget.Value, CreateContainerScopeMock(compiler));
			Assert.IsNotNull(target);
			Assert.AreEqual(IntForObjectTarget, target.GetObject());

		}

		[TestMethod]
		public void ShouldCompileNullableIntObjectTarget()
		{
			var compiler = CreateCompiler();
			var target = compiler.CompileTarget(_nullableIntObjectTarget.Value, CreateContainerScopeMock(compiler));
			Assert.IsNotNull(target);
			Assert.AreEqual(NullableIntForObjectTarget, target.GetObject());
		}

		[TestMethod]
		public void ShouldCompileRequiresIntConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var container = CreateDefaultMockForContainer(compiler);
			AddIntTargetToScopeMock(container);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresInt>(), container.Object);
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof (RequiresInt));
			Assert.AreEqual(IntForObjectTarget, ((IRequiresInt) result).Int);
		}

		[TestMethod]
		public void ShouldCompileRequiresNullableIntConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var container = CreateDefaultMockForContainer(compiler);
			AddNullableIntTargetToScopeMock(container);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>(), container.Object);
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(RequiresNullableInt));
			Assert.AreEqual(NullableIntForObjectTarget, ((IRequiresNullableInt)result).NullableInt);
		}

		[TestMethod]
		public void ShouldCompileRequiresIntConstructorTarget_WithNullable()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var container = CreateDefaultMockForContainer(compiler);
			//I think we should be able to register a nullable int for an int
			AddNullableIntTargetToScopeMock(container, typeof(int));

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresInt>(), container.Object);
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(RequiresInt));
			Assert.AreEqual(NullableIntForObjectTarget, ((IRequiresInt)result).Int);
		}

		[TestMethod]
		public void ShouldCompileRequiresNullableIntConstructorTarget_WithInt()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var container = CreateDefaultMockForContainer(compiler);
			AddIntTargetToScopeMock(container, typeof(int?));

			var target = compiler.CompileTarget(ConstructorTarget.Auto<RequiresNullableInt>(), container.Object);
			var result = target.GetObject();
			Assert.IsInstanceOfType(result, typeof(RequiresNullableInt));
			Assert.AreEqual(IntForObjectTarget, ((IRequiresNullableInt)result).NullableInt);
		}

		[TestMethod]
		public void ShouldCompileTransientConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var target = compiler.CompileTarget(ConstructorTarget.Auto<Transient>(), CreateContainerScopeMock(compiler));
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
				CreateContainerScopeMock(compiler),
				ExpressionHelper.DynamicContainerParam, null);
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
			var mockContainer = CreateDefaultMockForContainer(compiler);
			AddSingletonTargetToScopeMock(mockContainer);
			AddTransientTargetToScopeMock(mockContainer);
			var target = compiler.CompileTarget(ConstructorTarget.Auto<Composite>(), mockContainer.Object);
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
			var mockContainer = CreateDefaultMockForContainer(compiler);

			AddIntTargetToScopeMock(mockContainer);
			AddNullableIntTargetToScopeMock(mockContainer);
			AddStringTargetToScopeMock(mockContainer);
			AddTransientTargetToScopeMock(mockContainer);
			AddSingletonTargetToScopeMock(mockContainer);
			AddCompositeTargetToScopeMock(mockContainer);

			var target = compiler.CompileTarget(ConstructorTarget.Auto<SuperComplex>(), mockContainer.Object);
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
		public void ShouldCompileDynamicTestTargetToObjectFunc()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var inputDynamicContainer = CreateContainerScopeMock(compiler);
			var defaultContainer = CreateContainerScopeMock(compiler);

			var target = compiler.CompileTarget(new DynamicTestTarget(defaultContainer), CreateContainerScopeMock(compiler), ExpressionHelper.DynamicContainerParam, null);

			Assert.IsNotNull(target);
			Assert.AreSame(defaultContainer, target.GetObjectDynamic(null));
			Assert.AreSame(inputDynamicContainer, target.GetObjectDynamic(inputDynamicContainer));
		}

		[TestMethod]
		public void ShouldCompileDynamicTestTargetToIRezolverContainerFunc()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var inputDynamicContainer = CreateContainerScopeMock(compiler);
			var defaultContainer = CreateContainerScopeMock(compiler);

			var target = compiler.CompileTarget(new DynamicTestTarget(defaultContainer),
				CreateContainerScopeMock(compiler), ExpressionHelper.DynamicContainerParam, null);

			Assert.IsNotNull(target);
			Assert.AreSame(defaultContainer, target.GetObjectDynamic(null));
			Assert.AreSame(inputDynamicContainer, target.GetObjectDynamic(inputDynamicContainer));
		}
	}
}