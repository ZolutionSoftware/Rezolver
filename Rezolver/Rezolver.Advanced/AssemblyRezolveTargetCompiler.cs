using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Rezolver
{
	public class AssemblyRezolveTargetCompiler : IRezolveTargetCompiler
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

			Type compiledRezolverTargetGenericType = typeof(ICompiledRezolveTarget<>).MakeGenericType(target.DeclaredType);

			typeBuilder.AddInterfaceImplementation(typeof(ICompiledRezolveTarget));
			typeBuilder.AddInterfaceImplementation(compiledRezolverTargetGenericType);


			var staticGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, containerScope, targetStack, typeBuilder, typeof(object));
			var staticGetObjectDynamicMethod = CreateStatic_GetObjectDynamicMethod(target, containerScope, targetStack, typeBuilder, dynamicContainerExpression, typeof(object));
			var staticStrongGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, containerScope, targetStack, typeBuilder);
			var staticStrongGetObjectDynamicMethod = CreateStatic_GetObjectDynamicMethod(target, containerScope, targetStack, typeBuilder, dynamicContainerExpression);


			var methodBuilder = CreateInstance_GetObjectMethod(typeBuilder, staticGetObjectStaticMethod, "GetObject");
			typeBuilder.DefineMethodOverride(methodBuilder, typeof(ICompiledRezolveTarget).GetMethod("GetObject"));

			methodBuilder = CreateInstance_GetObjectDynamicMethod(typeBuilder, staticGetObjectDynamicMethod, "GetObjectDynamic");
			typeBuilder.DefineMethodOverride(methodBuilder, typeof(ICompiledRezolveTarget).GetMethod("GetObjectDynamic"));

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

		private MethodBuilder CreateStatic_GetObjectDynamicMethod(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack, 
			TypeBuilder typeBuilder, ParameterExpression dynamicContainerExpression, Type targetType = null)
		{
			targetType = targetType ?? target.DeclaredType;
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectDynamic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, targetType, new Type[0]);
			Expression.Lambda(target.CreateExpression(containerScope, targetType: targetType,
				currentTargets: targetStack, dynamicContainerExpression: dynamicContainerExpression ?? ExpressionHelper.DynamicContainerParam), ExpressionHelper.DynamicContainerParam).CompileToMethod(toReturn);
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
