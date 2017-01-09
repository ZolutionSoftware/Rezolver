// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
		/// Always returns true - we never wrap calls to a container inside a scope tracking expression.
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
			
		}
	}
}