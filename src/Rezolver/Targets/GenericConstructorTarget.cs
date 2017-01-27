// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
	/// <summary>
	/// Equivalent of <see cref="ConstructorTarget"/> but for open generic types.
	/// 
	/// So, this will handle the open generic MyType&lt;,&gt;, for example, whereas <see cref="ConstructorTarget"/>
	/// would handle the closed type MyType&lt;int, string&gt;.
	/// </summary>
	/// <seealso cref="TargetBase" />
	public class GenericConstructorTarget : TargetBase
	{
		/// <summary>
		/// Result returned from the <see cref="MapType(Type)"/> function
		/// </summary>
		public class GenericTypeMapping
		{
			/// <summary>
			/// Gets a string describing the reason why the type could not be mapped.  Can be used for exceptions, etc.
			/// </summary>
			/// <value>The error message.</value>
			public string ErrorMessage { get; }
			/// <summary>
			/// If <see cref="Success"/> = <c>true</c>, gets the final, closed, generic type implementation to be used for the
			/// requested type.
			/// </summary>
			/// <value>The type.</value>
			public Type Type { get; }
			/// <summary>
			/// Gets a value indicating whether the <see cref="DeclaredType"/> of the <see cref="GenericConstructorTarget"/>  
			/// was successfully mapped to the requested type.  If so, then an instance of <see cref="Type"/> will be compatible
			/// with the type that was requested.
			/// </summary>
			/// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
			public bool Success {  get { return Type != null; } }

			internal GenericTypeMapping(Type type)
			{
				Type = type;
			}

			internal GenericTypeMapping(string errorMessage)
			{
				ErrorMessage = errorMessage;
			}
		}
		private static Type[] EmptyTypes = new Type[0];

		/// <summary>
		/// Gets the generic type definition from which generic types are to be built and instances of which 
		/// will be constructed.
		/// </summary>
		public Type GenericType { get; private set; }

		/// <summary>
		/// Gets the member binding behaviour to be used when creating an instance.
		/// </summary>
		/// <value>The member binding behaviour.</value>
		public IMemberBindingBehaviour MemberBindingBehaviour { get; private set; }


		/// <summary>
		/// Implementation of the abstract base property.  Will return the unbound generic type passed to this object on construction.
		/// </summary>
		public override System.Type DeclaredType
		{
			get { return GenericType; }
		}


		/// <summary>
		/// Constructs a new instance of the <see cref="GenericConstructorTarget"/> for the given open generic type,
		/// which will utilise the optional <paramref name="memberBindingBehaviour"/> when it constructs its
		/// <see cref="ConstructorTarget"/> when <see cref="Bind(ICompileContext)"/> is called.
		/// </summary>
		/// <param name="genericType">The type of the object that is to be built (open generic of course)</param>
		/// <param name="memberBindingBehaviour">Optional.  The <see cref="IMemberBindingBehaviour"/> to be used for binding
		/// properties and/or fields on the <see cref="ConstructorTarget"/> that is generated.  If null, then no property 
		/// or fields will be bound on construction.</param>
		public GenericConstructorTarget(Type genericType, IMemberBindingBehaviour memberBindingBehaviour = null)
		{
			genericType.MustNotBeNull(nameof(genericType));
			if (!TypeHelpers.IsGenericTypeDefinition(genericType))
				throw new ArgumentException("The generic constructor target currently only supports fully open generics.  Use ConstructorTarget for closed generics.");
			if (TypeHelpers.IsAbstract(genericType) || TypeHelpers.IsInterface(genericType))
				throw new ArgumentException("The type must be a generic type definition of either a non-abstract class or value type.");
			GenericType = genericType;
			MemberBindingBehaviour = memberBindingBehaviour;
		}

		/// <summary>
		/// Override - introduces additional logic to cope with generic types not generally supported by the majority of other targets.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool SupportsType(Type type)
		{
			//example - if this is bound to Generic<> and you pass Generic<>, then although the
			//types are the same, 
			if (base.SupportsType(type))
				return true;
			return MapType(type).Success;
		}

		public GenericTypeMapping MapType(Type targetType)
		{
			//used both in SupportsType and in the Bind function - except the bind function
			//uses the error message as exception text
			if (TypeHelpers.IsGenericTypeDefinition(targetType))
				return new GenericTypeMapping($"The type { targetType } is an open generic and therefore can't be mapped to a closed version of { DeclaredType }");
			if (!TypeHelpers.IsGenericType(targetType))
				return new GenericTypeMapping($"The type { DeclaredType } cannot be mapped to non-generic type { targetType }");

			var genericType = targetType.GetGenericTypeDefinition();
			Type[] suppliedTypeArguments = EmptyTypes;
			Type[] finalTypeArguments = EmptyTypes;
			if (genericType == DeclaredType)
			{
				finalTypeArguments = TypeHelpers.GetGenericArguments(targetType);
			}
			else
			{
				finalTypeArguments = MapGenericParameters(targetType);

				if (finalTypeArguments.Length == 0 || finalTypeArguments.Any(t => t == null) || finalTypeArguments.Any(t => t.IsGenericParameter))
					return new GenericTypeMapping($"The requested type { targetType } doesn't contain enough generic arguments to calculate a closed generic type from { DeclaredType }");
			}

			//make the generic type
			return new GenericTypeMapping(DeclaredType.MakeGenericType(finalTypeArguments));
		}

		/// <summary>
		/// Obtains an <see cref="ITarget"/> (usually a <see cref="ConstructorTarget"/>) which will create 
		/// an instance of a generic type (whose definition is equal to <see cref="GenericType"/>) with 
		/// generic arguments set correctly according to the <see cref="ICompileContext.TargetType"/> of 
		/// the <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <remarks>The process of binding a requested type to the concrete type can be very complex, when
		/// inheritance chains and interface implementation maps are taken into account.
		/// 
		/// At the simplest end of the spectrum, if <see cref="GenericType"/> is <c>MyGeneric&lt;&gt;</c> and
		/// the <paramref name="context"/>'s <see cref="ICompileContext.TargetType"/> is <c>MyGeneric&lt;int&gt;</c>,
		/// then this function merely has to insert the <c>int</c> type as the generic parameter to the <c>MyGeneric&lt;&gt;</c>
		/// type definition, bake a new type and create an auto-bound <see cref="ConstructorTarget"/>.
		/// 
		/// Consider what happens, however, when the inheritance chain is more complex:
		/// 
		/// <code>
		/// interface IMyInterfaceCore&lt;T, U&gt; { }
		/// 
		/// class MyBaseClass&lt;T, U&gt; : IMyInterfaceCore&lt;U, T&gt; { }
		/// 
		/// class MyDerivedClass&lt;T, U&gt; : MyBaseClass&lt;U, T&gt; { }
		/// </code>
		/// 
		/// A <see cref="GenericConstructorTarget"/> bound to the generic type definition <c>MyDerivedClass&lt;,&gt;</c> can
		/// create instances not only of any generic type based on that definition, but also any generic type based on the definitions 
		/// of either it's immediate base, or that base's interface.  In order to do so, however, the parameters must be mapped 
		/// between the generic type definitions so that if an instance of <c>MyBaseClass&lt;string, int&gt;</c> is requested, 
		/// then an instance of <c>MyDerivedClass&lt;int, string&gt;</c> (note the arguments are reversed) is actually created.
		/// 
		/// Similarly, if an instance of <c>IMyInterface&lt;string, int&gt;</c> is requested, we actually need to create an 
		/// instance of <c>MyDerivedClass&lt;string, int&gt;</c> - because the generic arguments are reversed first through 
		/// the base class inheritance, and then again by the base class' implementation of the interface.
		/// 
		/// Note that a <see cref="GenericConstructorTarget"/> can only bind to the context's target type if there is enough information
		/// in order to deduce the generic type arguments for <see cref="GenericType"/>.  This means, in general, that the requested
		/// type will almost always need to be a generic type with at least as many type arguments as the <see cref="GenericType"/>.
		/// </remarks>
		public ITarget Bind(ICompileContext context)
		{
			context.MustNotBeNull(nameof(context));

			//always create a constructor target from new
			//basically this class simply acts as a factory for other constructor targets.

			var expectedType = context.TargetType;
			if (expectedType == null)
				throw new ArgumentException("GenericConstructorTarget requires a concrete type to be passed in the CompileContext - by definition it cannot simply create a default instance of the target type.", nameof(context));
			var mapTypeResult = MapType(context.TargetType);
			if (!mapTypeResult.Success)
				throw new ArgumentException(mapTypeResult.ErrorMessage, nameof(context));
			//construct the constructortarget
			return ConstructorTarget.Auto(mapTypeResult.Type, MemberBindingBehaviour);
		}

		private Type[] MapGenericParameters(Type requestedType)
		{
			var requestedTypeGenericDefinition = requestedType.GetGenericTypeDefinition();
			Type[] finalTypeArguments = TypeHelpers.GetGenericArguments(DeclaredType);
			//check whether it's a base or an interface
			var mappedBase = TypeHelpers.IsInterface(requestedTypeGenericDefinition) ?
				TypeHelpers.GetInterfaces(DeclaredType).FirstOrDefault(t => TypeHelpers.IsGenericType(t) && t.GetGenericTypeDefinition() == requestedTypeGenericDefinition)
				: DeclaredType.GetAllBases().SingleOrDefault(b => TypeHelpers.IsGenericType(b) && b.GetGenericTypeDefinition() == requestedTypeGenericDefinition);
			if (mappedBase != null)
			{
				var baseTypeParams = TypeHelpers.GetGenericArguments(mappedBase);
				var typeParamPositions = TypeHelpers.GetGenericArguments(DeclaredType)
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

				var suppliedTypeArguments = TypeHelpers.GetGenericArguments(requestedType);
				Type suppliedArg = null;
				foreach (var typeParam in typeParamPositions.Where(p => p.MappedTo != null))
				{
					suppliedArg = suppliedTypeArguments[typeParam.MappedTo[0]];
					foreach (var index in typeParam.MappedTo.Skip(1))
					{
						suppliedArg = TypeHelpers.GetGenericArguments(suppliedArg)[index];
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
		/// <param name="baseTypeParameter"></param>
		/// <param name="targetTypeParameter"></param>
		/// <returns></returns>
		private int[] DeepSearchTypeParameterMapping(Stack<int> previousTypeParameterPositions, Type baseTypeParameter, Type targetTypeParameter)
		{
			if (baseTypeParameter == targetTypeParameter)
				return previousTypeParameterPositions.ToArray();
			if (previousTypeParameterPositions == null)
				previousTypeParameterPositions = new Stack<int>();
			if (TypeHelpers.IsGenericType(baseTypeParameter))
			{
				var args = TypeHelpers.GetGenericArguments(baseTypeParameter);
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

		/// <summary>
		/// Equivalent of <see cref="ConstructorTarget.Auto(Type, IMemberBindingBehaviour)"/> but for open generic types.
		/// 
		/// Note - there is no generic version because that could only be invoked by reflection.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="memberBindingBehaviour">Optional behaviour controlling which properties and fields, if any, will receive injected values.</param>
		/// <exception cref="ArgumentException">
		/// This is raised from the <see cref="GenericConstructorTarget.GenericConstructorTarget(Type, IMemberBindingBehaviour)"/> constructor
		/// when the passed type is either not an open generic type
		/// or is an abstract class or interface.
		/// </exception>
		public static ITarget Auto(Type type, IMemberBindingBehaviour memberBindingBehaviour = null)
		{ 
			return new GenericConstructorTarget(type);
		}
	}
}
