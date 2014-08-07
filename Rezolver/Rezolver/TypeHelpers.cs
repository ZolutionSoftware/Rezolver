using System;

namespace Rezolver
{
	internal static class TypeHelpers
	{
		internal static bool CanBeNull(Type type)
		{
			return !type.IsValueType || IsNullableType(type);
		}

		internal static bool AreCompatible(Type from, Type to)
		{
			from.MustNotBeNull("from");
			to.MustNotBeNull("to");
			//this is checking whether it's possible to do a runtime cast between the
			//two types.  Now, this is more than just reference casting - as the runtime
			//will support 'int? a = null' for example, or 'int? a = 1' for example.

			if (to.IsAssignableFrom(from))
				return true;

			Type nulledType = null;
			return IsNullableType(@from, out nulledType) && to.IsAssignableFrom(nulledType);

			//if (to.IsValueType)
			//{
			//	if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			//		return true;
			//	return false;
			//}
			//return true;
		}

		internal static bool IsNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		internal static bool IsNullableType(Type type, out Type nulledType)
		{
			nulledType = null;

			if(!type.IsGenericType)
				return false;
			var genType = type.GetGenericTypeDefinition();
			if (genType != typeof(Nullable<>))
				return false;

			nulledType = type.GetGenericArguments()[0];
			return true;
		}
	}
}
