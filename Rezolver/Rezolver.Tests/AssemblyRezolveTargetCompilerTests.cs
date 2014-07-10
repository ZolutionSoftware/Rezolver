using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class AssemblyRezolveTargetCompilerTests
	{
		public class ToRezolve
		{
			public static int InstanceCount = 0;
			public ToRezolve()
			{
				InstanceCount++;
			}
		}

		[TestMethod]
		public void ShouldCompileCompiledTarget()
		{
			IRezolveTargetCompiler2 compiler = new AssemblyRezolveTargetCompiler();

			ConstructorTarget constructorTarget = ConstructorTarget.Auto<ToRezolve>();
			var rezolverContainer = Mock.Of<IRezolverContainer>();
			ICompiledRezolveTarget target = compiler.CompileTarget(constructorTarget,  rezolverContainer, ExpressionHelper.DynamicContainerParam, null);
			ToRezolve.InstanceCount = 0; 
			var toRezolve = (ToRezolve)target.GetObject();
			Assert.AreEqual(1, ToRezolve.InstanceCount);
			Assert.IsNotNull(toRezolve);

			var toRezolve2 = (ToRezolve) target.GetObjectDynamic(null);
			Assert.AreEqual(2, ToRezolve.InstanceCount);
			Assert.IsNotNull(toRezolve2);

			Assert.AreNotSame(toRezolve, toRezolve2);

			var del
			 = RezolveTargetCompiler.Default.CompileStatic(constructorTarget, rezolverContainer, null);

			object benchResult = null;

			Stopwatch s = new Stopwatch();
			int counter = 1000*1000;
			s.Start();
			while (counter-- != 0)
			{
				benchResult = new ToRezolve();
			}
			s.Stop();
			Console.WriteLine("Direct create took {0}ms", s.Elapsed.TotalMilliseconds);

			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = 1000 * 1000;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = del();
			}
			s.Stop();
			Console.WriteLine("Create via delegate took {0}ms", s.Elapsed.TotalMilliseconds);

			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = 1000 * 1000;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = target.GetObject();
			}
			s.Stop();
			Console.WriteLine("Create via dynamic interface impl took {0}ms", s.Elapsed.TotalMilliseconds);
		}
	}

	public interface IRezolveTargetCompiler2
	{
		ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolverContainer containerScope, ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack);
	}

	public interface ICompiledRezolveTarget
	{
		object GetObject();
		object GetObjectDynamic(IRezolverContainer dynamicContainer);
	}

	public class AssemblyRezolveTargetCompiler : IRezolveTargetCompiler2
	{
		private static int _assemblyCounter = 0;

		private int _targetCounter = 0;
		private readonly AssemblyBuilder _assemblyBuilder;
		private readonly ModuleBuilder _moduleBuilder;

		public AssemblyRezolveTargetCompiler()
		{
			_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(
				string.Format("Rezolver.AssemblyRezolveTargetCompiler.Compiled{0}, Culture=neutral, Version=0.0.0.0",
					++_assemblyCounter)), AssemblyBuilderAccess.RunAndCollect);

			_moduleBuilder = _assemblyBuilder.DefineDynamicModule("DefaultModule");
		}

		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack)
		{
			var typeBuilder = _moduleBuilder.DefineType(string.Format("Target_{0}_{1}", target.GetType().Name, ++_targetCounter));
			typeBuilder.AddInterfaceImplementation(typeof(ICompiledRezolveTarget));

			var staticGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, containerScope, targetStack, typeBuilder);
			var staticGetObjectDynamicMethod = CreateStatic_GetObjectDynamicMethod(target, containerScope, targetStack, typeBuilder);

			var methodBuilder = CreateInstance_GetObjectMethod(typeBuilder, staticGetObjectStaticMethod);
			typeBuilder.DefineMethodOverride(methodBuilder, typeof (ICompiledRezolveTarget).GetMethod("GetObject"));

			methodBuilder = CreateInstance_GetObjectDynamicMethod(typeBuilder, staticGetObjectDynamicMethod);
			typeBuilder.DefineMethodOverride(methodBuilder, typeof (ICompiledRezolveTarget).GetMethod("GetObjectDynamic"));

			var type = typeBuilder.CreateType();
			return (ICompiledRezolveTarget)Activator.CreateInstance(type);
		}

		private MethodBuilder CreateInstance_GetObjectDynamicMethod(TypeBuilder typeBuilder, MethodBuilder staticGetObjectDynamicMethod)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod("GetObjectDynamic",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
				typeof(object),
				new[] { typeof(IRezolverContainer) });

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.EmitCall(OpCodes.Call, staticGetObjectDynamicMethod, new Type[0]);
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private static MethodBuilder CreateInstance_GetObjectMethod(TypeBuilder typeBuilder,
			MethodBuilder staticGetObjectStaticMethod)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod("GetObject",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
				typeof (object),
				new Type[0]);

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.EmitCall(OpCodes.Call, staticGetObjectStaticMethod, new Type[0]);
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private MethodBuilder CreateStatic_GetObjectDynamicMethod(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack, TypeBuilder typeBuilder)
		{
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectDynamic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, typeof(object), new Type[0]);
			Expression.Lambda<Func<IRezolverContainer, object>>(target.CreateExpression(containerScope, targetType: typeof(object),
				currentTargets: targetStack, dynamicContainerExpression: ExpressionHelper.DynamicContainerParam), ExpressionHelper.DynamicContainerParam).CompileToMethod(toReturn);
			return toReturn;
		}

		private static MethodBuilder CreateStatic_GetObjectStaticMethod(IRezolveTarget target, IRezolverContainer containerScope,
			Stack<IRezolveTarget> targetStack, TypeBuilder typeBuilder)
		{
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectStatic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, typeof (object), new Type[0]);
			Expression.Lambda<Func<object>>(target.CreateExpression(containerScope, targetType: typeof (object),
				currentTargets: targetStack)).CompileToMethod(toReturn);
			return toReturn;
		}
	}
}
