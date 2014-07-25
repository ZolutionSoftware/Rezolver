using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	public abstract class RezolveTargetCompilerTestsBase
	{
		public interface ITransient
		{
		}

		public class Transient : ITransient
		{
			public static int Counter = 0;

			public Transient()
			{
				++Counter;
			}
		}

		public interface ISingleton { }

		public class Singleton : ISingleton
		{
			public static int Counter = 0;

			public Singleton()
			{
				++Counter;
			}
		}

		public interface IComposite
		{
			ISingleton Singleton { get; }
			ITransient Transient { get; }
		}

		public class Composite : IComposite
		{
			private readonly ISingleton _singleton;
			private readonly ITransient _transient;

			public Composite(ISingleton singleton, ITransient transient)
			{
				_singleton = singleton;
				_transient = transient;
			}

			public ISingleton Singleton
			{
				get { return _singleton; }
			}

			public ITransient Transient
			{
				get { return _transient; }
			}
		}

		private readonly Lazy<ObjectTarget> _stringObjectTarget = new Lazy<ObjectTarget>(() => StringForObjectTarget.AsObjectTarget());
		private const string StringForObjectTarget = "hello";

		private ObjectTarget StringObjectTarget
		{
			get { return _stringObjectTarget.Value; }
		}

		protected abstract IRezolveTargetCompiler CreateCompilerBase(string callingMethod);
		protected abstract void ReleaseCompiler(IRezolveTargetCompiler compiler);

		private IRezolveTargetCompiler _currentCompiler;

		protected IRezolveTargetCompiler CreateCompiler([CallerMemberName]string callingMethod = null)
		{
			return _currentCompiler = CreateCompilerBase(callingMethod);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if(_currentCompiler != null)
				ReleaseCompiler(_currentCompiler);
		}

		protected IRezolverContainer CreateContainerScopeMock(IRezolveTargetCompiler compiler)
		{
			Assert.IsNotNull(compiler, "The compiler must be passed when setting up the container mock for this test");
			var mock = new Mock<IRezolverContainer>();
			mock.Setup(c => c.Compiler).Returns(compiler);
			return mock.Object;
		}

		[TestMethod]
		public void ShouldCompileObjectTargetToObjectFunc()
		{
			var compiler = CreateCompiler();
			ICompiledRezolveTarget target = compiler.CompileTarget(StringObjectTarget, CreateContainerScopeMock(compiler), ExpressionHelper.DynamicContainerParam, null);
			Assert.IsNotNull(target);
			Assert.AreEqual(StringForObjectTarget, target.GetObject());
		}
		
		[TestMethod]
		public void ShouldCompileObjectTargetToStringFunc()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var target = compiler.CompileTarget(StringObjectTarget, CreateContainerScopeMock(compiler), ExpressionHelper.DynamicContainerParam, null);
			Assert.IsNotNull(target);
			Assert.AreEqual(StringForObjectTarget, target.GetObject());

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

		//[TestMethod]
		////as long as this chucks an exception it's fine - not going to 
		////mandate what type of exception is raised.
		//[ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
		//public void ShouldNotCompileObjectTargetToIntFunc()
		//{
		//	IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
		//	Func<object> func = compiler.CompileTarget();
		//	Debug.WriteLine(func);
		//}

		//[TestMethod]
		//[ExpectedException(typeof (Exception), AllowDerivedTypes = true)]
		//public void ShouldNotCompiledObjectTargetToIntFunc_Generic()
		//{
		//	IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
		//	Func<int> func = compiler.CompileStatic<int>(StringObjectTarget, CreateContainerScopeMock(compiler));
		//}

		/// <summary>
		/// Special test target for the dynamic tests - returns the dynamic container that is passed
		/// into the delegate that is built from the target, or a default if that dynamic container is
		/// passed as null.
		/// </summary>
		public class DynamicTestTarget : IRezolveTarget
		{
			private readonly IRezolverContainer _defaultContainer;

			public DynamicTestTarget(IRezolverContainer defaultContainer)
			{
				_defaultContainer = defaultContainer;
			}

			public bool SupportsType(Type type)
			{
				return type.IsAssignableFrom(typeof(IRezolverContainer));
			}

			public Expression CreateExpression(IRezolverContainer containerScope, Type targetType = null,
				ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> currentTargets = null)
			{
				//this method isn't always called with a dynamicContainerExpression passed
				if(targetType != null && !SupportsType(targetType))
					throw new ArgumentException(string.Format("Type not supported: {0}", targetType));
				if (dynamicContainerExpression != null)
				{
					return Expression.Coalesce(Expression.Convert(dynamicContainerExpression, targetType ?? DeclaredType),
						Expression.Convert(Expression.Constant(_defaultContainer, typeof(IRezolverContainer)), targetType ?? DeclaredType));
				}
				
				return Expression.Convert(Expression.Constant(_defaultContainer, typeof(IRezolverContainer)), targetType ?? DeclaredType);
			}

			public Type DeclaredType
			{
				get { return typeof(IRezolverContainer); }
			}
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