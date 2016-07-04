// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Holds a reference to the default compiler for this application.
	/// 
	/// By default this is set to the <see cref="TargetDelegateCompiler"/> - which is a general purpose
	/// compiler that should work on any platform which supports linq expression trees.
	/// </summary>
	public static class TargetCompiler
	{
		private static ITargetCompiler _default = new TargetDelegateCompiler();

		/// <summary>
		/// The default compiler to be used by rezolvers when they are not explicitly provided one.
		/// </summary>
		public static ITargetCompiler Default
		{
			get { return _default; }
			set
			{
				value.MustNotBeNull("value");
				_default = value;
			}
		}
	}
}