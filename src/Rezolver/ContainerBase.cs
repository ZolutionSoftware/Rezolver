// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// Starting point for implementations of <see cref="IContainer"/> - only creatable through inheritance.
  /// </summary>
  /// <remarks>Note that the class also implements <see cref="ITargetContainer"/> by proxying the <see cref="Targets"/> that are
  /// provided to it on construction.  All methods are implemented explicitly except the <see cref="Register(ITarget, Type)"/> method, 
  /// which available through the class' public API.
  /// 
  /// Note: <see cref="IContainer"/>s are generally not expected to implement <see cref="ITargetContainer"/>, and the framework
  /// will never assume they do.
  /// 
  /// The reason this class does is to make it easier to create a new container and to register targets into it without having to worry about
  /// managing a separate <see cref="ITargetContainer"/> instance in your application root - because all the registration extension methods defined
  /// in <see cref="ITargetContainerExtensions"/> will be available to developers in code which has a reference to this class, or one derived from it.
  /// 
  /// Note also that calling <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> on an instance of this type will always
  /// cause a <see cref="NotSupportedException"/> to be thrown.
  /// </remarks>
  public class ContainerBase : IContainer, ITargetContainer
  {
    private static readonly
      ConcurrentDictionary<Type, Lazy<ICompiledTarget>> MissingTargets = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();

    protected static ICompiledTarget GetMissingTarget(Type target)
    {
      return MissingTargets.GetOrAdd(target, t => new Lazy<ICompiledTarget>(() => new MissingCompiledTarget(t))).Value;
    }

    protected static bool IsMissingTarget(ICompiledTarget target)
    {
      return target is MissingCompiledTarget;
    }

    protected class MissingCompiledTarget : ICompiledTarget
    {
      private readonly Type _type;

      public MissingCompiledTarget(Type type)
      {
        _type = type;
      }

      public object GetObject(RezolveContext context)
      {
        throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
      }
    }

    protected ContainerBase(ITargetContainer targets = null, ITargetCompiler compiler = null)
    {
      Targets = targets ?? new TargetContainer();
      Compiler = compiler ?? TargetCompiler.Default;

    }

    /// <summary>
    /// The compiler that will be used to compile <see cref="ITarget"/> instances, obtained from the <see cref="Targets"/> container
    /// during <see cref="Resolve(RezolveContext)"/> and <see cref="TryResolve(RezolveContext, out object)"/> operations, into 
    /// <see cref="ICompiledTarget"/> instances that will actually provide the objects that are resolved.
    /// </summary>
    /// <remarks>Notes to implementers: This property must NEVER be null.</remarks>
    protected ITargetCompiler Compiler { get; }

    /// <summary>
    /// Provides the <see cref="ITarget"/> instances that will be compiled by the <see cref="Compiler"/> into <see cref="ICompiledTarget"/>
    /// instances.
    /// </summary>
    /// <remarks>Notes to implementers: This property must NEVER be null.</remarks>
    protected ITargetContainer Targets { get; }

    public virtual object Resolve(RezolveContext context)
    {
      return GetCompiledRezolveTarget(context).GetObject(context);
    }

    public virtual bool TryResolve(RezolveContext context, out object result)
    {
      var target = GetCompiledRezolveTarget(context);
      if (!IsMissingTarget(target))
      {
        result = target.GetObject(context);
        return true;
      }
      else
      {
        result = null;
        return false;
      }
    }

    public virtual IScopedContainer CreateLifetimeScope()
    {
      //optimistic implementation of this method - attempts a safe cast to ILifetimeScopeRezolver of itself
      //so that types derived from this class that are ILifetimeScopeRezolver instances do not need
      //to reimplement this method.
      return new OverridingScopedContainer(this as IScopedContainer, this);
    }

    public virtual ICompiledTarget FetchCompiled(RezolveContext context)
    {
      //note that this container is fixed as the container in the compile context - regardless of the
      //one passed in.  This is important.
      //note also that this container is only passed as scope if the context doesn't already have one.
      return GetCompiledRezolveTarget(context.CreateNew(this, context.Scope ?? (this as IScopedContainer)));
    }

    public virtual bool CanResolve(RezolveContext context)
    {
      //TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
      return Targets.Fetch(context.RequestedType) != null;
    }

    protected virtual ICompiledTarget GetCompiledRezolveTarget(RezolveContext context)
    {
      ITarget target = Targets.Fetch(context.RequestedType);

      if (target == null)
        return GetFallbackCompiledRezolveTarget(context);

      //if the entry advises us to fall back if possible, then we'll see what we get from the 
      //fallback operation.  If it's NOT the missing target, then we'll use that instead
      if (target.UseFallback)
      {
        var fallback = GetFallbackCompiledRezolveTarget(context);
        if (!IsMissingTarget(fallback))
          return fallback;
      }

      return Compiler.CompileTarget(target,
        new CompileContext(this,
          //the targets we pass here are wrapped in a new ChildBuilder by the context
          Targets,
          context.RequestedType));

    }

    /// <summary>
    /// Called by <see cref="GetCompiledRezolveTarget(RezolveContext)"/> if no valid <see cref="ITarget"/> can be
    /// found for the <paramref name="context"/> or if the one found has its <see cref="ITarget.UseFallback"/> property
    /// set to <c>true</c>.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>An <see cref="ICompiledTarget"/> to be used as the result of a <see cref="Resolve(RezolveContext)"/> operation
    /// where the search for a valid target either fails or is inconclusive (e.g. - empty enumerables).</returns>
    /// <remarks>The base implementation always returns an instance of the <see cref="MissingCompiledTarget"/> via
    /// the <see cref="GetMissingTarget(Type)"/> static method.</remarks>
    protected virtual ICompiledTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
    {
      return GetMissingTarget(context.RequestedType);
    }

    object IServiceProvider.GetService(Type serviceType)
    {
      return GetService(serviceType);
    }

    /// <summary>
    /// Protected virtual implementation of <see cref="IServiceProvider.GetService(Type)"/>.
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    protected virtual object GetService(Type serviceType)
    {
      //IServiceProvider should return null if not found - so we use TryResolve.
      object toReturn = null;
      this.TryResolve(serviceType, out toReturn);
      return toReturn;
    }

    /// <summary>
    /// Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/> - simply proxies the
    /// call to the target container referenced by the <see cref="Targets"/> property.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="serviceType"></param>
    /// <remarks>Remember: registering new targets into an <see cref="ITargetContainer"/> after an <see cref="IContainer"/>
    /// has started compiling targets within it can yield unpredictable results.
    /// 
    /// If you create a new container and perform all your registrations before you use it, however, then everything will 
    /// work as expected.
    /// 
    /// Note also the other ITargetContainer interface methods are implemented explicitly so as to hide them from the 
    /// list of class members.
    /// </remarks>
    public void Register(ITarget target, Type serviceType = null)
    {
      Targets.Register(target, serviceType);
    }

    #region ITargetContainer explicit implementation
    ITarget ITargetContainer.Fetch(Type type)
    {
      return Targets.Fetch(type);
    }

    IEnumerable<ITarget> ITargetContainer.FetchAll(Type type)
    {
      return Targets.FetchAll(type);
    }

    ITargetContainer ITargetContainer.CombineWith(ITargetContainer existing, Type type)
    {
      throw new NotSupportedException();
    }
    #endregion
  }
}
