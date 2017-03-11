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
		/// base interface only for <see cref="ITypeArgGenericMismatch{TFrom, TFromParent, TTo, TToParent}"/>
		/// just to simplify type testing
		/// </summary>
		private interface ITypeArgGenericMismatch { }
		/// <summary>
		/// Used purely to carry diagnostic information when type arguments aren't mapped successfully
		/// </summary>
		/// <typeparam name="TFrom">The type argument mapped from a target's <see cref="DeclaredType"/> corresponding to the argument that can't be mapped</typeparam>
		/// <typeparam name="TFromParent">The generic type which declares the type parameter <typeparamref name="TFrom"/></typeparam>
		/// <typeparam name="TTo">The type argument in the type for which a match was sought</typeparam>
		/// <typeparam name="TToParent">The generic type which declared the type parameter <typeparamref name="TTo"/></typeparam>
		private interface ITypeArgGenericMismatch<TFrom, TFromParent, TTo, TToParent> : ITypeArgGenericMismatch
		{

		}

		/// <summary>
		/// Another diagnostic-only interface.  Used for TFromParent and TToParent type arguments in the 
		/// <see cref="ITypeArgGenericMismatch{TFrom, TFromParent, TTo, TToParent}"/> type when a type parameter
		/// wasn't generic.
		/// </summary>
		private interface INotGeneric { }

		/// <summary>
		/// Result returned from the <see cref="MapType(Type)"/> function.  Represents various levels of success - 
		/// from a completely incompatible mapping (<see cref="Success"/> = <c>false</c>), or a successful mapping from
		/// an open generic type to a closed generic type which can then be constructed (<see cref="Success"/> = <c>true</c>
		/// and <see cref="IsFullyBound"/> = <c>true</c>) or, a successful mapping from an open generic type to another open 
		/// generic type (<see cref="Success"/> = <c>true</c> but <see cref="IsFullyBound"/> = <c>false</c>).
		/// 
		/// This mapping is then used by both the <see cref="SupportsType(Type)"/> and <see cref="Bind(ICompileContext)"/>
		/// functions.  Only fully bound mappings are supported by <see cref="Bind(ICompileContext)"/>, whereas 
		/// <see cref="SupportsType(Type)"/> will return <c>true</c> so long as the <see cref="Success"/> is true.
		/// 
		/// The caller, therefore, must ensure it is aware of the difference between open and closed generics.
		/// </summary>
		public class GenericTypeMapping
		{
			/// <summary>
			/// Gets a string describing the reason why the type could not be mapped.  Can be used for exceptions, etc.
			/// 
			/// Note that this can be set even if <see cref="Success"/> is <c>true</c> - because mappings exist between
			/// open generic types so that a target's <see cref="SupportsType(Type)"/> returns <c>true</c>, but the
			/// <see cref="Bind(ICompileContext)"/> function throws an exception for the same type, since you can't create
			/// an instance of an open generic.
			/// </summary>
			/// <value>The binding error message.</value>
			public string BindErrorMessage { get; }
			/// <summary>
			/// The type requested for mapping.  If this is an open generic, then the best result for this mapping will be
			/// that <see cref="Success"/> is <c>true</c> and <see cref="IsFullyBound"/> is <c>false</c>.
			/// </summary>
			public Type RequestedType { get; }
			/// <summary>
			/// If <see cref="Success"/> = <c>true</c>, gets the generic type to be used for the <see cref="RequestedType"/>.
			/// 
			/// Note that this could be either an open or closed generic - the <see cref="IsFullyBound"/> offers a quick means
			/// by which to determine this.  If <see cref="IsFullyBound"/> is <c>true</c>, then the mapping will succeed when 
			/// encountered by the <see cref="Bind(ICompileContext)"/> method.
			/// </summary>
			/// <value>The type.</value>
			public Type Type { get; }
			/// <summary>
			/// Gets a value indicating whether the <see cref="DeclaredType"/> of the <see cref="GenericConstructorTarget"/>  
			/// was successfully mapped to the requested type.  If so, and <see cref="IsFullyBound"/> is <c>true</c>, then an 
			/// instance of <see cref="Type"/> will be compatible with the type that was requested.
			/// 
			/// If <see cref="IsFullyBound"/> is <c>false</c>, then you can't create an instance of <see cref="Type"/> because it's
			/// an open generic - but you will be able to bind the same target to a closed generic of the same <see cref="Type"/>.
			/// </summary>
			/// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
			public bool Success {  get { return Type != null; } }

			/// <summary>
			/// If true, then the <see cref="Type"/> is a fully closed generic type that can be constructed (and therefore would
			/// be successfully bound by the <see cref="Bind(ICompileContext)"/> method, which uses the <see cref="MapType(Type)"/> 
			/// method).  If this is <c>false</c> but <see cref="Success"/> is <c>true</c>, then while the target is technically
			/// compatible with the requested type, you can't create an instance.  The target will, however, be able to mapped to
			/// a closed generic type based on the same <see cref="Type"/>.
			/// </summary>
			public bool IsFullyBound { get { return Success ? Type.IsConstructedGenericType : false; } }

			internal GenericTypeMapping(Type requestedType, Type type, string bindErrorMessage = null)
			{
				RequestedType = requestedType;
				Type = type;
				BindErrorMessage = bindErrorMessage;
			}

			internal GenericTypeMapping(Type requestedType, string errorMessage)
			{
				RequestedType = requestedType;
				BindErrorMessage = errorMessage;
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
		/// 
		/// This uses the <see cref="MapType(Type)"/> function to determine success, but only checks the <see cref="GenericTypeMapping.Success"/>
		/// flag.  As a result, this method will return true if an open generic base or interface of <see cref="DeclaredType"/>
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool SupportsType(Type type)
		{
			//note - the base test will return true for any generic base or interface of DeclaredType - including where
			//an open generic is passed which is the same a DeclaredType.
			if (base.SupportsType(type))
				return true;
			return MapType(type).Success;
		}

		/// <summary>
		/// Maps the <see cref="DeclaredType"/> open generic type to the <paramref name="targetType"/>.
		/// 
		/// Examine the <see cref="GenericTypeMapping.Success"/> of the result to check whether the
		/// result was successful.
		/// </summary>
		/// <param name="targetType">Type of the target.</param>
		public GenericTypeMapping MapType(Type targetType)
		{
			//used both in SupportsType and in the Bind function - except the bind function
			//uses the error message as exception text
			//if (TypeHelpers.IsGenericTypeDefinition(targetType))
			//	return new GenericTypeMapping($"The type { targetType } is an open generic and therefore can't be mapped to a closed version of { DeclaredType }");
			if (!TypeHelpers.IsGenericType(targetType))
				return new GenericTypeMapping(targetType, $"The type { DeclaredType } cannot be mapped to non-generic type { targetType }");

			var genericType = TypeHelpers.IsGenericTypeDefinition(targetType) ? targetType : targetType.GetGenericTypeDefinition();
			Type[] suppliedTypeArguments = EmptyTypes;
			Type[] finalTypeArguments = EmptyTypes;
			if (genericType == DeclaredType)
			{
				finalTypeArguments = TypeHelpers.GetGenericArguments(targetType);
			}
			else
			{
				finalTypeArguments = MapGenericParameters(targetType);

				if (finalTypeArguments == null)
					return new GenericTypeMapping(targetType, $"The requested type { targetType } is not a generic base or interface of { DeclaredType }");

				var genericMismatches = finalTypeArguments.Where(a => TypeHelpers.IsAssignableFrom(typeof(ITypeArgGenericMismatch), a)).ToArray();

				if (genericMismatches.Length != 0)
				{
					return new GenericTypeMapping(targetType, $@"One or more type arguments in the requested type { targetType } are 'less generic' than those of { DeclaredType }: {
						string.Join(", ", genericMismatches.Select(t =>
						{
							var mismatchedArgs = TypeHelpers.GetGenericArguments(t);
							return $@"{
								mismatchedArgs[0] } in { (mismatchedArgs[1] != typeof(INotGeneric) ? mismatchedArgs[1].ToString() : "[not generic]")
							} is mapped to less generic argument {
								mismatchedArgs[2] } of { (mismatchedArgs[3] != typeof(INotGeneric) ? mismatchedArgs[3].ToString() : "[not generic]")
							}";
						})
						)}");
				}

				if (finalTypeArguments.Length == 0 || finalTypeArguments.Any(t => t == null) || finalTypeArguments.Any(t => t.IsGenericParameter))
				{
					//if we were mapping to an open generic (generic type def), then the mapping is successful, so Success=true, but
					//we can't create an instance of the type unless we try the mapping again with a closed version of the same
					//type.  So that's what the error message here is for - it'll be used as an exception if Bind is called with the same
					//type, even though SupportsType would return true.
					//TODO: change dis to deep search for *ANY* open generics
					if (TypeHelpers.ContainsGenericParameters(targetType))
						return new GenericTypeMapping(targetType, DeclaredType, $"{ targetType } should be compatible with { DeclaredType }, but since it is an open generic type, no instance can be constructed");
					else
						return new GenericTypeMapping(targetType, $"There is not enough generic type information from { targetType } to map all generic arguments to { DeclaredType }.  This is likely to be because { targetType } has fewer type parameters than { DeclaredType }");
				}
			}

			//make the generic type
			return new GenericTypeMapping(targetType, DeclaredType.MakeGenericType(finalTypeArguments));
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
			//if the mapping fails, or the mapping is not fully bound to a closed generic, throw an exception
			if (!mapTypeResult.Success || !mapTypeResult.IsFullyBound)
				throw new ArgumentException(mapTypeResult.BindErrorMessage, nameof(context));
			//construct the constructortarget
			return Target.ForType(mapTypeResult.Type, MemberBindingBehaviour);
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

						//todo: if mapping is null, then we need to examine the generic constraints on t (or possibly its base or interfaces), which also works
						// - there's currently a GenericConstructorTargets test containing the ValidWideningGeneric type - which should be able to be supported.  If we're clever.

						return new
						{
							DeclaredTypeParamPosition = t.GenericParameterPosition,
							Type = t,
							//the projection here allows us to get the index of the base interface's generic type parameter
							//It is required because using the GenericParameterPosition property simply returns the index of the 
							//type in our declared type, as the type is passed down into the interfaces from the open generic
							//but closes them over those very types.  Thus, the <T> from an open generic class Foo<T> is passed down
							//to IFoo<T> almost as if it were a proper type, and the <T> in IFoo<> is actually equal to the <T> from Foo<T>.
							Mapping = mapping
						};
					}).OrderBy(r => r.Mapping != null ? r.Mapping[0].Index : int.MinValue).ToArray();

				var suppliedTypeArguments = TypeHelpers.GetGenericArguments(requestedType);
				Type suppliedArg = null;
				foreach (var typeParam in typeParamPositions.Where(p => p.Mapping != null))
				{
					//now, just because we've found a mapping, doesn't actually mean that we have success.
					//consider a declared type of IFoo<IEnumerable<T>> and a requested type of IFoo<T> - 
					//in this case it's not possible to map, because it only works if the T in the second IFoo<>
					//is IEnumerable<T>.
					//When it's the other way around - i.e. declared is IFoo<T> and requested is IFoo<IEnumerable<T>>, 
					//then it's possible to map, but we can only create if we are ultimmately given a concrete
					//IEnumerable<T> as the inner argument.
					suppliedArg = suppliedTypeArguments[typeParam.Mapping[0].Index];
					foreach (var mapping in typeParam.Mapping.Skip(1))
					{
						//so if the next parameter is not a generic type, then we can't get the arguments
						if (!TypeHelpers.IsGenericType(suppliedArg))
						{
							suppliedArg = typeof(ITypeArgGenericMismatch<,,,>).MakeGenericType(
								mapping.BaseTypeArgument, 
								mapping.BaseType ?? typeof(INotGeneric), 
								suppliedArg, 
								suppliedArg.DeclaringType ?? typeof(INotGeneric));
							break;
						}
						else
							suppliedArg = TypeHelpers.GetGenericArguments(suppliedArg)[mapping.Index];
					}
					finalTypeArguments[typeParam.DeclaredTypeParamPosition] = suppliedArg;
				}
			}
			else
				return null; //means that no mappings exist - because the types are not even compatible

			//note - some arguments might get left as generic parameters in failed scenarios
			return finalTypeArguments;
		}

		private class GenericParameterMapping
		{
			/// <summary>
			/// Gets or sets the index of the mapper parameter in the array of type parameters for <see cref="BaseType"/>
			/// </summary>
			/// <value>The index.</value>
			public int Index { get; set; }
			/// <summary>
			/// Gets or sets the type argument in <see cref="BaseType"/> represented by this mapping.
			/// </summary>
			/// <value>The base type parameter.</value>
			public Type BaseTypeArgument { get; set; }
			/// <summary>
			/// Gets or sets the type to which this mapping relates.  The type will be a base or interface of the <see cref="DeclaredType"/>
			/// </summary>
			/// <value>The type of the base.</value>
			public Type BaseType { get; set; }
			public Type TargetParameter { get; set; }
		}

		/// <summary>
		/// returns a series of type parameter indexes from the baseType parameter which can be used to derive
		/// the concrete type parameter to be used in a target type, given a fully-closed generic type as the model
		/// </summary>
		/// <param name="previousTypeParameterPositions"></param>
		/// <param name="baseTypeParameter"></param>
		/// <param name="targetTypeParameter"></param>
		/// <returns></returns>
		private GenericParameterMapping[] DeepSearchTypeParameterMapping(Stack<GenericParameterMapping> previousTypeParameterPositions, Type baseTypeParameter, Type targetTypeParameter)
		{
			if (baseTypeParameter == targetTypeParameter)
				return previousTypeParameterPositions.ToArray();
			if (previousTypeParameterPositions == null)
				previousTypeParameterPositions = new Stack<GenericParameterMapping>();
			if (TypeHelpers.IsGenericType(baseTypeParameter))
			{
				var args = TypeHelpers.GetGenericArguments(baseTypeParameter);
				GenericParameterMapping[] result = null;
				for (int f = 0; f < args.Length; f++)
				{
					previousTypeParameterPositions.Push(new GenericParameterMapping() { BaseType = baseTypeParameter, BaseTypeArgument = args[f], Index = f, TargetParameter = targetTypeParameter });
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
        [Obsolete("This method has been replaced by the Target.ForType method and will be removed in 1.2.")]
		public static ITarget Auto(Type type, IMemberBindingBehaviour memberBindingBehaviour = null)
		{ 
			return new GenericConstructorTarget(type);
		}
	}
}
