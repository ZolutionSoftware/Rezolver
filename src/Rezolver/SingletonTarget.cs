// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Rezolver
{
  /// <summary>
  /// Decorates any IRezolveTarget instance inside a lazily-initialised instance which will only ever
  /// create the target object once.
  /// </summary>
  public class SingletonTarget : TargetBase
  {
    /// <summary>
    /// Used in ensuring the correct construction and scope tracking of a singleton instance.
    /// 
    /// Implements the ICompiledRezolveTarget interface for the single case of creating a lazy on demand (from the
    /// first RezolveContext that's passed to its GetObject method, and returning its value.
    /// </summary>
    private class SingletonTargetLazyInitialiser : ICompiledTarget
    {
      private Func<RezolveContext, Lazy<object>> _lazyFactory;
      private Lazy<object> _lazy;

      //internal Lazy<object> Lazy {  get { return _lazy; } }

      internal SingletonTargetLazyInitialiser(SingletonTarget target, Func<RezolveContext, Lazy<object>> lazyFactory)
      {
        _lazyFactory = lazyFactory;
      }

      /// <summary>
      /// Creates the singleton target's Lazy on demand if required, and then returns the object that the underlying
      /// compiled code produces.
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      object ICompiledTarget.GetObject(RezolveContext context)
      {
        if (_lazy == null)
        {
          //here - if we create more than one lazy, it's not a big deal.  *Executing* two
          //different ones is a big deal.
          Interlocked.CompareExchange(ref _lazy, _lazyFactory(context), null);
        }
        return _lazy.Value;
      }
    }

    private static readonly MethodInfo ICompiledRezolveTarget_GetObject = MethodCallExtractor.ExtractCalledMethod((ICompiledTarget t) => t.GetObject(null));
    private static readonly ConstructorInfo LazyObject_Ctor = MethodCallExtractor.ExtractConstructorCall(() => new Lazy<object>(() => (object)null));

    private ITarget _innerTarget;
    private readonly ConcurrentDictionary<Type, SingletonTargetLazyInitialiser> _initialisers = new ConcurrentDictionary<Type, SingletonTargetLazyInitialiser>();

    /// <summary>
    /// Overrides the base class to ensure that automatic generation of the scope tracking code by RezolveTargetBase is disabled.
    /// 
    /// For the singleton, it's important that the scope tracking call occurs within the lazy's callback.
    /// </summary>
    protected override bool SuppressScopeTracking
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Constructs a new instance of the <see cref="SingletonTarget"/> class.
    /// </summary>
    /// <param name="innerTarget">The target whose result (when compiled) is to be used as the singleton instance.</param>
    public SingletonTarget(ITarget innerTarget)
    {
      innerTarget.MustNotBeNull("innerTarget");
      _innerTarget = innerTarget;
    }

    protected override Expression CreateScopeSelectionExpression(CompileContext context, Expression expression)
    {
      return ExpressionHelper.Make_Scope_GetScopeRootCallExpression(context);
    }

    protected override Expression CreateExpressionBase(CompileContext context)
    {
      //this isn't quite right, imho.  One singleton target should technically be able to create
      //more than one instance of a given concrete type, if that target is in a RezolverBuilder that is, 
      //in turn, used to build more than one top-level container (a permitted use case).
      //The way this code is written, however, the singleton will only ever create one instance of a type and,
      //in a cruel twist, will only ever belong to one scope (if applicable).
      //to fix this will mean being able to easily identify different container trees from each other,
      //and creating one instance for each unique tree.  This isn't even possible at the moment, because
      //there's no way to identify the root container in a chain of containers, unless you force the use
      //of scopes, which we don't necessarily want to do.
      //So I'm not sure if I've coded myself down a hole here - or if adding the necessary properties to 
      //the IRezolver interface to uniquely identify each one, and walk any parent/child relationship from 
      //child back to root would actually be a good thing to do in general anyway, so should therefore just
      //be done.

      SingletonTargetLazyInitialiser initialiser = null;

      if (!_initialisers.TryGetValue(context.TargetType ?? _innerTarget.DeclaredType, out initialiser))
      {
        //if our inner target is also a singleton, we just chain straight through to its own CreateExpression
        //call.  Note that if this is the case, our own lazy will never be constructed and the code will always
        //fall into this branch.
        if (_innerTarget is SingletonTarget)
          return _innerTarget.CreateExpression(context);

        initialiser = _initialisers.GetOrAdd(context.TargetType ?? _innerTarget.DeclaredType, t =>
        {
                  //get the underlying expression for the target that is to be turned into a singleton - but disable the
                  //generation of any scope-tracking code.
                  var innerExpression = ExpressionHelper.GetLambdaBodyForTarget(_innerTarget,
                      context.New(t, suppressScopeTracking: true));
                  //generate our scope tracking expression.
                  var scopeTracking = CreateScopeTrackingExpression(context, innerExpression);

                  //there's an argument for using the compiler from the compilecontext (via the Rezolver) here to generate this 
                  //lambda, but our interface specifically creates an ICompiledRezolveTarget - not a delegate - so it's not 
                  //really suitable.
                  //in any case, compiling directly to a delegate will work on all platforms on which Rezolver will work in the 
                  //first place, it just might not be as fast as it might be if we honoured the context's compiler, if
                  //that happens to be the AssemblyRezolveTargetCompiler.
                  var lazyLambdaExpr = Expression.Lambda(Expression.New(LazyObject_Ctor,
                      //creating the factory delegate for the lazy uniquely for each invokation of the outer lambda
                      Expression.Lambda(Expression.Convert(scopeTracking, typeof(object)))), context.RezolveContextExpression);
          var lazyLambda = (Func<RezolveContext, Lazy<object>>)lazyLambdaExpr.Compile();
                  //now we create and capture an instance of the SingletonTargetLazyInitialiser class, passing our
                  //dynamically constructed delegate along with this target
                  return new SingletonTargetLazyInitialiser(this, lazyLambda);
                  //return Expression.Call(Expression.Constant(new SingletonTargetLazyInitialiser(this, lazyLambda), typeof(ICompiledRezolveTarget)),
                  //    ICompiledRezolveTarget_GetObject, context.RezolveContextParameter);
                });
      }
      return Expression.Call(Expression.Constant(initialiser, typeof(ICompiledTarget)),
             ICompiledRezolveTarget_GetObject, context.RezolveContextExpression);
      //return Expression.Property(Expression.Constant(initialiser.Lazy), "Value");
    }

    public override bool SupportsType(Type type)
    {
      return _innerTarget.SupportsType(type);
    }

    public override Type DeclaredType
    {
      get { return _innerTarget.DeclaredType; }
    }
  }

  /// <summary>
  /// Extension method(s) to convert targets into singleton targets.
  /// </summary>
  public static class IRezolveTargetSingletonExtensions
  {
    /// <summary>
    /// Constructs a <see cref="SingletonTarget"/> that wraps the target on which the method is invoked.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static SingletonTarget Singleton(this ITarget target)
    {
      target.MustNotBeNull(nameof(target));
      return new SingletonTarget(target);
    }
  }
}
