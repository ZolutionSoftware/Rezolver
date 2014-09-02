using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			MethodCallExtractor.ExtractCalledMethod((IRezolver c) => c.CanResolve(null));

		private static readonly MethodInfo RezolverResolveMethod =
			MethodCallExtractor.ExtractCalledMethod((IRezolver c) => c.Resolve(null));

		private static readonly ConstructorInfo RezolveContextCtor =
			MethodCallExtractor.ExtractConstructorCall(() => new RezolveContext((IRezolver)null, (Type)null, (string)null, (ILifetimeScopeRezolver)null));

		private static readonly MethodInfo ContextNewContextMethod = 
			MethodCallExtractor.ExtractCalledMethod((RezolveContext context) => context.CreateNew((Type)null, (string)null));

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

			//we must then generate a conditional expression which checks whether the 
			//rezolver passed in the context (that's the parameter from the compile context) can rezolve
			//an object (optionally by the name, which might also be rezolved) and, if it can, call
			//that.  Only do this, though, if that rezolver is different to the one to which
			//the target belongs.

			//this is the underlying expression to use for the name in the compiled code
			var nameContext = new CompileContext(context, typeof(string));
			Expression nameExpr = _resolveNameTarget != null 
				? _resolveNameTarget.CreateExpression(nameContext) : Expression.Constant(null, typeof(string));
			//we also need to compile this name for static use.
			ICompiledRezolveTarget nameCompiled = _resolveNameTarget != null ?
				context.Rezolver.Compiler.CompileTarget(_resolveNameTarget,  nameContext) : null;

			//this needs to do a check to see if the inbound rezolver is different to the one we've got here
			//if it is, then we will defer to a resolve call on that rezolver.  Otherwise we will use the static
			//target, or throw an exception.

			//now we try and fetch the target from the rezolver that is passed in the context
			var staticTarget = context.Rezolver.Fetch(DeclaredType, nameCompiled != null ? (string)nameCompiled.GetObject(new RezolveContext(context.Rezolver, _resolveNameTarget.DeclaredType)) : null);
			var thisRezolver = Expression.Constant(context.Rezolver);
			var finalType = context.TargetType ?? DeclaredType;
			var finalTypeExpr = Expression.Constant(finalType, typeof(Type));
			
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
				staticExpr = Expression.Call(thisRezolver, RezolverResolveMethod, context.RezolveContextParameter);
			}

			if (staticExpr.Type != finalType)
				staticExpr = Expression.Convert(staticExpr, finalType);
			
			var newContextLocal = Expression.Parameter(typeof(RezolveContext), "newContext");
			var newContextExpr = Expression.Call(context.RezolveContextParameter, ContextNewContextMethod, finalTypeExpr, nameExpr);
			var useContextRezolverIfCanExpr = Expression.Block(finalType, new[] { newContextLocal },
				Expression.Assign(newContextLocal, newContextExpr),
				Expression.Condition(Expression.Call(context.ContextRezolverPropertyExpression, RezolverCanResolveMethod, newContextLocal),
					Expression.Convert(Expression.Call(context.ContextRezolverPropertyExpression, RezolverResolveMethod, newContextLocal), finalType),
					staticExpr
				));

//#if DEBUG
//			var expression = Expression.Condition(Expression.ReferenceEqual(context.ContextRezolverPropertyExpression, thisRezolver),
//				staticExpr,
//				useContextRezolverIfCanExpr);
//			Debug.WriteLine("RezolvedTarget expression for {0}: {1}", finalType, expression);
//			return expression;
//#else
			return Expression.IfThenElse(Expression.ReferenceEqual(context.ContextRezolverPropertyExpression, thisRezolver),
				staticExpr,
				useContextRezolverIfCanExpr);
//#endif
		}
	}
}