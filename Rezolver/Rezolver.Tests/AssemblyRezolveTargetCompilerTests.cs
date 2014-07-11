using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
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
			ICompiledRezolveTarget<ToRezolve> target2 = (ICompiledRezolveTarget<ToRezolve>)target;

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

			var del2 = RezolveTargetCompiler.Default.CompileStatic<ToRezolve>(constructorTarget, rezolverContainer);

			object benchResult = null;
			ToRezolve benchResult2 = null;
			const int counterStart = 1000*10000;
			Stopwatch s = new Stopwatch();
			int counter = counterStart;
			s.Start();
			while (counter-- != 0)
			{
				benchResult = new ToRezolve();
			}
			s.Stop();
			Console.WriteLine("Direct create (object) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Start();
			while (counter-- != 0)
			{
				benchResult2 = new ToRezolve();
			}
			s.Stop();
			Console.WriteLine("Direct create (no cast) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			//--------------

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = target.GetObject();
			}
			s.Stop();
			Console.WriteLine("Create via interface (object) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = target.GetObjectDynamic(null);
			}
			s.Stop();
			Console.WriteLine("Create via interface impl (object/with container) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult2 = target2.GetObject();
			}
			s.Stop();
			Console.WriteLine("Create via interface (ToRezolve) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult2 = target2.GetObjectDynamic(null);
			}
			s.Stop();
			Console.WriteLine("Create via interface (ToRezolve/with container) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();
			//--------------

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = del();
			}
			s.Stop();
			Console.WriteLine("Create via delegate (object) took {0}ms", s.Elapsed.TotalMilliseconds);
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult2 = del2();
			}
			s.Stop();
			Console.WriteLine("Create via delegate (ToRezolve) took {0}ms", s.Elapsed.TotalMilliseconds);

			GC.Collect(2);
			GC.WaitForFullGCComplete();

			
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

	public interface ICompiledRezolveTarget<out T> : ICompiledRezolveTarget
	{
		new T GetObject();
		new T GetObjectDynamic(IRezolverContainer dynamicContainer);
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
			var typeBuilder = _moduleBuilder.DefineType(string.Format("Target_{0}_{1}", target.DeclaredType.Name, ++_targetCounter));

			Type compiledRezolverTargetGenericType = typeof (ICompiledRezolveTarget<>).MakeGenericType(target.DeclaredType);

			typeBuilder.AddInterfaceImplementation(typeof(ICompiledRezolveTarget));
			typeBuilder.AddInterfaceImplementation(compiledRezolverTargetGenericType);


			var staticGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, containerScope, targetStack, typeBuilder, typeof(object));
			var staticGetObjectDynamicMethod = CreateStatic_GetObjectDynamicMethod(target, containerScope, targetStack, typeBuilder, typeof(object));
			var staticStrongGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, containerScope, targetStack, typeBuilder);
			var staticStrongGetObjectDynamicMethod = CreateStatic_GetObjectDynamicMethod(target, containerScope, targetStack, typeBuilder);


			var methodBuilder = CreateInstance_GetObjectMethod(typeBuilder, staticGetObjectStaticMethod, "GetObject");
			typeBuilder.DefineMethodOverride(methodBuilder, typeof (ICompiledRezolveTarget).GetMethod("GetObject"));

			methodBuilder = CreateInstance_GetObjectDynamicMethod(typeBuilder, staticGetObjectDynamicMethod, "GetObjectDynamic");
			typeBuilder.DefineMethodOverride(methodBuilder, typeof (ICompiledRezolveTarget).GetMethod("GetObjectDynamic"));

			methodBuilder = CreateInstance_GetObjectMethod(typeBuilder, staticStrongGetObjectStaticMethod, "GetStrongObject");
			typeBuilder.DefineMethodOverride(methodBuilder, compiledRezolverTargetGenericType.GetMethod("GetObject"));

			methodBuilder = CreateInstance_GetObjectDynamicMethod(typeBuilder, staticStrongGetObjectDynamicMethod, "GetStrongObjectDynamic");
			typeBuilder.DefineMethodOverride(methodBuilder, compiledRezolverTargetGenericType.GetMethod("GetObjectDynamic"));

			var type = typeBuilder.CreateType();
			return (ICompiledRezolveTarget)Activator.CreateInstance(type);
		}

		private MethodBuilder CreateInstance_GetObjectDynamicMethod(TypeBuilder typeBuilder, MethodBuilder staticGetObjectDynamicMethod, string methodName)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
				staticGetObjectDynamicMethod.ReturnType,
				new[] { typeof(IRezolverContainer) });

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.EmitCall(OpCodes.Call, staticGetObjectDynamicMethod, new Type[0]);
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private static MethodBuilder CreateInstance_GetObjectMethod(TypeBuilder typeBuilder,
			MethodBuilder staticGetObjectStaticMethod, string methodName)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
				staticGetObjectStaticMethod.ReturnType,
				new Type[0]);

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.EmitCall(OpCodes.Call, staticGetObjectStaticMethod, new Type[0]);
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private MethodBuilder CreateStatic_GetObjectDynamicMethod(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack, TypeBuilder typeBuilder, Type targetType = null)
		{
			targetType = targetType ?? target.DeclaredType;
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectDynamic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, targetType, new Type[0]);
			Expression.Lambda(target.CreateExpression(containerScope, targetType: targetType,
				currentTargets: targetStack, dynamicContainerExpression: ExpressionHelper.DynamicContainerParam), ExpressionHelper.DynamicContainerParam).CompileToMethod(toReturn);
			return toReturn;
		}

		private static MethodBuilder CreateStatic_GetObjectStaticMethod(IRezolveTarget target, IRezolverContainer containerScope,
			Stack<IRezolveTarget> targetStack, TypeBuilder typeBuilder, Type targetType = null)
		{
			targetType = targetType ?? target.DeclaredType;
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectStatic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, targetType, new Type[0]);
			Expression.Lambda(target.CreateExpression(containerScope, targetType: targetType,
				currentTargets: targetStack)).CompileToMethod(toReturn);
			return toReturn;
		}
	}
}
