using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Used specifically when binding arguments to method parameters when a parameter is optional and its
	/// default value is to be used when binding to it.
	/// </summary>
	public class OptionalParameterTarget : TargetBase
	{
		public override Type DeclaredType
		{
			get
			{
				return MethodParameter.ParameterType;
			}
		}

		protected override bool SuppressScopeTracking
		{
			get
			{
				//there's never any need to perform scope tracking on default values.
				return true;
			}
		}

		public ParameterInfo MethodParameter { get; }
		public OptionalParameterTarget(ParameterInfo methodParameter)
		{
			methodParameter.MustNotBeNull(nameof(methodParameter));
			methodParameter.MustNot(pi => !pi.IsOptional, "The methodParameter must represent an optional parameter", nameof(methodParameter));
			MethodParameter = methodParameter;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			return (MethodParameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault ?
				(Expression)Expression.Constant(MethodParameter.DefaultValue, MethodParameter.ParameterType) : Expression.Default(MethodParameter.ParameterType);
		}
	}
}
