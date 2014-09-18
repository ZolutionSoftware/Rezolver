using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class GenericConstructorTarget : RezolveTargetBase
	{
		private static Type[] EmptyTypes = new Type[0];

		private Type _genericType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="genericType">The type of the object that is to be built (open generic of course)</param>
		public GenericConstructorTarget(Type genericType)
		{
			_genericType = genericType;
		}

		public override bool SupportsType(Type type)
		{
			if (!base.SupportsType(type))
			{
				//scenario - requested type is a closed generic built from this target's open generic
				if (!type.IsGenericType)
					return false;

				var genericType = type.GetGenericTypeDefinition();
				if (genericType == DeclaredType)
					return true;
				if (DeclaredType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType))
					return true;
				return false;
			}
			return true;
		}

		protected override System.Linq.Expressions.Expression CreateExpressionBase(CompileContext context)
		{
			//always create a constructor target from new
			//basically this class simply acts as a factory for other constructor targets.

			var expectedType = context.TargetType;
			if (expectedType == null)
				throw new ArgumentException("GenericConstructorTarget requires a concrete to be passed in the CompileContext - by definition it cannot simply create a default instance of the target type.", "context");
			if (!expectedType.IsGenericType)
				throw new ArgumentException("The compile context requested an instance of a non-generic type to be built.", "context");

			var genericType = expectedType.GetGenericTypeDefinition();
			Type[] suppliedTypeArguments = EmptyTypes;
			Type[] finalTypeArguments = EmptyTypes;
			if (genericType == DeclaredType)
			{
				//will need, at some point to map the type arguments of this target to the type arguments supplied,
				//but, for the moment, no.
				finalTypeArguments = expectedType.GetGenericArguments();
			}
			else
			{
				if (expectedType.IsInterface && expectedType.IsGenericType)
				{
					//find the required interface among the declared type's interfaces.
					var mappedInterface = DeclaredType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);
					if (mappedInterface != null)
					{
						var interfaceTypeParams = mappedInterface.GetGenericArguments();
						var typeParamPositions = DeclaredType
							.GetGenericArguments()
							.Select(t =>
								new
								{
									DeclaredTypeParamPosition = t.GenericParameterPosition,
									Type = t,
									//the projection here allows us to get the index of the base interface's generic type parameter
									//It is required because using the GenericParameterPosition property simply returns the index of the 
									//type in our declared type, as the type is passed down into the interfaces from the open generic
									//but closes them over those very types.  Thus, the <T> from an open generic class Foo<T> is passed down
  								//to IFoo<T> almost as if it were a proper type, and the <T> in IFoo<> is actually equal to the <T> from Foo<T>.
									MappedTo = interfaceTypeParams.Select((Type tt, int i) => 
										new { Type = tt, InterfaceTypeParameterPosition = i }).FirstOrDefault(tt => tt.Type == t)
								}).OrderBy(r => r.MappedTo != null ? r.MappedTo.InterfaceTypeParameterPosition : int.MinValue).ToArray();
						if (typeParamPositions.All(r => r.MappedTo != null))
						{
							suppliedTypeArguments = expectedType.GetGenericArguments();
							finalTypeArguments = new Type[typeParamPositions.Length];
							foreach (var typeParam in typeParamPositions)
							{
								finalTypeArguments[typeParam.DeclaredTypeParamPosition] = suppliedTypeArguments[typeParam.MappedTo.InterfaceTypeParameterPosition];
							}
						}
					}
				}

				if (finalTypeArguments.Length == 0 || finalTypeArguments.Any(t => t == null))
					throw new ArgumentException("Unable to complete generic target, not enough information from CompileContext", "context");
			}

			//make the generic type
			var typeToBuild = DeclaredType.MakeGenericType(finalTypeArguments);
			//construct the constructortarget
			var target = ConstructorTarget.Auto(typeToBuild);

			return target.CreateExpression(context);
		}

		//private Type MapInterfaceTypeParameter(Type theParameter, Type theBaseType, int targetParameterPosition)
		//{
		//	var initialMatch = theBaseType.GetGenericArguments().Select((Type tt, int i) => 
		//								new { Type = tt, InterfaceTypeParameterPosition = i }).FirstOrDefault(tt => tt.Type == theParameter)	;

		public override System.Type DeclaredType
		{
			get { return _genericType; }
		}


		//in order for this to work, we're going to need a dummy type that we can use, because
		//you can't pass open generics as type parameters.
		public static GenericConstructorTarget Auto<TGeneric>()
		{
			throw new NotImplementedException();
		}

		public static IRezolveTarget Auto(Type type)
		{
			//I might relax this constraint later - since we could implement partially open generics.
			if (!type.IsGenericTypeDefinition)
				throw new ArgumentException("The passed type must be an open generic type");
			return new GenericConstructorTarget(type);
		}
	}
}
