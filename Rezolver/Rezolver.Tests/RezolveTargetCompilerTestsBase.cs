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
		public void ShouldCompileConstructorTarget()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var target = compiler.CompileTarget(ConstructorTarget.Auto<Transient>(), CreateContainerScopeMock(compiler),
				ExpressionHelper.DynamicContainerParam, null);
			Assert.IsNotNull(target);
			var lastCount = Transient.Counter;
			var result = target.GetObject();
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(Transient));
			Assert.AreEqual(lastCount + 1, Transient.Counter);
		}

		[TestMethod]
		public void ShouldCompileConstructorTargetAsSingleton()
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
			var mockContainer = new Mock<IRezolverContainer>();
			mockContainer.Setup(r => r.Compiler).Returns(compiler);
			mockContainer.Setup(r => r.Fetch(typeof (ISingleton), null))
				.Returns(new SingletonTarget(ConstructorTarget.Auto<Singleton>()));
			mockContainer.Setup(r => r.Fetch(typeof (ITransient), null))
				.Returns(ConstructorTarget.Auto<Transient>());

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
			var mockContainer = new Mock<IRezolverContainer>();
			mockContainer.Setup(r => r.Compiler).Returns(compiler);

			AddIntObjectTargetToScopeMock(mockContainer);
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