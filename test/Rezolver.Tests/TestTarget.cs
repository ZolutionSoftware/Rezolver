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

		

		private ScopeBehaviour? _scopeBehaviour;

		public ScopeBehaviour ScopeBehaviour
		{
			get
			{
				if (_scopeBehaviour == null)
					throw new NotImplementedException();
				return _scopeBehaviour.Value;
			}
		}

        public ScopePreference ScopePreference => ScopePreference.Current;

        private bool? _supportsType;
		public bool SupportsType(Type type)
		{
			if(_supportsType == null)
				throw new NotImplementedException();
			return _supportsType.Value;
		}

		

		public TestTarget(Type declaredType = null, bool? useFallBack = null, bool? supportsType = null, ScopeBehaviour? scopeBehaviour = null)
		{
			_declaredType = declaredType;
			_useFallBack = useFallBack;
			_supportsType = supportsType;
			_scopeBehaviour = scopeBehaviour;
		}
	}
}
