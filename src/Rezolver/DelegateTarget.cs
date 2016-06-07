using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// Implements IRezolveTarget using factory function delegates.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DelegateTarget<T> : TargetBase
	{
		private readonly Type _declaredType;
		private readonly Func<T> _factory;

		public DelegateTarget(Func<T> factory, Type declaredType = null)
		{
			factory.MustNotBeNull("factory");
			_factory = factory;
			if (declaredType != null)
			{
				if (!TypeHelpers.AreCompatible(typeof(T), declaredType) && !TypeHelpers.AreCompatible(declaredType, typeof(T)))
					throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, typeof(T)));
			}
			_declaredType = declaredType ?? typeof(T);
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			//TODO: This doesn't forward the rezolve context, and it should, which would mean changing how
			//this code is compiled.

			//have to pull the _factory member local otherwise it's seen as a member access, which
			//explodes in the full-blown Dynamic Assembly scenario, but it's private.
			//var factoryLocal = _factory;
			return Expression.Invoke(Expression.Constant(_factory));
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
	}

	public static class DelegateTargetExtensions
	{
		public static DelegateTarget<T> AsDelegateTarget<T>(this Func<T> factory, Type declaredType = null)
		{
			return new DelegateTarget<T>(factory, declaredType);
		}
	}
}
