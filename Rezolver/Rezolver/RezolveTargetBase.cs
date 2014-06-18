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
				throw new ArgumentException(String.Format(Exceptions.TargetDoesntSupportType_Format, targetType),
					"targetType");
			return CreateExpressionBase(scope, targetType);
		}

		public abstract Type DeclaredType
		{
			get;
		}

		/// <summary>
		/// Automatically derives parameter bindings for the passed method
		/// </summary>
		/// <param name="method"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		protected static ParameterBinding[] DeriveParameterBindings(MethodBase method)
		{
			method.MustNotBeNull("method");
			var parameters = method.GetParameters();
			ParameterBinding[] toReturn = new ParameterBinding[parameters.Length];
			int current = 0;
			ParameterBinding binding = null;
			foreach (var parameter in parameters)
			{
				if (parameter.IsOptional)
				{
					if((parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
						binding = new ParameterBinding(parameter, parameter.DefaultValue.AsObjectTarget(parameter.ParameterType));
					else
						binding = new ParameterBinding(parameter, new DefaultTarget(parameter.ParameterType));
				}
				toReturn[current++] = binding;
			}
			return toReturn;
		}
	}
}
