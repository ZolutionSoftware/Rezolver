using System;
using System.Collections;
using System.Collections.Generic;
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
			IRezolverTargetCompiler compiler = new RezolverTargetCompiler();
			Func<object> func = compiler.CompileStatic(StringObjectTarget, Mock.Of<IRezolverContainer>(), 
				targetType: (Type)null, targetStack: null);
			Assert.IsNotNull(func);
			Assert.AreEqual(StringForObjectTarget, func());
		}

		[TestMethod]
		public void ShouldCompileObjectTargetToStringFunc()
		{
			IRezolverTargetCompiler compiler = new RezolverTargetCompiler();
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
			IRezolverTargetCompiler compiler = new RezolverTargetCompiler();
			Func<object> func = compiler.CompileStatic(StringObjectTarget, Mock.Of<IRezolverContainer>(),
				targetType: typeof (int), targetStack: null);
		}

		[TestMethod]
		[ExpectedException(typeof (Exception), AllowDerivedTypes = true)]
		public void ShouldNotCompiledObjectTargetToIntFunc_Generic()
		{
			IRezolverTargetCompiler compiler = new RezolverTargetCompiler();
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
			IRezolverTargetCompiler compiler = new RezolverTargetCompiler();
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
			IRezolverTargetCompiler compiler = new RezolverTargetCompiler();
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

	public interface IRezolverTargetCompiler
	{
		/// <summary>
		/// Compiles a delegate for rezolving an object via the given target based on the targets configured
		/// within the passed <paramref name="containerScope"/>.  The delegate will not accept a dynamic container
		/// at run time.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="containerScope"></param>
		/// <param name="targetType"></param>
		/// <param name="targetStack"></param>
		/// <returns></returns>
		Func<object> CompileStatic(IRezolveTarget target, IRezolverContainer containerScope, Type targetType = null,
			Stack<IRezolveTarget> targetStack = null);

		/// <summary>
		/// Compiles a strongly typed delegate for rezolving an object via the given target based on the targets configured 
		/// within the passed <paramref name="containerScope"/>.  The delegate will not accept a dynamic container
		/// at run time.
		/// </summary>
		/// <typeparam name="TTarget"></typeparam>
		/// <param name="target"></param>
		/// <param name="containerScope"></param>
		/// <param name="targetStack"></param>
		/// <returns></returns>
		Func<TTarget> CompileStatic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack = null);

		Func<IRezolverContainer, object> CompileDynamic(IRezolveTarget target, IRezolverContainer containerScope, 
			ParameterExpression dynamicContainerExpression, Type targetType = null, Stack<IRezolveTarget> targetStack = null);

		Func<IRezolverContainer, TTarget> CompileDynamic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack = null);


	}

	public class RezolverTargetCompiler : IRezolverTargetCompiler
	{
		public Func<object> CompileStatic(IRezolveTarget target, IRezolverContainer containerScope, Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
			return
				Expression.Lambda<Func<object>>(target.CreateExpression(containerScope, targetType: targetType, currentTargets: targetStack)).Compile();
		}

		public Func<TTarget> CompileStatic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack = null)
		{
			return
				Expression.Lambda<Func<TTarget>>(target.CreateExpression(containerScope, targetType: typeof(TTarget),
					currentTargets: targetStack)).Compile();
		}

		public Func<IRezolverContainer, object> CompileDynamic(IRezolveTarget target, IRezolverContainer containerScope, ParameterExpression dynamicContainerExpression, 
			Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
			return
				Expression.Lambda<Func<IRezolverContainer, object>>(target.CreateExpression(containerScope, targetType: targetType,
					dynamicContainerExpression: dynamicContainerExpression, currentTargets: targetStack), dynamicContainerExpression).Compile();
		}

		public Func<IRezolverContainer, TTarget> CompileDynamic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack = null)
		{
			return
				Expression.Lambda<Func<IRezolverContainer, TTarget>>(target.CreateExpression(containerScope,
					targetType: typeof (TTarget),
					dynamicContainerExpression: dynamicContainerExpression, currentTargets: targetStack), dynamicContainerExpression).Compile();
		}
	}
}
