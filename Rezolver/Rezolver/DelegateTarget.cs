using System;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Implements IRezolveTarget using factory function delegates.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DelegateTarget<T> : RezolveTargetBase
	{
		private readonly Type _declaredType;
		private readonly Func<T> _factory;	
		public DelegateTarget(Func<T> factory, Type declaredType = null)
		{
			factory.MustNotBeNull("factory");
			_factory = factory;
			if (declaredType != null)
			{
				if (!TypeHelpers.AreCompatible(typeof(T), declaredType))
					throw new ArgumentException(string.Format(Resources.Exceptions.DeclaredTypeIsNotCompatible_Format, declaredType, typeof(T)));
			}
			_declaredType = declaredType ?? typeof(T);
		}

		public override Expression CreateExpression(IRezolverScope scope, Type targetType = null)
		{
			throw new NotImplementedException();

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
