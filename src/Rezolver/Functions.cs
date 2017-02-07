// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// This type is only used when using expressions as targets (via the <see cref="ExpressionTarget"/> type) - it's
	/// functions server no actual purpose other than to act as hooks to create specific <see cref="ITarget"/> objects
	/// in place of static code.
	/// 
	/// For example, the <see cref="Resolve{T}"/> function is used to trigger the creation of a <see cref="ResolvedTarget"/>
	/// in its place - thus allowing expressions to leverage the full power of the Rezolver API all through a simple method call.
	/// </summary>
	public static class Functions
	{
		/// <summary>
		/// Translated to a <see cref="ResolvedTarget" /> with <typeparamref name="T"/> being the 
		/// type that will be resolved in this function's place.
		/// </summary>
		/// <typeparam name="T">The type to be resolved.</typeparam>
		/// <exception cref="NotImplementedException">Always.  The method is not intended to be used outside
		/// of an expression.</exception>
		public static T Resolve<T>()
		{
			throw new NotImplementedException(ExceptionResources.NotRuntimeMethod);
		}
	}
}
