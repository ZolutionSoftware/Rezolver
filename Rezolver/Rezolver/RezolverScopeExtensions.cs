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
		private static readonly MethodInfo[] RezolveMethods =
		{
			MethodCallExtractor.ExtractCalledMethod((IRezolverScope scope) => Rezolve<int>(scope)).GetGenericMethodDefinition()
			, MethodCallExtractor.ExtractCalledMethod((IRezolverScope scope) => Rezolve<int>(scope, null)).GetGenericMethodDefinition()
		};

		public static T Rezolve<T>(this IRezolverScope scope)
		{
			throw new NotImplementedException(Exceptions.NotRuntimeMethod);
		}

		public static T Rezolve<T>(this IRezolverScope scope, string name = null)
		{
			throw new NotImplementedException(Exceptions.NotRuntimeMethod);
		}

		public static RezolveCallExpressionInfo ExtractRezolveCall(Expression e)
		{
			MethodCallExpression methodExpr = e as MethodCallExpression;
			if (methodExpr != null)
			{
				if (methodExpr.Method.IsGenericMethod)
				{
					var match = RezolveMethods.SingleOrDefault(m => m.Equals(methodExpr.Method.GetGenericMethodDefinition()));
					if (match != null)
					{
						//by the number of the parameters we know if a string is being passed
						var nameParameter = match.GetParameters().FirstOrDefault(pi => pi.ParameterType == typeof (string));

						if (nameParameter != null)
						{
							//extract the expression being passed as the argument to the name parameter.
						}

					}
				}
			}

			return null;
		}

		public class RezolveCallExpressionInfo
		{
			public Type Type { get; private set; }
			public Expression Name { get; private set; }

			internal RezolveCallExpressionInfo(Type type, Expression name)
			{
				Type = type;
				Name = name;
			}
		}
	}
}
