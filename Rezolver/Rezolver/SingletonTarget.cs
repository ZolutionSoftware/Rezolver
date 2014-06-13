using System;
using System.Threading;

namespace Rezolver
{
	/// <summary>
	/// Decorates any IRezolveTarget instance inside a lazily-initialised instance which will only ever
	/// create the target object once.
	/// 
	/// TODO: I don't actually think this will work properly because the scope is not responsible for actually
	/// creating the instances - a Rezolver will be.  This target is more an instruction on how to build a given instance.
	/// </summary>
	public class SingletonTarget : IRezolveTarget
	{
		private readonly Lazy<object> _lazyTarget;
		private IRezolveTarget _innerTarget;

		public SingletonTarget(IRezolveTarget innerTarget)
		{
			

			innerTarget.MustNotBeNull("innerTarget");
			// TODO: Complete member initialization
			this._innerTarget = innerTarget;
			//if the passed target is another Singleton target, then import its lazy target reference
			//to this one rather than wrapping it in another one.
			SingletonTarget singletonTarget = innerTarget as SingletonTarget;

			if (singletonTarget != null)
				_lazyTarget = singletonTarget._lazyTarget;
			else
				_lazyTarget = new Lazy<object>(() => _innerTarget.GetObject());
		}

		public bool SupportsType(Type type)
		{
			return _innerTarget.SupportsType(type);
		}

		public object GetObject()
		{
			return _lazyTarget.Value;
		}

		public Type DeclaredType
		{
			get { return _innerTarget.DeclaredType; }
		}
	}
}
