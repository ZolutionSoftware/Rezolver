// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  public class GenericConstructorTarget : TargetBase
  {
    private static Type[] EmptyTypes = new Type[0];

    private Type _genericType;
    private IPropertyBindingBehaviour _propertyBindingBehaviour;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericType">The type of the object that is to be built (open generic of course)</param>
    /// <param name="propertyBindingBehaviour">Optional.  The <see cref="IPropertyBindingBehaviour"/> to be used for binding properties and/or fields on the 
    /// 'new' expression that is generated.  If null, then no property or fields will be bound on construction.</param>
    public GenericConstructorTarget(Type genericType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      if (!TypeHelpers.IsGenericTypeDefinition(genericType))
        throw new ArgumentException("The generic constructor target currently only supports fully open generics.  Partially open generics are not yet supported, and for fully closed generics, use ConstructorTarget");
      if (!TypeHelpers.IsClass(genericType) || TypeHelpers.IsAbstract(genericType))
        throw new ArgumentException("The type must be a non-abstract generic class");
      _genericType = genericType;
      _propertyBindingBehaviour = propertyBindingBehaviour;
    }

    /// <summary>
    /// Override - introduces additional logic to cope with generic types not generally supported by the majority of other targets.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override bool SupportsType(Type type)
    {
      if (base.SupportsType(type))
        return true;

      //scenario - requested type is a closed generic built from this target's open generic
      if (!TypeHelpers.IsGenericType(type))
        return false;

      var genericType = type.GetGenericTypeDefinition();
      if (genericType == DeclaredType)
        return true;

      if (!TypeHelpers.IsInterface(genericType))
      {
        var bases = DeclaredType.GetAllBases();
        var matchedBase = bases.FirstOrDefault(b => TypeHelpers.IsGenericType(b) && b.GetGenericTypeDefinition() == genericType);
        if (matchedBase != null)
          return true;
      }
      //TODO: tighten this up to handle the proposed partially open type
      else if (TypeHelpers.GetInterfaces(DeclaredType).Any(t => TypeHelpers.IsGenericType(t) && t.GetGenericTypeDefinition() == genericType))
        return true;

      return false;
    }

    /// <summary>
    /// Determines the generic type to be bound, and then generates a <see cref="System.Linq.Expressions.NewExpression"/> for that type.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override System.Linq.Expressions.Expression CreateExpressionBase(CompileContext context)
    {
      //always create a constructor target from new
      //basically this class simply acts as a factory for other constructor targets.

      var expectedType = context.TargetType;
      if (expectedType == null)
        throw new ArgumentException("GenericConstructorTarget requires a concrete to be passed in the CompileContext - by definition it cannot simply create a default instance of the target type.", "context");
      if (!TypeHelpers.IsGenericType(expectedType))
        throw new ArgumentException("The compile context requested an instance of a non-generic type to be built.", "context");

      var genericType = expectedType.GetGenericTypeDefinition();
      Type[] suppliedTypeArguments = EmptyTypes;
      Type[] finalTypeArguments = EmptyTypes;
      if (genericType == DeclaredType)
      {
        finalTypeArguments = TypeHelpers.GetGenericArguments(expectedType);
      }
      else
      {
        if (TypeHelpers.IsGenericType(expectedType))
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
      Type[] finalTypeArguments = TypeHelpers.GetGenericArguments(targetType);
      //check whether it's a base or an interface
      var mappedBase = TypeHelpers.IsInterface(requestedTypeGenericDefinition) ?
          TypeHelpers.GetInterfaces(targetType).FirstOrDefault(t => TypeHelpers.IsGenericType(t) && t.GetGenericTypeDefinition() == requestedTypeGenericDefinition)
          : targetType.GetAllBases().SingleOrDefault(b => TypeHelpers.IsGenericType(b) && b.GetGenericTypeDefinition() == requestedTypeGenericDefinition);
      if (mappedBase != null)
      {
        var baseTypeParams = TypeHelpers.GetGenericArguments(mappedBase);
        var typeParamPositions = TypeHelpers.GetGenericArguments(targetType)
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
    /// Implementation of the abstract base property.  Will retrn the unbound generic type passed to this object on construction.
    /// </summary>
    public override System.Type DeclaredType
    {
      get { return _genericType; }
    }


    /// <summary>
    /// Equivalent of <see cref="ConstructorTarget.Auto{T}(IPropertyBindingBehaviour)"/> for generic type definitions although,
    /// note that it's not possible to bind this generic method in C# directly from source code.
    /// </summary>
    /// <typeparam name="TGeneric"></typeparam>
    /// <param name="propertyBindingBehaviour"></param>
    /// <returns></returns>
    public static ITarget Auto<TGeneric>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      return Auto(typeof(TGeneric), propertyBindingBehaviour);
    }

    public static ITarget Auto(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      //I might relax this constraint later - since we could implement partially open generics.
      if (!TypeHelpers.IsGenericTypeDefinition(type))
        throw new ArgumentException("The passed type must be an open generic type");
      if (!TypeHelpers.IsClass(type) || TypeHelpers.IsAbstract(type))
        throw new ArgumentException("The passed type must a non-abstract class");
      return new GenericConstructorTarget(type);
    }
  }
}
