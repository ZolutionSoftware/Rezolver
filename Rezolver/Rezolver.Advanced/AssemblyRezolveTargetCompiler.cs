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
		private ModuleBuilder _moduleBuilder;
		private readonly string _assemblyModuleName;

		public AssemblyBuilder AssemblyBuilder
		{
			get { return _assemblyBuilder; }
		}

		public static AssemblyBuilder CreateAssemblyBuilder(
			AssemblyBuilderAccess assemblyBuilderAccess = AssemblyBuilderAccess.RunAndCollect)
		{
			return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(
				string.Format("Rezolver.Dynamic{0}, Culture=neutral, Version=0.0.0.0",
					++_assemblyCounter)), assemblyBuilderAccess);
		}

		public AssemblyRezolveTargetCompiler() : this(CreateAssemblyBuilder())
		{

		}

		public AssemblyRezolveTargetCompiler(AssemblyBuilder assemblyBuilder)
		{
			if (assemblyBuilder == null) throw new ArgumentNullException("assemblyBuilder");
			_assemblyModuleName = assemblyBuilder.GetName().Name;
			_assemblyBuilder = assemblyBuilder;
			_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyModuleName, _assemblyModuleName + ".dll");
		}

		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolver scope,
			ParameterExpression dynamicRezolverExpression, Stack<IRezolveTarget> targetStack)
		{
			var typeBuilder = _moduleBuilder.DefineType(string.Format("Target_{0}_{1}", target.DeclaredType.Name, ++_targetCounter));

			typeBuilder.AddInterfaceImplementation(typeof(ICompiledRezolveTarget));


			var staticGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, scope, targetStack, typeBuilder, typeof(object));
			var staticGetObjectDynamicMethod = CreateStatic_GetObjectDynamicMethod(target, scope, targetStack, typeBuilder, dynamicRezolverExpression, typeof(object));

			var methodBuilder = CreateInstance_GetObjectMethod(typeBuilder, staticGetObjectStaticMethod, "GetObject");
			typeBuilder.DefineMethodOverride(methodBuilder, typeof(ICompiledRezolveTarget).GetMethod("GetObject"));

			methodBuilder = CreateInstance_GetObjectDynamicMethod(typeBuilder, staticGetObjectDynamicMethod, "GetObjectDynamic");
			typeBuilder.DefineMethodOverride(methodBuilder, typeof(ICompiledRezolveTarget).GetMethod("GetObjectDynamic"));

			var type = typeBuilder.CreateType();
			return (ICompiledRezolveTarget)Activator.CreateInstance(type);
		}

		private class ConstantRewriter : ExpressionVisitor
		{
			public class ConstantFieldMapping
			{
				public FieldInfo Field;
				public ConstantExpression Original;
			}

			public class ConstantExpressionToReplace : Expression
			{
				public ConstantFieldMapping Mapping { get; private set; }

				public ConstantExpressionToReplace(ConstantFieldMapping mapping)
				{
					Mapping = mapping;
				}

				public override ExpressionType NodeType
				{
					get { return ExpressionType.Extension; }
				}

				public override Type Type
				{
					get { return Mapping.Original.Type; }
				}
			}

			private static int _helperTypeCounter = 0;
			private readonly List<ConstantFieldMapping> _mappings = new List<ConstantFieldMapping>();
			private readonly Expression _e;
			private int _constantCounter = 0;
			private readonly Lazy<TypeBuilder> _helperTypeBuilder;
			private readonly Lazy<Type> _helperType;

			public Type HelperType
			{
				get { return _mappings.Count > 0 ? _helperType.Value : null; }
			}

			public IEnumerable<ConstantFieldMapping> Mappings { get { return _mappings; } } 

			public ConstantRewriter(ModuleBuilder parentModuleBuilder, Expression e)
			{
				_e = e;
				_helperTypeBuilder = new Lazy<TypeBuilder>(() => parentModuleBuilder.DefineType(string.Format("ConstantHelper{0}", ++_helperTypeCounter)));
				_helperType = new Lazy<Type>(() => _helperTypeBuilder.Value.CreateType());
			}

			public Expression LiftConstants()
			{
				//rewrite the expression twice if constants are to be lifted
				var expr = Visit(_e);
				if (_mappings.Count != 0)
					return Visit(expr);
				return _e;
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				//var helperBuilder = _constantProviderTypeBuilder.Value;
				//create a field on the type with the same type as the constant, with a dynamic name
				var field = _helperTypeBuilder.Value.DefineField(string.Format("_c{0}", ++_constantCounter), node.Type, FieldAttributes.Public | FieldAttributes.Static);
				var mapping = new ConstantFieldMapping() { Field = field, Original = node };
				_mappings.Add(mapping);

				return new ConstantExpressionToReplace(mapping);
			}

			protected override Expression VisitExtension(Expression node)
			{
				var replacementExpr = node as ConstantExpressionToReplace;

				if (replacementExpr != null)
				{
					var helperType = HelperType;
					if (helperType != null)
					{
						//fetch the actual field from the compiled type
						var compiledField = helperType.GetField(replacementExpr.Mapping.Field.Name);
						//write that back to the mapping
						replacementExpr.Mapping.Field = compiledField;
						//copy the value from the constant to the static field
						compiledField.SetValue(null, replacementExpr.Mapping.Original.Value);
						//return an expression representing reading that field.
						return Expression.Field(null, compiledField);
					}
				}
				return base.VisitExtension(node);
			}
		}

		private MethodBuilder CreateInstance_GetObjectDynamicMethod(TypeBuilder typeBuilder, MethodBuilder staticGetObjectDynamicMethod, string methodName)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
				typeof(object),
				new[] { typeof(IRezolver) });

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.EmitCall(OpCodes.Call, staticGetObjectDynamicMethod, new Type[0]);

			if (staticGetObjectDynamicMethod.ReturnType.IsValueType)
				ilgen.Emit(OpCodes.Box);
			
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private static MethodBuilder CreateInstance_GetObjectMethod(TypeBuilder typeBuilder,
			MethodBuilder staticGetObjectStaticMethod, string methodName)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
				typeof(object),
				new Type[0]);

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.EmitCall(OpCodes.Call, staticGetObjectStaticMethod, new Type[0]);
			if (staticGetObjectStaticMethod.ReturnType.IsValueType)
				ilgen.Emit(OpCodes.Box);
			//else
			//	ilgen.Emit(OpCodes.Castclass, typeof (object));
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private MethodBuilder CreateStatic_GetObjectDynamicMethod(IRezolveTarget target, IRezolver scope, Stack<IRezolveTarget> targetStack,
			TypeBuilder typeBuilder, ParameterExpression dynamicRezolverExpression, Type targetType = null)
		{
			targetType = targetType ?? target.DeclaredType;


			var toBuild = target.CreateExpression(scope, targetType: targetType,
				currentTargets: targetStack,
				dynamicRezolverExpression: dynamicRezolverExpression ?? ExpressionHelper.DynamicRezolverParam);

			//now we have to rewrite the expression to life all constants out and feed them in from an additional
			//parameter that we also dynamically compile.
			var rewriter = new ConstantRewriter(_moduleBuilder, toBuild);
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectDynamic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, targetType, new Type[0]);

			Expression.Lambda(rewriter.LiftConstants(),
				dynamicRezolverExpression ?? ExpressionHelper.DynamicRezolverParam).CompileToMethod(toReturn);
			return toReturn;
		}

		private MethodBuilder CreateStatic_GetObjectStaticMethod(IRezolveTarget target, IRezolver scope,
			Stack<IRezolveTarget> targetStack, TypeBuilder typeBuilder, Type targetType = null)
		{
			targetType = targetType ?? target.DeclaredType;

			var toBuild = target.CreateExpression(scope, targetType: targetType, currentTargets: targetStack);

			//now we have to rewrite the expression to life all constants out and feed them in from an additional
			//parameter that we also dynamically compile.
			var rewriter = new ConstantRewriter(_moduleBuilder, toBuild);
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectStatic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, targetType, new Type[0]);
			Expression.Lambda(rewriter.LiftConstants()).CompileToMethod(toReturn);
			return toReturn;
		}
	}
}
