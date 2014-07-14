using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	public abstract class RezolveTargetCompilerTestsBase
	{
		private readonly Lazy<ObjectTarget> _stringObjectTarget = new Lazy<ObjectTarget>(() => StringForObjectTarget.AsObjectTarget());
		private const string StringForObjectTarget = "hello";

		private ObjectTarget StringObjectTarget
		{
			get { return _stringObjectTarget.Value; }
		}

		[TestMethod]
		public void ShouldCompileObjectTargetToObjectFunc()
		{
			var compiler = CreateCompiler();
			ICompiledRezolveTarget target = compiler.CompileTarget(StringObjectTarget, Mock.Of<IRezolverContainer>(), ExpressionHelper.DynamicContainerParam, null);
			Assert.IsNotNull(target);
			Assert.AreEqual(StringForObjectTarget, target.GetObject());
		}

		protected abstract IRezolveTargetCompiler CreateCompiler();
		
		[TestMethod]
		public void ShouldCompileObjectTargetToStringFunc()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var target = compiler.CompileTarget(StringObjectTarget, Mock.Of<IRezolverContainer>(), ExpressionHelper.DynamicContainerParam, null);
			Assert.IsNotNull(target);
			Assert.AreEqual(StringForObjectTarget, target.GetObject());
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
		//	Func<int> func = compiler.CompileStatic<int>(StringObjectTarget, Mock.Of<IRezolverContainer>());
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
			IRezolveTargetCompiler compiler = CreateCompiler();
			var inputDynamicContainer = Mock.Of<IRezolverContainer>();
			var defaultContainer = Mock.Of<IRezolverContainer>();

			var target = compiler.CompileTarget(new DynamicTestTarget(defaultContainer), Mock.Of<IRezolverContainer>(), ExpressionHelper.DynamicContainerParam, null);

			Assert.IsNotNull(target);
			Assert.AreSame(defaultContainer, target.GetObjectDynamic(null));
			Assert.AreSame(inputDynamicContainer, target.GetObjectDynamic(inputDynamicContainer));
		}

		[TestMethod]
		public void ShouldCompileDynamicTestTargetToIRezolverContainerFunc()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();
			var inputDynamicContainer = Mock.Of<IRezolverContainer>();
			var defaultContainer = Mock.Of<IRezolverContainer>();

			var target = compiler.CompileTarget(new DynamicTestTarget(defaultContainer),
				Mock.Of<IRezolverContainer>(), ExpressionHelper.DynamicContainerParam, null);

			Assert.IsNotNull(target);
			Assert.AreSame(defaultContainer, target.GetObjectDynamic(null));
			Assert.AreSame(inputDynamicContainer, target.GetObjectDynamic(inputDynamicContainer));
		}
	}
}