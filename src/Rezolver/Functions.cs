// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// This type is only used when building targets from expressions, and provides a natural way to
	/// have specific <see cref="ITarget"/> objects created from natural code, when using the default
	/// <see cref="TargetAdapter"/> to translate expressions into targets.
	/// </summary>
	public static class Functions
	{
		/// <summary>
		/// Translated to a <see cref="RezolvedTarget" /> with <typeparamref name="T"/> being the 
		/// type taht will be resolved.
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
