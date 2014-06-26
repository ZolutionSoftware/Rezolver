using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rezolver.Resources;

namespace Rezolver
{
	public static class RezolverScopeExtensions
	{
		/// <summary>
		/// This extension mmethod is used only for helping to build IRezolveTarget instances from expressions.
		/// 
		/// Executing this method at runtime will always throw a <see cref="NotImplementedException"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scope"></param>
		/// <returns></returns>
		public static T Rezolve<T>(this IRezolverScope scope)
		{
			throw new NotImplementedException(Exceptions.NotRuntimeMethod);
		}

		/// <summary>
		/// This extension mmethod is used only for helping to build IRezolveTarget instances from expressions.
		/// 
		/// Executing this method at runtime will always throw a <see cref="NotImplementedException"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scope"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T Rezolve<T>(this IRezolverScope scope, string name)
		{
			throw new NotImplementedException(Exceptions.NotRuntimeMethod);
		}
	}
}
