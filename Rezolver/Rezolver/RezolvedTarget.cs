using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Versioning;
using System.Xml.Serialization;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Represents a target that is rezolved during expression building and/or at rezolve time.
	/// 
	/// That is, a target is located from the rezolver that is supplied to the CreateExpression method,
	/// and that target is then used to donate the expression.
	/// </summary>
	public class RezolvedTarget : RezolveTargetBase
	{
		private readonly Type _resolveType;
		private readonly IRezolveTarget _resolveNameTarget;

		private static readonly MethodInfo RezolverCanResolveMethod =
			MethodCallExtractor.ExtractCalledMethod((IRezolver c) => c.CanResolve(typeof(object), null, null));

		private static readonly MethodInfo RezolverResolveMethod =
			MethodCallExtractor.ExtractCalledMethod((IRezolver c) => c.Resolve(null));

		private static readonly ConstructorInfo RezolveContextCtor =
			MethodCallExtractor.ExtractConstructorCall(() => new RezolveContext((Type)null, (string)null, (IRezolver)null, (ILifetimeScopeRezolver)null));

		public IRezolveTarget Name { get { return _resolveNameTarget; } }

		internal RezolvedTarget(RezolveTargetAdapter.RezolveCallExpressionInfo rezolveCall)
		{
			_resolveType = rezolveCall.Type;
			_resolveNameTarget = rezolveCall.Name;
		}

		public RezolvedTarget(Type type, string name = null)
			: this(type, name != null ? name.AsObjectTarget() : null)
		{

		}

		public RezolvedTarget(Type type, IRezolveTarget name)
		{
			type.MustNotBeNull("type");
			_resolveType = type;
			_resolveNameTarget = name;
		}

		public override Type DeclaredType
		{
			get { return _resolveType; }
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			//we get the expression for the name that is to be rezolved.  That could be null.  If not, 
			//then we have that to bake in the generated code.

			//get the expression for the object that would be resolved statically.  If there is none,
			//then we emit a call back into the rezolver that's passed in the context.

			//we must then generate a conditional expression which checks whether the dynamic
			//rezolver passed in the context (that's the parameter from the compile context) can rezolve
			//an object (optionally by the name, which might also be rezolved) and, if it can,

			//this is the underlying expression to use for the name in the compiled code
			var nameContext = new CompileContext(context, typeof(string));
			Expression nameExpr = _resolveNameTarget != null 
				? _resolveNameTarget.CreateExpression(nameContext) : Expression.Constant(null, typeof(string));
			//we also need to compile this name for static use.
			ICompiledRezolveTarget nameCompiled = _resolveNameTarget != null ?
				context.Rezolver.Compiler.CompileTarget(_resolveNameTarget,  nameContext) : null;

			//now we try and fetch the target from the rezolver that is passed in the context
			var staticTarget = context.Rezolver.Fetch(DeclaredType, nameCompiled != null ? (string)nameCompiled.GetObject(RezolveContext.EmptyContext) : null);

			var finalType = context.TargetType ?? DeclaredType;
			var finaltypeExpr = Expression.Constant(finalType, typeof(Type));
			var nullRezolverExpr = Expression.Default(typeof(IRezolver));

			Expression staticExpr = null;
			if (staticTarget != null)
				staticExpr = staticTarget.CreateExpression(context);
			else
			{
				//represents a call back into the rezolver passed in the context to this method
				//when no target was found in the static search.  This is just a convenient way
				//to generate an exception saying that the dependency couldn't be found.
				//unless, of course, some naughty person has snuck in an additional registration
				//into the rezolver after compilation has been done ;)
				staticExpr = Expression.Call(Expression.Constant(context.Rezolver), RezolverResolveMethod, context.RezolveContextParameter); //Expression.Constant(DeclaredType), nameExpr, nullRezolverExpr);
			}

			//although for compilation purposes an expresson returning a derived type is allowed,
			//it's not for the conditional expression that I'm using below.  So a conversion must
			//be done on the 
			if (finalType != staticExpr.Type)
				staticExpr = Expression.Convert(staticExpr, finalType);

			if (context.EnableDynamicRezolver)
			{
				//represents a call to the CanResolve method of the dynamic rezolver passed to the code when executed
				var dynamicCanRezolveCallExpr = Expression.Call(context.ContextDynamicRezolverPropertyExpression, RezolverCanResolveMethod, finaltypeExpr, nameExpr, nullRezolverExpr);
				//represents a call to the Resolve method of the fynamic rezolver passed to the code when executed.  Note
				//that the return value will be converted to the exact final type of this target.
				var newRezolveContextExpr = Expression.New(RezolveContextCtor, finaltypeExpr, nameExpr, context.ContextDynamicRezolverPropertyExpression, context.ContextScopePropertyExpression);

				var dynamicRezolveCallExpr = Expression.Convert(Expression.Call(context.ContextDynamicRezolverPropertyExpression, RezolverResolveMethod, newRezolveContextExpr), finalType);
				
				//this is the code that we generate when dynamic rezolvers are supported.
				//return rezolveContext.DynamicRezolver != null && rezolveContext.DynamicRezolver.CanResolve(finalType) 
				// ? rezolveContext.DynamicRezolver.Rezolve(finalTypeExpr, nameExpr) : staticExpr;

				return Expression.Condition(
					Expression.AndAlso(Expression.ReferenceNotEqual(context.ContextDynamicRezolverPropertyExpression, nullRezolverExpr), dynamicCanRezolveCallExpr),
					/*iftrue*/ dynamicRezolveCallExpr,
					/*iffalse*/ staticExpr);
			}
			else
			{
				return staticExpr;
			}
		}
	}
}