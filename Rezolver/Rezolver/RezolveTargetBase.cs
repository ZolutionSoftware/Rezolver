using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Abstract base class, suggested as a starting point for implementations of IRezolveTarget.
	/// </summary>
	public abstract class RezolveTargetBase : IRezolveTarget
	{
		/// <summary>
		/// Abstract method called to create the expression - this is called by <see cref="CreateExpression"/> after the
		/// <paramref name="targetType"/> has been validated, if provided.
		/// 
		/// Note - if your implementation needs to support dynamic Rezolve operations from the container that is passed
		/// to an IRezolverContainer's Rezolve method, use <see cref="ExpressionHelper.DynamicContainerParam"/> as the target
		/// expression.
		/// 
		/// You can also use the <see cref="ExpressionHelper.GetDynamicRezolveCall"/> method to help build such expressions.
		/// </summary>
		/// <param name="scopeContainer"></param>
		/// <param name="targetType"></param>
		/// <param name="currentTargets"></param>
		/// <returns></returns>
		protected abstract Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null, Stack<IRezolveTarget> currentTargets = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool SupportsType(Type type)
		{
			type.MustNotBeNull("type");
			return TypeHelpers.AreCompatible(DeclaredType, type);
		}

		/// <summary>
		/// Virtual method implementing IRezolveTarget.CreateExpression.  Rather than overriding this method,
		/// your starting point is to implement the abstract method <see cref="CreateExpressionBase"/>.
		/// </summary>
		/// <param name="containerScope">The scope in which this target is perform compile-time rezolve operations.</param>
		/// <param name="targetType">The target type of the expression.</param>
		/// <param name="currentTargets"></param>
		/// <returns></returns>
		public virtual Expression CreateExpression(IRezolverContainer containerScope, Type targetType = null, Stack<IRezolveTarget> currentTargets = null)
		{
			if (targetType != null && !SupportsType(targetType))
				throw new ArgumentException(String.Format(Exceptions.TargetDoesntSupportType_Format, targetType),
					"targetType");
			if (currentTargets != null)
			{
				if (currentTargets.Contains(this))
					throw new InvalidOperationException(string.Format(Exceptions.CyclicDependencyDetectedInTargetFormat, GetType(), DeclaredType));
			}
			else
				currentTargets = new Stack<IRezolveTarget>();

			currentTargets.Push(this);

			try
			{
				var result = CreateExpressionBase(containerScope, targetType: targetType, currentTargets: currentTargets);

				return result;
			}
			finally
			{
				currentTargets.Pop();
			}
		}

		public abstract Type DeclaredType
		{
			get;
		}
	}
}
