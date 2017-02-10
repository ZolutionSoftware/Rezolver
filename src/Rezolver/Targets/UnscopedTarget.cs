using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
	/// <summary>
	/// Wraps another target to force scoping to be ignored for the object that it produces, regardless
	/// of whether that object is IDisposable
	/// </summary>
	public class UnscopedTarget : TargetBase
	{
		public override ScopeActivationBehaviour ScopeBehaviour
		{
			get
			{
				return ScopeActivationBehaviour.None;
			}
		}
		public override Type DeclaredType
		{
			get
			{
				return Inner.DeclaredType;
			}
		}

		public override bool UseFallback
		{
			get
			{
				return Inner.UseFallback;
			}
		}

		public ITarget Inner { get; }

		public UnscopedTarget(ITarget inner)
		{
			inner.MustNotBeNull(nameof(inner));
			Inner = inner;
		}

		public override bool SupportsType(Type type)
		{
			return Inner.SupportsType(type);
		}
	}
}
