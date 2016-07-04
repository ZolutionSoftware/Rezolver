// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
  /// <summary>
  /// Special version of IObjectTargetMetadata which creates an instance of the 
  /// requested type from a JToken.  Some more work might be required here
  /// to 
  /// </summary>
  public class LazyJsonObjectTargetMetadata : ObjectTargetMetadataBase
  {
    private readonly object _locker = new object();
    private object _object;
    private readonly JToken _token;
    private readonly JsonSerializer _serializer;
    //as with the out-of-the-box object target, all instances of this start off as unbound
    private readonly ITypeReference _declaredType;
    public override ITypeReference DeclaredType
    {
      get { return _declaredType; }
    }

    internal LazyJsonObjectTargetMetadata(JToken token, JsonSerializer serializer)
      : this(token, serializer, TypeReference.Unbound)
    {

    }

    private LazyJsonObjectTargetMetadata(JToken token, JsonSerializer serializer, ITypeReference declaredType)
    {
      _token = token;
      _serializer = serializer;
      _declaredType = declaredType;
    }

    protected override IRezolveTargetMetadata BindBase(params ITypeReference[] targetTypes)
    {
      //this is awkward - because registrations can occur for more than one type - legitimately so.
      //so for an unbound literal value from JSON we can't restrict the user to only ever having one
      //type, we just have to choose the first in the array.

      //if(targetTypes.Length != 1)
      //	throw new ArgumentException("Must only have one unambiguous target type for this target to bind to");

      return new LazyJsonObjectTargetMetadata(_token, _serializer, targetTypes[0]);
    }

    public override object GetObject(Type type)
    {
      if (type == null) throw new ArgumentNullException("type");

      if (_object == null)
      {
        lock (_locker)
        {
          if (_object == null)
          {
            try
            {
              _object = _token.ToObject(type, _serializer);
            }
            catch (Exception ex)
            {
              throw new ArgumentException(string.Format("Unable to deserialize object of type {0} from the underlying JSON data", type), ex);
            }
          }
        }
      }
      return _object;
    }
  }
}
