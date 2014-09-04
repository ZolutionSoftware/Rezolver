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

		private sealed class StaticInvoker : ICompiledRezolveTarget
		{
			private Func<RezolveContext, object> _targetMethod;

			public StaticInvoker(MethodInfo targetMethod)
			{
				_targetMethod = (Func<RezolveContext, object>)Delegate.CreateDelegate(typeof(Func<RezolveContext, object>), targetMethod);
			}

			public object GetObject(RezolveContext context)
			{
				return _targetMethod(context);
				//return _targetMethod.Invoke(context);
			}
		}

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

		public AssemblyRezolveTargetCompiler()
			: this(CreateAssemblyBuilder())
		{

		}

		public AssemblyRezolveTargetCompiler(AssemblyBuilder assemblyBuilder)
		{
			if (assemblyBuilder == null) throw new ArgumentNullException("assemblyBuilder");
			_assemblyModuleName = assemblyBuilder.GetName().Name;
			_assemblyBuilder = assemblyBuilder;
			try
			{
				_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyModuleName, _assemblyModuleName + ".dll");
			}
			catch (NotSupportedException)
			{
				_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyModuleName);
			}
		}

		public AssemblyRezolveTargetCompiler(ModuleBuilder moduleBuilder)
		{
			_assemblyModuleName = moduleBuilder.Name;
			_assemblyBuilder = ((AssemblyBuilder)moduleBuilder.Assembly);
			_moduleBuilder = moduleBuilder;
		}

		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, CompileContext context)
		{
			var temp = string.Format("Target_{0}_{1}", target.DeclaredType.Name, ++_targetCounter);
			var typeBuilder = _moduleBuilder.DefineType(temp);

			//when there's no target type or if it's a value type, then the 
			//delegate binding won't work because a box operation is required
			// when it's a reference type it doesn't matter.
			if ((context.TargetType ?? target.DeclaredType).IsValueType)
				context = new CompileContext(context, typeof(object), false);
			else
				context = new CompileContext(context, context.TargetType, false);

			var staticGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, context, typeBuilder);

			var type = typeBuilder.CreateType();

			return new StaticInvoker(type.GetMethod("GetObjectStatic", BindingFlags.NonPublic | BindingFlags.Static));
		}

		internal void CompileTargetToMethod(IRezolveTarget target, CompileContext context, MethodBuilder methodBuilder)
		{
			var toBuild = target.CreateExpression(context);

			//now we have to rewrite the expression to life all constants out and feed them in from an additional
			//parameter that we also dynamically compile.
			var rewriter = new ConstantRewriter(_moduleBuilder, toBuild);
			var rewritten = rewriter.LiftConstants();
			var sharedLocals = context.SharedLocals.ToArray();
			if (sharedLocals.Length != 0)
				rewritten = Expression.Block(rewritten.Type, sharedLocals, rewritten);
			Expression.Lambda(rewriter.LiftConstants(), context.RezolveContextParameter).CompileToMethod(methodBuilder);
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
				ConstantFieldMapping mapping = _mappings.FirstOrDefault(cfm => object.ReferenceEquals(cfm.Original, node) || (object.ReferenceEquals(cfm.Original.Value, node.Value) && cfm.Original.Type == node.Type));

				if (mapping == null)
				{
					//var helperBuilder = _constantProviderTypeBuilder.Value;
					//create a field on the type with the same type as the constant, with a dynamic name
					var field = _helperTypeBuilder.Value.DefineField(string.Format("_c{0}", ++_constantCounter), node.Type, FieldAttributes.Public | FieldAttributes.Static);
					mapping = new ConstantFieldMapping() { Field = field, Original = node };
					_mappings.Add(mapping);
				}
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
				new[] { typeof(RezolveContext) });

			var ilgen = methodBuilder.GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.EmitCall(OpCodes.Call, staticGetObjectStaticMethod, new Type[0]);
			if (staticGetObjectStaticMethod.ReturnType.IsValueType)
				ilgen.Emit(OpCodes.Box, staticGetObjectStaticMethod.ReturnType);
			//else
			//	ilgen.Emit(OpCodes.Castclass, typeof (object));
			ilgen.Emit(OpCodes.Ret);
			return methodBuilder;
		}

		private MethodBuilder CreateStatic_GetObjectStaticMethod(IRezolveTarget target, CompileContext context, TypeBuilder typeBuilder)
		{
			//targetType = context.TargetType ?? target.DeclaredType;
			if (context.TargetType == null)
				context = new CompileContext(context, target.DeclaredType);
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectStatic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, context.TargetType, new Type[0]);

			CompileTargetToMethod(target, context, toReturn);
			return toReturn;
		}
	}
}
