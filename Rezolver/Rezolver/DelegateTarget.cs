using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
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

		public override object GetObject()
		{
			return _factory();
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
