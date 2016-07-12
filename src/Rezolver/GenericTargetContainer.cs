// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
  public class GenericTargetContainer : TargetDictionaryContainer
  {
    private readonly Type _genericType;
    private TargetListContainer _targets;
    //private Dictionary<Type, TargetListContainer> _targets;
    public GenericTargetContainer(Type genericType)
    {
      _genericType = genericType;
      _targets = new TargetListContainer(genericType);
    }

    public override void Register(ITarget target, Type serviceType = null)
    {
      if (serviceType == null) serviceType = target.DeclaredType;

      //if the type we're adding against is equal to this container's generic type definition,
      //then we add it to the collection of targets that are registered specifically against
      //this type.
      if (serviceType == _genericType)
        _targets.Register(target, serviceType);
      else
      {
        //the type MUST therefore be a closed generic over the generic type definition,
        //if it's not, then we must throw an exception
        if (!TypeHelpers.IsGenericType(serviceType) || serviceType.GetGenericTypeDefinition() != _genericType)
          throw new Exception($"Type must be equal to the generic type definition { _genericType } or a closed instance of that type");
        base.Register(target, serviceType);
      }
    }

    public override ITarget Fetch(Type type)
    {
      //don't bother checking type validity, just find the container entry with the 
      //given type and return the result of its fetch function.
      //If we don't find one - then we return the targets that have been registered against the 
      //open generic type definition.
      ITarget baseResult = null;
      foreach (var searchType in DeriveGenericTypeSearchList(type))
      {
        if ((baseResult = base.Fetch(searchType)) != null)
          return baseResult;
      }

      //no direct match for any of the search types in our dictionary - so resort to the 
      //targets that have been registered directly against the open generic type.
      return _targets.Fetch(type);
    }

    public override IEnumerable<ITarget> FetchAll(Type type)
    {
      IEnumerable<ITarget> baseResults = null;
      //this is similar to the fetch method, except it'll return as soon as it gets
      //a non-empty hit for any of the search types.
      foreach (var searchType in DeriveGenericTypeSearchList(type))
      {
        if ((baseResults = base.FetchAll(searchType)).Any())
          return baseResults;
      }

      return _targets.FetchAll(type);
    }

    private IEnumerable<Type> DeriveGenericTypeSearchList(Type type)
    {
      //for IFoo<IEnumerable<Nullable<T>>>, this should return something like
      //IFoo<IEnumerable<Nullable<T>>>, 
      //IFoo<IEnumerable<Nullable<>>>, 
      //IFoo<IEnumerable<>>,
      //IFoo<>

      //using an iterator method is not the best for performance, but fetching type
      //registrations from a container builder is an operation that, so long as a caching
      //resolver is used, shouldn't be repeated often.

      if (!TypeHelpers.IsGenericType(type) || TypeHelpers.IsGenericTypeDefinition(type))
      {
        yield return type;
        yield break;
      }

      //for every generic type, there is at least two versions - the closed and the open
      //when you consider, also, that a generic parameter might also be a generic, with multiple
      //versions - you can see that things can get icky.  
      var typeParams = TypeHelpers.GetGenericArguments(type);
      var typeParamSearchLists = typeParams.Select(t => DeriveGenericTypeSearchList(t).ToArray()).ToArray();
      var genericType = type.GetGenericTypeDefinition();

      foreach (var combination in CartesianProduct(typeParamSearchLists))
      {
        yield return genericType.MakeGenericType(combination.ToArray());
      }
      yield return genericType;
    }

    static IEnumerable<IEnumerable<T>> CartesianProduct<T>
    (IEnumerable<IEnumerable<T>> sequences)
    {
      //thank you Eric Lippert...
      IEnumerable<IEnumerable<T>> emptyProduct =
        new[] { Enumerable.Empty<T>() };
      return sequences.Aggregate(
        emptyProduct,
        (accumulator, sequence) =>
          from accseq in accumulator
          from item in sequence
          select accseq.Concat(new[] { item }));
    }

  }
}
