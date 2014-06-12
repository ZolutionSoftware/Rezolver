using System;

namespace Rezolver
{
	public abstract class RezolveTargetBase : IRezolveTarget
	{

		public bool SupportsType(Type type)
		{
			type.MustNotBeNull("type");
			return TypeHelpers.AreCompatible(DeclaredType, type);
		}

		public abstract object GetObject();

		public abstract Type DeclaredType
		{
			get;
		}
	}
}
