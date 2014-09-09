using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	public class PropertyOrFieldBinding
	{
		public static readonly PropertyOrFieldBinding[] None = new PropertyOrFieldBinding[0];

		public MemberInfo Member { get; private set; }

		public IRezolveTarget Target { get; private set; }

		public Type MemberType
		{
			get
			{
				PropertyInfo p = Member as PropertyInfo;
				if (p != null)
					return p.PropertyType;
				else
					return ((FieldInfo)Member).FieldType;
			}
		}

		public PropertyOrFieldBinding(MemberInfo member, IRezolveTarget target)
		{
			Member = member;
			Target = target;
		}

		/// <summary>
		/// Attempts to bind all publicly writable instance properties of the given type
		/// with RezolveTargets.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PropertyOrFieldBinding[] DeriveAutoPropertyBinding(Type type, bool includeFields = false)
		{
			//note - the canwrite and GetSetMethod check in the predicate 
			var memberBindings = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.CanWrite && p.GetSetMethod() != null)
				.Select(p => new PropertyOrFieldBinding(p, new RezolvedTarget(p.PropertyType)));
			if(includeFields)
			{
				memberBindings = memberBindings.Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public)
					.Select(m => new PropertyOrFieldBinding(m, new RezolvedTarget(m.FieldType))));
			}

			return memberBindings.ToArray();
		}

		public MemberBinding CreateMemberBinding(CompileContext context)
		{
			return Expression.Bind(Member, Target.CreateExpression(new CompileContext(context, MemberType, true)));
		}
	}
}
