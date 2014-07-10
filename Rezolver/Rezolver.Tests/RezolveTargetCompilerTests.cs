using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolveTargetCompilerTests
	{
		private const string StringForObjectTarget = "hello";

		private readonly Lazy<ObjectTarget> _stringObjectTarget = new Lazy<ObjectTarget>(() => StringForObjectTarget.AsObjectTarget());

		private ObjectTarget StringObjectTarget
		{
			get { return _stringObjectTarget.Value; }
		}

		[TestMethod]
		public void ShouldCompileObjectTargetToObjectFunc()
		{
			IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
			Func<object> func = compiler.CompileStatic(StringObjectTarget, Mock.Of<IRezolverContainer>(), 
				targetType: (Type)null, targetStack: null);
			Assert.IsNotNull(func);
			Assert.AreEqual(StringForObjectTarget, func());
		}

		[TestMethod]
		public void ShouldCompileObjectTargetToStringFunc()
		{
			IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
			Func<string> func = compiler.CompileStatic<string>(StringObjectTarget, Mock.Of<IRezolverContainer>(),
				targetStack: null);
			Assert.IsNotNull(func);
			Assert.AreEqual(StringForObjectTarget, func());
		}

		[TestMethod]
		//as long as this chucks an exception it's fine - not going to 
		//mandate what type of exception is raised.
		[ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
		public void ShouldNotCompileObjectTargetToIntFunc()
		{
			IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
			Func<object> func = compiler.CompileStatic(StringObjectTarget, Mock.Of<IRezolverContainer>(),
				targetType: typeof (int), targetStack: null);
			Debug.WriteLine(func);
		}

		[TestMethod]
		[ExpectedException(typeof (Exception), AllowDerivedTypes = true)]
		public void ShouldNotCompiledObjectTargetToIntFunc_Generic()
		{
			IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
			Func<int> func = compiler.CompileStatic<int>(StringObjectTarget, Mock.Of<IRezolverContainer>());
		}

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
				Assert.IsNotNull(dynamicContainerExpression);
				if(targetType != null && !SupportsType(targetType))
					throw new ArgumentException(string.Format("Type not supported: {0}", targetType));
				return Expression.Coalesce(Expression.Convert(dynamicContainerExpression, targetType ?? DeclaredType),
					Expression.Convert(Expression.Constant(_defaultContainer), targetType ?? DeclaredType));
			}

			public Type DeclaredType
			{
				get { return typeof(IRezolverContainer); }
			}
		}

		[TestMethod]
		public void ShouldCompileDynamicTestTargetToObjectFunc()
		{
			IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
			var inputDynamicContainer = Mock.Of<IRezolverContainer>();
			var defaultContainer = Mock.Of<IRezolverContainer>();

			Func<IRezolverContainer, object> func = compiler.CompileDynamic(new DynamicTestTarget(defaultContainer), 
				Mock.Of<IRezolverContainer>(), ExpressionHelper.DynamicContainerParam);

			Assert.IsNotNull(func);
			Assert.AreSame(defaultContainer, func(null));
			Assert.AreSame(inputDynamicContainer, func(inputDynamicContainer));
		}

		[TestMethod]
		public void ShouldCompileDynamicTestTargetToIRezolverContainerFunc()
		{
			IRezolveTargetCompiler compiler = new RezolveTargetCompiler();
			var inputDynamicContainer = Mock.Of<IRezolverContainer>();
			var defaultContainer = Mock.Of<IRezolverContainer>();

			Func<IRezolverContainer, IRezolverContainer> func = compiler.CompileDynamic<IRezolverContainer>(new DynamicTestTarget(defaultContainer),
				Mock.Of<IRezolverContainer>(), ExpressionHelper.DynamicContainerParam);

			Assert.IsNotNull(func);
			Assert.AreSame(defaultContainer, func(null));
			Assert.AreSame(inputDynamicContainer, func(inputDynamicContainer));
		}

		//TODO: add a compiler log?

		
	}



	public class SpecialRezolveTargetCompiler : IRezolveTargetCompiler
	{
		public Func<object> CompileStatic(IRezolveTarget target, IRezolverContainer containerScope, Type targetType = null,
			Stack<IRezolveTarget> targetStack = null)
		{
			throw new NotImplementedException();
		}

		public Func<TTarget> CompileStatic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack = null)
		{
			throw new NotImplementedException();
		}

		public Func<IRezolverContainer, object> CompileDynamic(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
			throw new NotImplementedException();
		}

		public Func<IRezolverContainer, TTarget> CompileDynamic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack = null)
		{
			throw new NotImplementedException();
		}
	}
}
