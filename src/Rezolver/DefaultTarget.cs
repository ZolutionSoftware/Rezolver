using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// A target that simply creates a default instance of a given type.  I.e. the same
	/// as doing default(type) in C#.
	/// </summary>
	public class DefaultTarget : TargetBase
	{
		private readonly Type _declaredType;

		public DefaultTarget(Type type)
		{
			type.MustNotBeNull("type");
			_declaredType = type;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			return Expression.Default(DeclaredType);
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
	}
}