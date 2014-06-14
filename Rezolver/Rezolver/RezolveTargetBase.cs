using System;
using System.Linq.Expressions;

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
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="targetType"></param>
		/// <returns></returns>
		protected abstract Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null);

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
		/// <param name="scope"></param>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public virtual Expression CreateExpression(IRezolverScope scope, Type targetType = null)
		{
			if (targetType != null && !SupportsType(targetType))
				throw new ArgumentException(string.Format(Resources.Exceptions.TargetDoesntSupportType_Format, targetType),
					"targetType");
			return CreateExpressionBase(scope, targetType);
		}

		public abstract Type DeclaredType
		{
			get;
		}
	}
}
