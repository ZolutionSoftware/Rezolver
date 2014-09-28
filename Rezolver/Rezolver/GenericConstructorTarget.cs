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
		private IPropertyBindingBehaviour _propertyBindingBehaviour;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="genericType">The type of the object that is to be built (open generic of course)</param>
		public GenericConstructorTarget(Type genericType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			if (!genericType.IsGenericTypeDefinition)
				throw new ArgumentException("The generic constructor target currently only supports fully open generics.  Partially open generics are not yet supported, and for fully closed generics, use ConstructorTarget");
			_genericType = genericType;
			_propertyBindingBehaviour = propertyBindingBehaviour;
		}

		public override bool SupportsType(Type type)
		{
			if (base.SupportsType(type))
				return true;

			//scenario - requested type is a closed generic built from this target's open generic
			if (!type.IsGenericType)
				return false;

			var genericType = type.GetGenericTypeDefinition();
			if (genericType == DeclaredType)
				return true;

			if (!genericType.IsInterface)
			{
				var bases = TypeHelpers.GetAllBases(DeclaredType);
				var matchedBase = bases.FirstOrDefault(b => b.IsGenericType && b.GetGenericTypeDefinition() == genericType);
				if (matchedBase != null)
					return true;
			}
			//TODO: tighten this up to handle the proposed partially open type
			else if (DeclaredType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType))
				return true;

			return false;
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
				if (expectedType.IsGenericType)
					finalTypeArguments = MapGenericParameters(expectedType, DeclaredType);

				if (finalTypeArguments.Length == 0 || finalTypeArguments.Any(t => t == null) || finalTypeArguments.Any(t => t.IsGenericParameter))
					throw new ArgumentException("Unable to complete generic target, not enough information from CompileContext", "context");
			}

			//make the generic type
			var typeToBuild = DeclaredType.MakeGenericType(finalTypeArguments);
			//construct the constructortarget
			var target = ConstructorTarget.Auto(typeToBuild, _propertyBindingBehaviour);

			return target.CreateExpression(context);
		}

		private Type[] MapGenericParameters(Type requestedType, Type targetType)
		{
			var requestedTypeGenericDefinition = requestedType.GetGenericTypeDefinition();
			Type[] finalTypeArguments = targetType.GetGenericArguments();
			//check whether it's a base or an interface
			var mappedBase = requestedTypeGenericDefinition.IsInterface ?
				targetType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == requestedTypeGenericDefinition)
				: TypeHelpers.GetAllBases(targetType).SingleOrDefault(b => b.IsGenericType && b.GetGenericTypeDefinition() == requestedTypeGenericDefinition);
			if (mappedBase != null)
			{
				var baseTypeParams = mappedBase.GetGenericArguments();
				var typeParamPositions = targetType
					.GetGenericArguments()
					.Select(t =>
					{
						var mapping = DeepSearchTypeParameterMapping(null, mappedBase, t);

						//if the mapping is not found, but one or more of the interface type parameters are generic, then 
						//it's possible that one of those has been passed the type parameter.
						//the problem with that, fromm our point of view, however, is how then 

						return new
						{
							DeclaredTypeParamPosition = t.GenericParameterPosition,
							Type = t,
							//the projection here allows us to get the index of the base interface's generic type parameter
							//It is required because using the GenericParameterPosition property simply returns the index of the 
							//type in our declared type, as the type is passed down into the interfaces from the open generic
							//but closes them over those very types.  Thus, the <T> from an open generic class Foo<T> is passed down
							//to IFoo<T> almost as if it were a proper type, and the <T> in IFoo<> is actually equal to the <T> from Foo<T>.
							MappedTo = mapping
						};
					}).OrderBy(r => r.MappedTo != null ? r.MappedTo[0] : int.MinValue).ToArray();

				var suppliedTypeArguments = requestedType.GetGenericArguments();
				Type suppliedArg = null;
				foreach (var typeParam in typeParamPositions.Where(p => p.MappedTo != null))
				{
					suppliedArg = suppliedTypeArguments[typeParam.MappedTo[0]];
					foreach (var index in typeParam.MappedTo.Skip(1))
					{
						suppliedArg = suppliedArg.GetGenericArguments()[index];
					}
					finalTypeArguments[typeParam.DeclaredTypeParamPosition] = suppliedArg;
				}
			}
			return finalTypeArguments;
		}

		/// <summary>
		/// returns a series of type parameter indexes from the baseType parameter which can be used to derive
		/// the concrete type parameter to be used in a target type, given a fully-closed generic type as the model
		/// </summary>
		/// <param name="previousTypeParameterPositions"></param>
		/// <param name="candidateTypeParameter"></param>
		/// <param name="targetTypeParameter"></param>
		/// <returns></returns>
		private int[] DeepSearchTypeParameterMapping(Stack<int> previousTypeParameterPositions, Type baseTypeParameter, Type targetTypeParameter)
		{
			if (baseTypeParameter == targetTypeParameter)
				return previousTypeParameterPositions.ToArray();
			if (previousTypeParameterPositions == null)
				previousTypeParameterPositions = new Stack<int>();
			if (baseTypeParameter.IsGenericType)
			{
				var args = baseTypeParameter.GetGenericArguments();
				int[] result = null;
				for (int f = 0; f < args.Length; f++)
				{
					previousTypeParameterPositions.Push(f);
					result = DeepSearchTypeParameterMapping(previousTypeParameterPositions, args[f], targetTypeParameter);
					previousTypeParameterPositions.Pop();
					if (result != null)
						return result;
				}
			}
			return null;
		}

		public override System.Type DeclaredType
		{
			get { return _genericType; }
		}


		//in order for this to be callable in all cases, we're going to need a dummy type that we can use, because
		//you can't pass open generics as type parameters.  That dummy type 
		public static GenericConstructorTarget Auto<TGeneric>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			if (!typeof(TGeneric).IsGenericTypeDefinition)
				throw new InvalidOperationException("The passed type must be an open generic type");
			return new GenericConstructorTarget(typeof(TGeneric), propertyBindingBehaviour);
		}

		public static IRezolveTarget Auto(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			//I might relax this constraint later - since we could implement partially open generics.
			if (!type.IsGenericTypeDefinition)
				throw new ArgumentException("The passed type must be an open generic type");
			return new GenericConstructorTarget(type);
		}
	}
}
