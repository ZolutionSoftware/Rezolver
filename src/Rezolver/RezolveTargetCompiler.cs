using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Holds a reference to the default compiler for this application.
	/// 
	/// By default this is set to the RezolveTargetDelegateCompiler - which is a general purpose
	/// compiler that should work on any platform.
	/// </summary>
	public static class RezolveTargetCompiler
	{
		private static IRezolveTargetCompiler _default = new RezolveTargetDelegateCompiler();

		/// <summary>
		/// The default compiler to be used by rezolvers when they are not explicitly provided one.
		/// </summary>
		public static IRezolveTargetCompiler Default
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