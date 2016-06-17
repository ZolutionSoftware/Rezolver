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
		/// Gets the target that this <see cref="RezolvedTarget"/> will fallback to if a satisfactory target cannot be found
		/// at compile time.
		/// </summary>
		/// <remarks>The <see cref="ITarget.UseFallback"/> property is also used to determine whether this will be 
		/// used.  If the target resolved from the <see cref="CompileContext"/> has its <see cref="ITarget.UseFallback"/>
		/// property set to true, and this property is non-null for this target, then this target will be used.
		/// 
		/// Note also that extension containers such as <see cref="OverridingContainer"/> also have the ability to override
		/// the use of this fallback if they successfully resolve the type.
		/// </remarks>
		public ITarget FallbackTarget
		{
			get
			{
				return _fallbackTarget;
			}
		}

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
		/// Attempts to obtain the target that this <see cref="RezolvedTarget"/> resolves to for the given <see cref="CompileContext"/>.
		/// 
		/// Used in the implementation of <see cref="CreateExpressionBase(CompileContext)"/> but also available to consumers to enable
		/// checking of RezolvedTargets to see if they'll succeed at compile time (useful when late-binding overloaded constructors, 
		/// for example).
		/// </summary>
		/// <param name="context">The context from which a target is to be resolved.</param>
		/// <returns>The target resolved by this target - could be the <see cref="FallbackTarget"/>, could be null.</returns>
		/// <remarks>The target that is returned depends both on the <paramref name="context"/> passed and also whether 
		/// a <see cref="FallbackTarget"/> has been provided to this target.</remarks>
		public virtual ITarget Resolve(CompileContext context)
		{
			context.MustNotBeNull(nameof(context));

			var fromContext = context.Fetch(_resolveType);
			if (fromContext == null)
				return _fallbackTarget; //might still be null of course
			else if (fromContext.UseFallback)
				return _fallbackTarget ?? fromContext;

			return fromContext;
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

			//try to resolve the target from the context.  Note this could resolve the fallback target.
			var staticTarget = Resolve(context);
			//TODO: This should be a shared expression
			var thisRezolver = Expression.Constant(context.Rezolver, typeof(IContainer));
			var declaredTypeExpr = Expression.Constant(DeclaredType, typeof(Type));

			var newContextLocal = context.GetOrAddSharedLocal(typeof(RezolveContext), "newContext");
			var newContextExpr = Expression.Call(context.RezolveContextParameter, ContextNewContextMethod, declaredTypeExpr);
			var setNewContextLocal = Expression.Assign(newContextLocal, newContextExpr);
			bool setNewContextFirst = false;
			Expression staticExpr = null;
			if (staticTarget != null)
			{
				staticExpr = staticTarget.CreateExpression(new CompileContext(context, DeclaredType, true)); //need a new context here to change the resolve type to our declared type.
				if (staticExpr == null)
					throw new InvalidOperationException(string.Format(ExceptionResources.TargetReturnedNullExpressionFormat, staticTarget.GetType(), context.TargetType));
			}
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

			//Equivalent to (RezolveContext r) => r.Resolver.CanResolve(type) ? r.Resolver.Resolve<DeclaredType>() : <<staticExpr>>
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
		}
	}
}