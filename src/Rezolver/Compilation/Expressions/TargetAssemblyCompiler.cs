// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
	//code below is disbaled at build time until I've got expression compilation working properly again
	//after which time, I will then derive this compiler from the expression generating one, throw away
	//a lot of the code found here and just hook in the core expression generation stuff with the assembly
	//generation stuff.
#if SUSPENDED && !MAXCOMPAT
	/// <summary>
	/// Implementation of the <see cref="ITargetCompiler"/> which compiles dynamic code to an assembly (which can, potentially, be saved to disk).
	/// 
	/// Suitable for environments that support the full .Net profile.
	/// 
	/// Generally, the performance of a container built using this compiler will be better than one that uses the <see cref="TargetDelegateCompiler"/>.
	/// </summary>
	public class TargetAssemblyCompiler : ITargetCompiler
	{
		private static int _assemblyCounter = 0;

		private int _targetCounter = 0;
		private readonly AssemblyBuilder _assemblyBuilder;
		private ModuleBuilder _moduleBuilder;
		private readonly string _assemblyModuleName;

		private sealed class StaticInvoker : ICompiledTarget
		{
			private Func<ResolveContext, object> _targetMethod;

			public StaticInvoker(MethodInfo targetMethod)
			{
				_targetMethod = (Func<ResolveContext, object>)Delegate.CreateDelegate(typeof(Func<ResolveContext, object>), targetMethod);
			}

			public object GetObject(ResolveContext context)
			{
				return _targetMethod(context);
				//return _targetMethod.Invoke(context);
			}
		}

		/// <summary>
		/// Gets the assembly builder whose dynamic assembly is receiving the dynamically generated code.
		/// </summary>
		/// <value>The assembly builder.</value>
		public AssemblyBuilder AssemblyBuilder
		{
			get { return _assemblyBuilder; }
		}

		/// <summary>
		/// Shortcut method for creating an assembly builder that is suitable for use with an <see cref="TargetAssemblyCompiler"/>, but
		/// with the supplied access settings (e.g. if you want to be able to save the assembly).
		/// </summary>
		/// <param name="assemblyBuilderAccess">The assembly builder access.</param>
		/// <param name="dir">If supplied, then it's the base directory that will be used when saving the dynamic dll.</param>
		/// <returns>An AssemblyBuilder instance that can be passed to the <see cref="TargetAssemblyCompiler"/> constructor.</returns>
		public static AssemblyBuilder CreateAssemblyBuilder(
			AssemblyBuilderAccess assemblyBuilderAccess = AssemblyBuilderAccess.RunAndCollect, string dir = null)
		{
			return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(
				string.Format("Rezolver.Dynamic{0}, Culture=neutral, Version=0.0.0.0",
					++_assemblyCounter)), assemblyBuilderAccess, dir);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetAssemblyCompiler"/> class.
		/// </summary>
		public TargetAssemblyCompiler()
			: this(CreateAssemblyBuilder())
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetAssemblyCompiler"/> class.
		/// </summary>
		/// <param name="assemblyBuilder">The assembly builder into which the dynamically generated code will be compiled.</param>
		/// <exception cref="System.ArgumentNullException">assemblyBuilder is <c>null</c></exception>
		public TargetAssemblyCompiler(AssemblyBuilder assemblyBuilder)
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

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetAssemblyCompiler"/> class.
		/// </summary>
		/// <param name="moduleBuilder">The module builder - if the assembly builder is already being used for something else 
		/// and you want the dynamic code for the container to be compiled into a specific module within that assembly.</param>
		public TargetAssemblyCompiler(ModuleBuilder moduleBuilder)
		{
			_assemblyModuleName = moduleBuilder.Name;
			_assemblyBuilder = ((AssemblyBuilder)moduleBuilder.Assembly);
			_moduleBuilder = moduleBuilder;
		}

		/// <summary>
		/// Creates and builds a compiled target for the passed rezolve target which can then be used to
		/// create/obtain the object(s) it produces.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="context">The current compilation context.</param>
		/// <returns>A compiled target that produces the object represented by <paramref name="target" />.</returns>
		public ICompiledTarget CompileTarget(ITarget target, CompileContext context)
		{
			var temp = string.Format("Target_{0}_{1}", target.DeclaredType.Name, ++_targetCounter);
			var typeBuilder = _moduleBuilder.DefineType(temp);

			//when there's no target type or if it's a value type, then the 
			//delegate binding won't work because a box operation is required
			// when it's a reference type it doesn't matter.
			if ((context.TargetType ?? target.DeclaredType).IsValueType)
				context = new CompileContext(context, typeof(object), inheritSharedExpressions: false);
			else
				context = new CompileContext(context, context.TargetType, inheritSharedExpressions: false);

			var staticGetObjectStaticMethod = CreateStatic_GetObjectStaticMethod(target, context, typeBuilder);

			var type = typeBuilder.CreateType();

			return new StaticInvoker(type.GetMethod("GetObjectStatic", BindingFlags.NonPublic | BindingFlags.Static));
		}

		internal void CompileTargetToMethod(ITarget target, CompileContext context, MethodBuilder methodBuilder)
		{
      throw new NotImplementedException();
			//var toBuild = target.CreateExpression(context);

			////if we have shared conditionals, then we want to try and reorder them as the intention
			////of the use of shared expressions is to consolidate them into one.  We do this on the boolean
			////expressions that might be used as tests for conditionals
			//var sharedConditionalTests = context.SharedExpressions.Where(e => e.Type == typeof(Boolean)).ToArray();
			//if (sharedConditionalTests.Length != 0)
			//	toBuild = new ConditionalRewriter(toBuild, sharedConditionalTests).Rewrite();

			////now we have to rewrite the expression to life all constants out and feed them in from an additional
			////parameter that we also dynamically compile.
			//var rewriter = new ConstantRewriter(_moduleBuilder, toBuild);
			//toBuild = rewriter.LiftConstants();

			//toBuild = toBuild.Optimise();

			////shared locals are local variables generated by targets that would normally be duplicated
			////if multiple targets of the same type are used in one compiled target.  By sharing them,
			////they reduce the size of the stack required for any generated code, but in turn 
			////the compiler is required to lift them out and add them to an all-encompassing BlockExpression
			////surrounding all the code - otherwise they won't be in scope.
			//var sharedLocals = context.SharedExpressions.OfType<ParameterExpression>().ToArray();
			//if (sharedLocals.Length != 0)
			//	toBuild = Expression.Block(toBuild.Type, sharedLocals, toBuild);

			//Expression.Lambda(toBuild, context.RezolveContextExpression).CompileToMethod(methodBuilder);
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
				new[] { typeof(IContainer) });

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
				new[] { typeof(ResolveContext) });

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

		private MethodBuilder CreateStatic_GetObjectStaticMethod(ITarget target, CompileContext context, TypeBuilder typeBuilder)
		{
			//targetType = context.TargetType ?? target.DeclaredType;
			if (context.TargetType == null)
				context = new CompileContext(context, target.DeclaredType, inheritSharedExpressions: false);
			MethodBuilder toReturn = typeBuilder.DefineMethod("GetObjectStatic",
				MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard, context.TargetType, new Type[0]);

			CompileTargetToMethod(target, context, toReturn);
			return toReturn;
		}
	}
#endif
}
