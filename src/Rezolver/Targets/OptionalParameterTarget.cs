// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Reflection;

namespace Rezolver.Targets
{
	/// <summary>
	/// Used specifically when binding arguments to method parameters when a parameter is optional and its
	/// default value is to be used when binding to it.
	/// </summary>
	public class OptionalParameterTarget : TargetBase
	{
		/// <summary>
		/// Always returns the <see cref="ParameterInfo.ParameterType"/> of the <see cref="MethodParameter"/>
		/// </summary>
		public override Type DeclaredType { get { return MethodParameter.ParameterType; } }

		/// <summary>
		/// Possibly going to be removed or at least changed.
		/// </summary>
		protected override bool SuppressScopeTracking
		{
			get
			{
				//there's never any need to perform scope tracking on default values.
				return true;
			}
		}

		/// <summary>
		/// Always returns true, since using a default argument of a parameter is always considered
		/// to be a last-resort.
		/// </summary>
		public override bool UseFallback { get { return true; } }

		/// <summary>
		/// The parameter to which this target is bound.
		/// </summary>
		public ParameterInfo MethodParameter { get; }

		/// <summary>
		/// Constructs a new instance of the <see cref="OptionalParameterTarget"/> class.
		/// </summary>
		/// <param name="methodParameter">Required - parameter to which this target will be bound.
		/// 
		/// Its <see cref="ParameterInfo.IsOptional"/> property must be <c>true</c> otherwise an <see cref="ArgumentException"/> is thrown.</param>
		public OptionalParameterTarget(ParameterInfo methodParameter)
		{
			methodParameter.MustNotBeNull(nameof(methodParameter));
			methodParameter.MustNot(pi => !pi.IsOptional, "The methodParameter must represent an optional parameter", nameof(methodParameter));
			MethodParameter = methodParameter;
		}
	}
}
