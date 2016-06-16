using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// Represents a target that is rezolved statically at compile time via the <see cref="CompileContext"/>, or dynamically 
	/// (at 'resolve time') from the <see cref="IContainer"/> that is attached to the current <see cref="RezolveContext"/> when 
	/// <see cref="IContainer.Resolve(RezolveContext)"/> is called.
	/// 
	/// This is the most common way that we bind constructor parameters, for example - i.e. 'I want an
	/// IService instance - go get it'.
	/// </summary>
	/// <remarks>The concept of compile-time resolving is what is typically implemented by most other IOC containers - at
	/// compile time, a target is resolved for a given type and, if found, its expression is used.  If it's not found, 
	/// then an error occurs.
	/// 
	/// Rezolver does this, but goes further when the target can't be resolved at compile-time - in this case, it will emit 
	/// a call back into the current <see cref="RezolveContext"/>'s <see cref="IContainer"/> to try and dynamically resolve 
	/// the value that is required.
	/// 
	/// Furthermore, the code it produces in either case also checks that the <see cref="IContainer"/> that is active at
	/// resolve-time is the same one (if applicable) that was active during compile-time.  If it isn't, then it'll automatically 
	/// defer resolving of the value to that container
	/// </remarks>
	public class RezolvedTarget : TargetBase
	{
		private static readonly MethodInfo RezolverCanResolveMethod =
			MethodCallExtractor.ExtractCalledMethod((IContainer c) => c.CanResolve(null));

		private static readonly MethodInfo RezolverResolveMethod =
			MethodCallExtractor.ExtractCalledMethod((IContainer c) => c.Resolve(null));

		private static readonly ConstructorInfo RezolveContextCtor =
			MethodCallExtractor.ExtractConstructorCall(() => new RezolveContext((IContainer)null, (Type)null, (IScopedContainer)null));

		private static readonly MethodInfo ContextNewContextMethod =
			MethodCallExtractor.ExtractCalledMethod((RezolveContext context) => context.CreateNew((Type)null));

		//this one cannot be obtained via expression extraction - as it uses an output parameter and there's no way of
		//modelling that to the compiler.
		private static readonly MethodInfo RezolverTryResolveMethod = TypeHelpers.GetMethod(typeof(IContainer),"TryResolve");

		private readonly Type _resolveType;
		private readonly ITarget _fallbackTarget;	

		/// <summary>
		/// Creates a new <see cref="RezolvedTarget"/> for the given <paramref name="type"/> which will attempt to 
		/// resolve a value at compile time and/or resolve-time and, if it can't, will either use the <paramref name="fallbackTarget"/>
		/// or will throw an exception.
		/// </summary>
		/// <param name="type">Required.  The type to be resolved</param>
		/// <param name="fallbackTarget">Optional.  The target to be used if the value cannot be resolved at either compile time or 
		/// resolve-time.</param>
		public RezolvedTarget(Type type, ITarget fallbackTarget = null)
		{
			type.MustNotBeNull("type");
			_resolveType = type;
			_fallbackTarget = fallbackTarget;
		}

		/// <summary>
		/// The type that will be resolved
		/// </summary>
		public override Type DeclaredType
		{
			get { return _resolveType; }
		}

		/// <summary>
		/// Always returns true - we never wrap calls to a rezolver inside a scope tracking expression.
		/// </summary>
		protected override bool SuppressScopeTracking
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns true or false based on whether this target will be able to resolve
		/// an object from the context that is passed.  Note - it's a way of dry-running
		/// the resolve operation before compiling the expression.
		/// 
		/// Please note, also, that it only works statically.  I.e. - if the dependency builder
		/// in context cannot resolve the type, then this method returns false - it will not take 
		/// into account dynamic fallback to the run time resolver.
		/// 
		/// TODO: This might be removed as the functionality for which it was added might no longer be
		/// needed.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual bool CanResolve(CompileContext context)
		{
			context.MustNotBeNull(nameof(context));
			return context.Fetch(_resolveType) != null;
		}

		/// <summary>
		/// Implementation of <see cref="TargetBase.CreateExpressionBase(CompileContext)"/>.
		/// Constructs the expression.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected override Expression CreateExpressionBase(CompileContext context)
		{
			//get the expression for the object that would be resolved statically.  If there is none,
			//then we emit a call back into the rezolver that's passed in the context.

			//we must then generate a conditional expression which checks whether the 
			//rezolver passed in the context (that's the parameter from the compile context) can rezolve
			//an object (optionally by the name, which might also be rezolved) and, if it can, call
			//that.  Only do this, though, if that rezolver is different to the one to which
			//the target belongs.

			//this needs to do a check to see if the inbound rezolver is different to the one we've got here
			//if it is, then we will defer to a resolve call on that rezolver.  Otherwise we will use the static
			//target, or throw an exception.

			//now we try and fetch the target from the rezolver that is passed in the context
			var staticTarget = context.Fetch(DeclaredType);
			//TODO: This should be a shared expression
			var thisRezolver = Expression.Constant(context.Rezolver, typeof(IContainer));
			//I did have a line that used 'context.TargetType ?? DeclaredType' but I changed this because the 
			//RezolvedTarget should know in advance which type it is that's being resolved, and that shouldn't 
			//change after creation.  It also fixed the initial set of bugs I had with resolving aliases.
			var declaredTypeExpr = Expression.Constant(DeclaredType, typeof(Type));

			var newContextLocal = context.GetOrAddSharedLocal(typeof(RezolveContext), "newContext");
			var newContextExpr = Expression.Call(context.RezolveContextParameter, ContextNewContextMethod, declaredTypeExpr);
			var setNewContextLocal = Expression.Assign(newContextLocal, newContextExpr);
			bool setNewContextFirst = false;
			Expression staticExpr = null;
			Expression defaultExpr = _fallbackTarget != null ? _fallbackTarget.CreateExpression(new CompileContext(context, DeclaredType, true)) : null;
			if (staticTarget != null)
			{
				staticExpr = staticTarget.CreateExpression(new CompileContext(context, DeclaredType, true)); //need a new context here to change the resolve type to our declared type.
				if (staticExpr == null)
					throw new InvalidOperationException(string.Format(ExceptionResources.TargetReturnedNullExpressionFormat, staticTarget.GetType(), context.TargetType));
			}
			else if (defaultExpr != null)
				staticExpr = defaultExpr; //if no target found then use the fallback
			else
			{
				//if no fallback call back into the rezolver passed in the context to this method
				//when no target was found in the static search.  This is just a convenient way
				//to generate an exception saying that the dependency couldn't be found.
				//unless, of course, some naughty person has snuck in an additional registration
				//into the rezolver after compilation has been done ;)
				setNewContextFirst = true;
				staticExpr = Expression.Call(thisRezolver, RezolverResolveMethod, newContextLocal);
			}

			if (staticExpr.Type != DeclaredType)
				staticExpr = Expression.Convert(staticExpr, DeclaredType);

			Expression useContextRezolverIfCanExpr = Expression.Condition(Expression.Call(context.ContextRezolverPropertyExpression, RezolverCanResolveMethod, newContextLocal),
					Expression.Convert(Expression.Call(context.ContextRezolverPropertyExpression, RezolverResolveMethod, newContextLocal), DeclaredType),
					staticExpr
				);

			List<Expression> blockExpressions = new List<Expression>();
			if (setNewContextFirst)
				blockExpressions.Add(setNewContextLocal);
			else
				useContextRezolverIfCanExpr = Expression.Block(DeclaredType, setNewContextLocal, useContextRezolverIfCanExpr);

			//note the use of the shared expression here - which enables an advanced optimisation specifically connected with
			//conditionals
			blockExpressions.Add(Expression.Condition(context.GetOrAddSharedExpression(typeof(bool), 
					"IsSameRezolver", 
					() => Expression.ReferenceEqual(context.ContextRezolverPropertyExpression, thisRezolver), this.GetType()),
				staticExpr,
				useContextRezolverIfCanExpr));

			if (blockExpressions.Count == 1)
				return blockExpressions[0];
			else
				return Expression.Block(DeclaredType, blockExpressions);
			//#endif
		}
	}
}