// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// Context of a call to an IRezolver's Resolve method.  The container is included
  /// in the context to allow code generated from <see cref="ITarget.CreateExpression(CompileContext)"/> to refer back to the container.
  /// 
  /// This also allows us to retarget compiled targets at other containers (e.g. <see cref="OverridingContainer"/>s
  /// that override existing registrations or define new ones).
  /// </summary>
  public class RezolveContext : IEquatable<RezolveContext>
  {
    public static readonly RezolveContext EmptyContext = new RezolveContext(null);

    private class StubContainer : IContainer
    {
      private static readonly StubContainer _instance = new StubContainer();

      public static StubContainer Instance
      {
        get
        {
          return _instance;
        }
      }

      public ITargetCompiler Compiler
      {
        get { throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set")); }
      }

      public ITargetContainer Targets
      {
        get { throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set")); }
      }

      public bool CanResolve(RezolveContext context)
      {
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }

      public object Resolve(RezolveContext context)
      {
        context.MustNotBeNull("context");
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }

      public bool TryResolve(RezolveContext context, out object result)
      {
        context.MustNotBeNull("context");
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }

      public IScopedContainer CreateLifetimeScope()
      {
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }

      public ICompiledTarget FetchCompiled(RezolveContext context)
      {
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }

      public void Register(ITarget target, Type type = null)
      {
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }

      public IScopedContainer CreateLifetimeScope(IContainer overridingRezolver)
      {
        throw new NotImplementedException();
      }

      object IServiceProvider.GetService(Type serviceType)
      {
        throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
      }
    }

    private Type _requestedType;
    public Type RequestedType { get { return _requestedType; } private set { _requestedType = value; } }

    private IContainer _container;

    /// <summary>
    /// The container for this context.
    /// </summary>
    public IContainer Container { get { return _container; } private set { _container = value; } }

    private IScopedContainer _scope;
    public IScopedContainer Scope { get { return _scope; } private set { _scope = value; } }

    private RezolveContext() { }

    public RezolveContext(IContainer container, Type requestedType)
      : this(container)
    {
      RequestedType = requestedType;
    }

    public RezolveContext(IContainer container, Type requestedType, IScopedContainer scope)
      : this(container)
    {
      RequestedType = requestedType;
      Scope = scope;
    }

    private RezolveContext(IContainer container)
    {
      _container = container ?? StubContainer.Instance;
      //automatically inherit the container as this context's scope, if it's of the correct type.
      //note - all the other constructors chain to this one.  Note that other constructors
      //might supply a separate scope in addition, which will overwrite the scope set here.
      _scope = container as IScopedContainer;
    }

    public override string ToString()
    {
      List<string> parts = new List<string>();

      parts.Add($"Type: {RequestedType}");
      parts.Add($"Container: {Container}");
      if (Scope != null)
      {
        if (Scope == Container)
          parts[parts.Count - 1] = $"Scope Container: {Scope}";
        else
          parts.Add($"Scope: {Scope}");
      }

      return $"({string.Join(", ", parts)})";
    }

    public override int GetHashCode()
    {
      return _requestedType.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      return Equals(obj as RezolveContext);
    }

    public virtual bool Equals(RezolveContext other)
    {
      return object.ReferenceEquals(this, other) || _requestedType == other._requestedType;
    }

    public static bool operator ==(RezolveContext left, RezolveContext right)
    {
      //same ref - yes
      if (object.ReferenceEquals(left, right))
        return true;
      //one is null, the other not - short-circuit
      //have to be careful not to do left == null or right == null here or we stackoverflow...
      if (object.ReferenceEquals(null, left) != object.ReferenceEquals(null, right))
        return false;
      //now standard equality check on type/name
      return left._requestedType == right._requestedType;
    }

    public static bool operator !=(RezolveContext left, RezolveContext right)
    {
      //same reference
      if (object.ReferenceEquals(left, right))
        return false;
      //one is null, the other isn't - short-circuit
      //have to be careful not to do left == null or right == null here or we stackoverflow ...
      if (object.ReferenceEquals(null, left) != object.ReferenceEquals(null, right))
        return true;
      //now standard inequality check on type/name
      return left._requestedType != right._requestedType;

      //TODO: Going to need to think of a way to bring in user-defined equalities in here - for those
      //contexts where the registration does 'interesting' things with the context.
    }

    /// <summary>
    /// Returns a clone of this context, but replaces the type, establishing the root context relationship
    /// also, either by inheriting this one's root context, or setting this as the root context.
    /// </summary>
    /// <param name="requestedType"></param>
    /// <returns></returns>
    public RezolveContext CreateNew(Type requestedType)
    {
      return new RezolveContext()
      {
        Container = Container,
        RequestedType = requestedType,
        Scope = Scope
      };
    }

    public RezolveContext CreateNew(IContainer container, Type requestedType)
    {
      return new RezolveContext()
      {
        Container = container,
        RequestedType = requestedType,
        Scope = Scope
      };
    }

    public RezolveContext CreateNew(Type requestedType, IScopedContainer scope)
    {
      return new RezolveContext()
      {
        Container = Container,
        RequestedType = requestedType,
        Scope = scope
      };
    }

    public RezolveContext CreateNew(IContainer container, Type requestedType, IScopedContainer scope)
    {
      return new RezolveContext()
      {
        Container = container,
        RequestedType = requestedType,
        Scope = scope
      };
    }

    public RezolveContext CreateNew(IContainer container)
    {
      return new RezolveContext()
      {
        Container = container,
        RequestedType = RequestedType,
        Scope = Scope
      };
    }

    public RezolveContext CreateNew(IScopedContainer scope)
    {
      return new RezolveContext()
      {
        Container = Container,
        RequestedType = RequestedType,
        Scope = scope
      };
    }

    public RezolveContext CreateNew(IContainer container, IScopedContainer scope)
    {
      return new RezolveContext()
      {
        Container = container ?? Container, //can't have a null container
        RequestedType = RequestedType,
        Scope = scope
      };
    }
  }
}
