// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;
using Rezolver.Runtime;

namespace Rezolver.Targets
{
    /// <summary>
    /// Equivalent of <see cref="ConstructorTarget"/> but for open generic types.
    ///
    /// So, this will handle the open generic MyType&lt;,&gt;, for example, whereas <see cref="ConstructorTarget"/>
    /// would handle the closed type MyType&lt;int, string&gt;.
    /// </summary>
    /// <seealso cref="TargetBase" />
    public partial class GenericConstructorTarget : TargetBase
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

        private static Type[] EmptyTypes = new Type[0];

        private readonly Type _genericType;
        /// <summary>
        /// Gets the generic type definition from which generic types are to be built and instances of which
        /// will be constructed.
        /// </summary>
        /// <remarks></remarks>
        public Type GenericType { get => GenericTypeConstructor?.DeclaringType ?? this._genericType; }

        /// <summary>
        /// If supplied on construction, then this is a constructor declared on the <see cref="GenericType"/> which is to be
        /// used for every instance that is created.
        /// </summary>
        /// <remarks>When set, the <see cref="GenericType"/> is derived from the <see cref="MemberInfo.DeclaringType"/> of this
        /// reference.</remarks>
        public ConstructorInfo GenericTypeConstructor { get; }

        /// <summary>
        /// Gets the member binding behaviour to be used when creating an instance.  The rules for this are the same as for
        /// <see cref="ConstructorTarget.MemberBindingBehaviour"/>.
        /// </summary>
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
        /// which will utilise the optional <paramref name="memberBinding"/> when it constructs its
        /// <see cref="ConstructorTarget"/> when <see cref="Bind(ICompileContext)"/> is called.
        /// </summary>
        /// <param name="genericType">Required. The type of the object that is to be built (open generic of course)</param>
        /// <param name="memberBinding">Optional. Provides an explicit member injection behaviour to be used when creating the instance.
        /// If not provided, then the <see cref="Bind(ICompileContext)"/> method will attempt to obtain one via the options API from the
        /// <see cref="ICompileContext"/> - and if one is still not available, then no member binding will be performed.</param>
        public GenericConstructorTarget(Type genericType, IMemberBindingBehaviour memberBinding = null)
        {
            if (genericType == null)
                throw new ArgumentNullException(nameof(genericType));

            if (!genericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("The generic constructor target currently only supports fully open generics.  Use ConstructorTarget for closed generics.");
            }

            if (genericType.IsAbstract || genericType.IsInterface)
            {
                throw new ArgumentException("The type must be a generic type definition of either a non-abstract class or value type.");
            }

            this._genericType = genericType;
            MemberBindingBehaviour = memberBinding;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="GenericConstructorTarget"/> type.
        /// </summary>
        /// <param name="genericConstructor">Required. The constructor on a generic type definition that will be used for
        /// each concrete generic instance produced by this target.  Must be a member of a generic type definition that is
        /// neither an abstract class or interface.</param>
        /// <param name="memberBinding">Optional. Provides an explicit member injection behaviour to be used when creating the instance.
        /// If not provided, then the <see cref="Bind(ICompileContext)"/> method will attempt to obtain one via the options API from the
        /// <see cref="ICompileContext"/> - and if one is still not available, then no member binding will be performed.</param>
        public GenericConstructorTarget(ConstructorInfo genericConstructor, IMemberBindingBehaviour memberBinding = null)
        {
            if (genericConstructor == null)
            {
                throw new ArgumentNullException(nameof(genericConstructor));
            }

            Type genericType = genericConstructor.DeclaringType;
            if (!genericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"The supplied constructor {genericConstructor} does not belong to an open generic type.  Use ConstructorTarget for non-generic constructors");
            }

            if (genericType.IsAbstract || genericType.IsInterface)
            {
                throw new ArgumentException($"The type which owns the given constructor ({genericConstructor}) must be a generic type definition of either a non-abstract class or value type.");
            }

            GenericTypeConstructor = genericConstructor;
            MemberBindingBehaviour = memberBinding;
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
            // note - the base test will return true for any generic base or interface of DeclaredType - including where
            // an open generic is passed which is the same a DeclaredType.
            if (base.SupportsType(type))
            {
                return true;
            }

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
            if (!targetType.IsGenericType)
            {
                return new GenericTypeMapping(targetType, GetMappingError_DeclaredTypeCannotBeMapped(DeclaredType, targetType));
            }

            var genericType = targetType.IsGenericTypeDefinition ? targetType : targetType.GetGenericTypeDefinition();
            Type[] suppliedTypeArguments = EmptyTypes;
            Type[] finalTypeArguments = EmptyTypes;
            if (genericType == DeclaredType)
            {
                finalTypeArguments = targetType.GetGenericArguments();
            }
            else
            {
                finalTypeArguments = MapGenericParameters(targetType);

                if (finalTypeArguments == null)
                {
                    return new GenericTypeMapping(targetType, GetMappingError_NotABaseOrInterface(DeclaredType, targetType));
                }

                var genericMismatches = finalTypeArguments.Where(a => typeof(ITypeArgGenericMismatch).IsAssignableFrom(a)).ToArray();

                if (genericMismatches.Length != 0)
                {
                    return new GenericTypeMapping(targetType, GetMappingError_ArgumentsLessGeneric(DeclaredType, targetType, genericMismatches));
                }

                if (finalTypeArguments.Length == 0 || finalTypeArguments.Any(t => t == null) || finalTypeArguments.Any(t => t.IsGenericParameter))
                {
                    // if we were mapping to an open generic (generic type def), then the mapping is successful, so Success=true, but
                    // we can't create an instance of the type unless we try the mapping again with a closed version of the same
                    // type.  So that's what the error message here is for - it'll be used as an exception if Bind is called with the same
                    // type, even though SupportsType would return true.
                    // TODO: change dis to deep search for *ANY* open generics
                    if (targetType.ContainsGenericParameters)
                    {
                        return new GenericTypeMapping(targetType, DeclaredType, bindErrorMessage: GetMappingError_OpenGenericResult(DeclaredType, targetType));
                    }
                    else
                    {
                        return new GenericTypeMapping(targetType, GetMappingError_NotEnoughTypeInformation(DeclaredType, targetType));
                    }
                }
            }

            Type targetGeneric;
            ConstructorInfo mappedConstructor = null;
            try
            {
                // make the generic type
                targetGeneric = DeclaredType.MakeGenericType(finalTypeArguments);
            }
            catch (TypeLoadException tlex)
            {
                return new GenericTypeMapping(targetType, GetMappingError_TypeLoadException(DeclaredType, finalTypeArguments, tlex));
            }
            catch (ArgumentException aex)
            {
                return new GenericTypeMapping(targetType, GetMappingError_InvalidTypeArguments(DeclaredType, finalTypeArguments, aex));
            }

            // now, if we have a constructor, we have to get the correct version for this generic type
            if (GenericTypeConstructor != null)
            {
                mappedConstructor = GenericTypeConstructor.ToGenericTypeCtor(targetGeneric);
                if (mappedConstructor == null)
                {
                    return new GenericTypeMapping(targetType, targetGeneric, bindErrorMessage: $"Could not map the constructor {GenericTypeConstructor} from the type {GenericType} to the type {targetGeneric}");
                }
            }

            return new GenericTypeMapping(targetType, targetGeneric, mappedConstructor);
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
            if(context == null) throw new ArgumentNullException(nameof(context));

            // always create a constructor target from new
            // basically this class simply acts as a factory for other constructor targets.

            var expectedType = context.TargetType;
            if (expectedType == null)
            {
                throw new ArgumentException("GenericConstructorTarget requires a concrete TargetType to be passed in the CompileContext.", nameof(context));
            }

            var mapTypeResult = MapType(context.TargetType);
            // if the mapping fails, or the mapping is not fully bound to a closed generic, throw an exception
            if (!mapTypeResult.Success || !mapTypeResult.IsFullyBound)
            {
                throw new ArgumentException(mapTypeResult.BindErrorMessage, nameof(context));
            }

            // construct the constructortarget
            if (mapTypeResult.Constructor != null)
            {
                return Target.ForConstructor(mapTypeResult.Constructor, MemberBindingBehaviour);
            }
            else
            {
                return Target.ForType(mapTypeResult.Type, MemberBindingBehaviour);
            }
        }

        private Type[] MapGenericParameters(Type requestedType)
        {
            var requestedTypeGenericDefinition = requestedType.GetGenericTypeDefinition();
            Type[] finalTypeArguments = DeclaredType.GetGenericArguments();
            // check whether it's a base or an interface
            var mappedBase = requestedTypeGenericDefinition.IsInterface ?
                DeclaredType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == requestedTypeGenericDefinition)
                : DeclaredType.GetAllBases().SingleOrDefault(b => b.IsGenericType && b.GetGenericTypeDefinition() == requestedTypeGenericDefinition);
            if (mappedBase != null)
            {
                var baseTypeParams = mappedBase.GetGenericArguments();
                var typeParamPositions = DeclaredType.GetGenericArguments()
                    .Select(t =>
                    {
                        var mapping = DeepSearchTypeParameterMapping(null, mappedBase, t);

                        // todo: if mapping is null, then we need to examine the generic constraints on t (or possibly its base or interfaces), which also works
                        // - there's currently a GenericConstructorTargets test containing the ValidWideningGeneric type - which should be able to be supported.  If we're clever.

                        return new
                        {
                            DeclaredTypeParamPosition = t.GenericParameterPosition,
                            Type = t,
                            // the projection here allows us to get the index of the base interface's generic type parameter
                            // It is required because using the GenericParameterPosition property simply returns the index of the
                            // type in our declared type, as the type is passed down into the interfaces from the open generic
                            // but closes them over those very types.  Thus, the <T> from an open generic class Foo<T> is passed down
                            // to IFoo<T> almost as if it were a proper type, and the <T> in IFoo<> is actually equal to the <T> from Foo<T>.
                            Mapping = mapping
                        };
                    }).OrderBy(r => r.Mapping != null ? r.Mapping[0].Index : int.MinValue).ToArray();

                var suppliedTypeArguments = requestedType.GetGenericArguments();
                Type suppliedArg = null;
                foreach (var typeParam in typeParamPositions.Where(p => p.Mapping != null))
                {
                    // now, just because we've found a mapping, doesn't actually mean that we have success.
                    // consider a declared type of IFoo<IEnumerable<T>> and a requested type of IFoo<T> -
                    // in this case it's not possible to map, because it only works if the T in the second IFoo<>
                    // is IEnumerable<T>.
                    // When it's the other way around - i.e. declared is IFoo<T> and requested is IFoo<IEnumerable<T>>,
                    // then it's possible to map, but we can only create if we are ultimmately given a concrete
                    // IEnumerable<T> as the inner argument.
                    suppliedArg = suppliedTypeArguments[typeParam.Mapping[0].Index];
                    foreach (var mapping in typeParam.Mapping.Skip(1))
                    {
                        // so if the next parameter is not a generic type, then we can't get the arguments
                        if (!suppliedArg.IsGenericType)
                        {
                            suppliedArg = typeof(ITypeArgGenericMismatch<,,,>).MakeGenericType(
                                mapping.BaseTypeArgument,
                                mapping.BaseType ?? typeof(INotGeneric),
                                suppliedArg,
                                suppliedArg.DeclaringType ?? typeof(INotGeneric));
                            break;
                        }
                        else
                        {
                            suppliedArg = suppliedArg.GetGenericArguments()[mapping.Index];
                        }
                    }

                    finalTypeArguments[typeParam.DeclaredTypeParamPosition] = suppliedArg;
                }
            }
            else
            {
                return null; // means that no mappings exist - because the types are not even compatible
            }

            // note - some arguments might get left as generic parameters in failed scenarios
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
        /// Returns a series of type parameter indexes from the baseType parameter which can be used to derive
        /// the concrete type parameter to be used in a target type, given a fully-closed generic type as the model
        /// </summary>
        /// <param name="previousTypeParameterPositions"></param>
        /// <param name="baseTypeParameter"></param>
        /// <param name="targetTypeParameter"></param>
        /// <returns></returns>
        private GenericParameterMapping[] DeepSearchTypeParameterMapping(Stack<GenericParameterMapping> previousTypeParameterPositions, Type baseTypeParameter, Type targetTypeParameter)
        {
            if (baseTypeParameter == targetTypeParameter)
            {
                var result = previousTypeParameterPositions.ToArray();
                Array.Reverse(result);
                return result;
            }

            if (previousTypeParameterPositions == null)
            {
                previousTypeParameterPositions = new Stack<GenericParameterMapping>();
            }

            if (baseTypeParameter.IsGenericType)
            {
                var args = baseTypeParameter.GetGenericArguments();
                GenericParameterMapping[] result = null;
                for (int f = 0; f < args.Length; f++)
                {
                    previousTypeParameterPositions.Push(new GenericParameterMapping() { BaseType = baseTypeParameter, BaseTypeArgument = args[f], Index = f, TargetParameter = targetTypeParameter });
                    result = DeepSearchTypeParameterMapping(previousTypeParameterPositions, args[f], targetTypeParameter);
                    previousTypeParameterPositions.Pop();
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

#region mapping error message generators
        private static string GetMappingError_InvalidTypeArguments(Type genericType, Type[] finalTypeArguments, ArgumentException aex)
        {
            return $"One or more type arguments passed to the generic type {genericType} are invalid.  The type arguments passed were {{{string.Join(", ", finalTypeArguments.AsEnumerable())}}}.  Exception message: {aex.Message}";
        }

        private static string GetMappingError_TypeLoadException(Type genericType, Type[] finalTypeArguments, TypeLoadException tlex)
        {
            return $"Could not construct generic type {genericType} with type arguments {{{string.Join(", ", finalTypeArguments.AsEnumerable())}}} - TypeLoadException occurred: {tlex.Message}";
        }

        private static string GetMappingError_NotEnoughTypeInformation(Type genericType, Type requestedType)
        {
            return $"There is not enough generic type information from {requestedType} to map all generic arguments to {genericType}.  This is most likely because {requestedType} has fewer type parameters than {genericType}";
        }

        private static string GetMappingError_OpenGenericResult(Type genericType, Type requestedType)
        {
            return $"{requestedType} should be compatible with {genericType}, but since it is an open generic type, no instance can be constructed";
        }

        private static string GetMappingError_ArgumentsLessGeneric(Type genericType, Type requestedType, Type[] genericMismatches)
        {
            return $@"One or more type arguments in the requested type {requestedType} are 'less generic' than those of {genericType}: {
                                    string.Join(", ", genericMismatches.Select(t =>
                                    {
                                        var mismatchedArgs = t.GetGenericArguments();
                                        return $@"{
                                            mismatchedArgs[0]} in {(mismatchedArgs[1] != typeof(INotGeneric) ? mismatchedArgs[1].ToString() : "[not generic]")} is mapped to less generic argument {
                                            mismatchedArgs[2]} of {(mismatchedArgs[3] != typeof(INotGeneric) ? mismatchedArgs[3].ToString() : "[not generic]")}";
                                    })
                                    )}
";
        }

        private static string GetMappingError_NotABaseOrInterface(Type genericType, Type requestedType)
        {
            return $"The requested type {requestedType} is not a generic base or interface of {genericType}";
        }

        private static string GetMappingError_DeclaredTypeCannotBeMapped(Type genericType, Type requestedType)
        {
            return $"The type {genericType} cannot be mapped to non-generic type {requestedType}";
        }
#endregion
    }
}
