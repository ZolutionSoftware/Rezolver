using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestTarget : ITarget
	{
		private Type _declaredType;
		public Type DeclaredType
		{
			get
			{
				if(_declaredType == null)
					throw new NotImplementedException();
				return _declaredType;
			}
		}

		private bool? _useFallBack;
		public bool UseFallback
		{
			get
			{
				if(_useFallBack == null)
					throw new NotImplementedException();
				return _useFallBack.Value;
			}
		}

		

		private ScopeActivationBehaviour? _scopeBehaviour;

		public ScopeActivationBehaviour ScopeBehaviour
		{
			get
			{
				if (_scopeBehaviour == null)
					throw new NotImplementedException();
				return _scopeBehaviour.Value;
			}
		}

		private bool? _supportsType;
		public bool SupportsType(Type type)
		{
			if(_supportsType == null)
				throw new NotImplementedException();
			return _supportsType.Value;
		}

		public IContainerScope SelectScope(ResolveContext context)
		{
			return context.NewScope;
		}

		public TestTarget(Type declaredType = null, bool? useFallBack = null, bool? supportsType = null, ScopeActivationBehaviour? scopeBehaviour = null)
		{
			_declaredType = declaredType;
			_useFallBack = useFallBack;
			_supportsType = supportsType;
		}
	}
}
