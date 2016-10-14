// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
  /// <summary>
  /// A target that binds to a type's constructor with zero or more arguments supplied by other <see cref="ITarget"/>s and, optionally
  /// binding to the new instance's writeable properties.
  /// </summary>
  /// <remarks>Although you can create this target directly through the 
  /// <see cref="ConstructorTarget.ConstructorTarget(Type, ConstructorInfo, IPropertyBindingBehaviour, ParameterBinding[])"/> constructor,
  /// you're more likely to create it through factory methods such as <see cref="Auto{T}(IPropertyBindingBehaviour)"/> or, more likely still,
  /// extension methods such as <see cref="ITargetContainerExtensions.RegisterType{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/> during
  /// your application's container setup phase.
  /// 
  /// The expression built by this class' implementation of <see cref="CreateExpressionBase(CompileContext)"/> will be an expression tree that ultimately
  /// creates a new instance of the <see cref="DeclaredType"/>.</remarks>
  public class ConstructorTarget : TargetBase
  {
    //note - neither of these two classes inherit from TargetBase because that class performs lots 
    //of unnecessary checks that don't apply to these because we always create boundconstructortargets from 
    //within the ConstructorTarget's CreateExpressionBase - so they effectively inherit it's TargetBase
    private class BoundConstructorTarget : ITarget
    {
      public Type DeclaredType
      {
        get
        {
          return _ctor.DeclaringType;
        }
      }

      public bool UseFallback
      {
        get
        {
          return false;
        }
      }

      private readonly ConstructorInfo _ctor;
      private readonly ParameterBinding[] _boundArgs;

      public BoundConstructorTarget(ConstructorInfo ctor, params ParameterBinding[] boundArgs)
      {
        _ctor = ctor;
        _boundArgs = boundArgs ?? ParameterBinding.None;
      }

      public Expression CreateExpression(CompileContext context)
      {
        return Expression.New(_ctor, _boundArgs.Select(a => a.CreateExpression(context)));
      }

      public bool SupportsType(Type type)
      {
        return TypeHelpers.AreCompatible(DeclaredType, type);
      }
    }

    private class MemberInitialiserTarget : ITarget
    {
      PropertyOrFieldBinding[] _propertyBindings;
      BoundConstructorTarget _nestedTarget;

      /// <summary>
      /// Please note that with this constructor, no checking is performed that the property bindings are
      /// actually valid for the target's type.
      /// </summary>
      /// <param name="target"></param>
      /// <param name="propertyBindings"></param>
      internal MemberInitialiserTarget(BoundConstructorTarget target, PropertyOrFieldBinding[] propertyBindings)
        : base()
      {
        _nestedTarget = target;
        _propertyBindings = propertyBindings ?? PropertyOrFieldBinding.None;
      }

      public bool SupportsType(Type type)
      {
        return _nestedTarget.SupportsType(type);
      }

      public Expression CreateExpression(CompileContext context)
      {
        if (_propertyBindings.Length == 0)
          return _nestedTarget.CreateExpression(context);
        else
        {
          var nestedExpression = _nestedTarget.CreateExpression(context.New(_nestedTarget.DeclaredType));
          //have to locate the NewExpression constructed by the inner target and then rewrite it as
          //a MemberInitExpression with the given property bindings.  Note - if the expression created
          //by the ConstructorTarget is surrounded with any non-standard funny stuff - i.e. anything that
          //could require a NewExpression, then this code won't work.  Points to the possibility that we
          //might need some additional funkiness to allow code such as this to do its thing.
          return new NewExpressionMemberInitRewriter(null,
              _propertyBindings.Select(b => b.CreateMemberBinding(context.New(b.MemberType)))).Visit(nestedExpression);
        }
      }

      public Type DeclaredType
      {
        get { return _nestedTarget.DeclaredType; }
      }

      public bool UseFallback
      {
        get
        {
          return _nestedTarget.UseFallback;
        }
      }
    }

    private readonly Type _declaredType;
    private readonly ConstructorInfo _ctor;
    private readonly ParameterBinding[] _parameterBindings;
    private IPropertyBindingBehaviour _propertyBindingBehaviour;
    /// <summary>
    /// used when named parameter bindings are supplied on construction via the static factory methods, but without 
    /// a specific constructor
    /// 
    /// The class will perform a late-bound lookup through each of the possible bindings to find the best before
    /// creating the expression.
    /// </summary>
    private readonly IDictionary<string, ITarget> _suppliedArgs;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorTarget"/> class.
    /// </summary>
    /// <param name="type">Required.  The type to be constructed when resolved in a container.</param>
    /// <param name="ctor">Optional.  The specific constructor to be bound on the <paramref name="type"/></param>
    /// <param name="propertyBindingBehaviour">Optional.  If provided, then this is used to extend the expression returned by <see cref="CreateExpressionBase(CompileContext)"/>
    /// into a <see cref="MemberInitExpression"/> with one or more writeable properties also being bound to the runtime <see cref="IContainer"/>.</param>
    /// <param name="parameterBindings">Optional, although can only be supplied if <paramref name="ctor"/> is provided.  
    /// Specific bindings for the parameters of the given <paramref name="ctor"/> which should be used during code generation.</param>
    /// <remarks>
    /// If you do not pass an argument to the <paramref name="ctor"/> parameter, then best available constructor on the <paramref name="type"/> is determined
    /// when <see cref="ITarget.CreateExpression(CompileContext)"/> is called (which ultimately calls <see cref="CreateExpressionBase(CompileContext)"/>).
    /// 
    /// The best available constructor is defined as the constructor with the most parameters for which arguments can be resolved from the <see cref="CompileContext"/> at compile-time
    /// (i.e. when <see cref="CreateExpressionBase(CompileContext)"/> is called) to the fewest number of <see cref="ITarget"/> objects whose <see cref="ITarget.UseFallback"/> 
    /// is false (for example - when an IEnumerable of a service is requested, but no registrations are found, a target is returned with <see cref="ITarget.UseFallback"/> set to 
    /// <c>true</c>, and whose expression will equate to an empty enumerable).
    /// 
    /// This allows the system to bind to different constructors automatically based on the other registrations that are present in the <see cref="ITargetContainer"/> of the active
    /// <see cref="RezolveContext.Container"/> when code is compiled in response to a call to <see cref="IContainer.Resolve(RezolveContext)"/>.
    /// </remarks>
    public ConstructorTarget(Type type, ConstructorInfo ctor, IPropertyBindingBehaviour propertyBindingBehaviour, ParameterBinding[] parameterBindings)
    {
      type.MustNotBeNull(nameof(type));
      _declaredType = type;
      _ctor = ctor;
      _parameterBindings = parameterBindings ?? ParameterBinding.None;
      _propertyBindingBehaviour = propertyBindingBehaviour;
      _suppliedArgs = new Dictionary<string, ITarget>();
    }

    private ConstructorTarget(Type type, ConstructorInfo ctor, IPropertyBindingBehaviour propertyBindingBehaviour, IDictionary<string, ITarget> suppliedArgs)
    {
      _declaredType = type;
      _ctor = ctor;
      _parameterBindings = ParameterBinding.None;
      _propertyBindingBehaviour = propertyBindingBehaviour;
      _suppliedArgs = suppliedArgs;
    }

    /// <summary>
    /// Implementation of <see cref="TargetBase.CreateExpressionBase(CompileContext)"/>
    /// </summary>
    /// <param name="context">The current compile context</param>
    /// <exception cref="AmbiguousMatchException">If no definitively 'best' constructor can be determined.</exception>
    protected override Expression CreateExpressionBase(CompileContext context)
    {
      ConstructorInfo ctor = _ctor;
      ParameterBinding[] boundArgs = ParameterBinding.None;

      if (ctor == null)
      {
        //have to go searching for the best constructor match for the current context,
        //which will also give us our arguments
        var publicCtorGroups = GetPublicConstructorGroups(DeclaredType);
        //var possibleBindingsGrouped = publicCtorGroups.Select(g => g.Select(ci => new BoundConstructorTarget(ci, ParameterBinding.BindMethod(ci))));
        var ctorsWithBindingsGrouped = publicCtorGroups.Select(g =>
          g.Select(ci => new
          {
            ctor = ci,
            //filtered collection of parameter bindings along with the actual ITarget that is resolved for each
            //NOTE: we're using the default behaviour of ParameterBinding here which is to auto-resolve an argument
            //value or to use the parameter's default if it is optional.
            bindings = ParameterBinding.BindMethod(ci, _suppliedArgs)// ci.GetParameters().Select(pi => new ParameterBinding(pi))
              .Select(pb => new { Parameter = pb, RezolvedArg = pb.Resolve(context) })
              .Where(bp => bp.RezolvedArg != null).ToArray()
            //(ABOVE) only include bindings where a target was found - means we can quickly
            //determine if all parameters are bound by checking the array length is equal to the
            //number of parameters on the constructor itself (BELOW)
          }).Where(a => a.bindings.Length == g.Key).ToArray()
        ).Where(a => a.Length > 0).ToArray(); //filter to where there is at least one successfully bound constructor

        //No constructors for which we could bind all parameters with either a mix of resolved or default arguments.
        //so we'll auto-bind to the constructor with the most parameters - if there is one - leaving the application
        //with the responsibility of ensuring that the correct registrations are made in the target container, or 
        //in the container supplied at resolve-time, to satisfy the constructor's dependencies.
        if (ctorsWithBindingsGrouped.Length == 0)
        {
          if (publicCtorGroups.Length != 0)
          {
            var mostGreedy = publicCtorGroups[0].ToArray();
            if (mostGreedy.Length > 1)
              throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneConstructorFormat, DeclaredType, string.Join(", ", mostGreedy.AsEnumerable())));
            else
            {
              ctor = mostGreedy[0];
              boundArgs = ParameterBinding.BindWithRezolvedArguments(ctor).ToArray();
            }
          }
          else
            throw new InvalidOperationException(string.Format(ExceptionResources.NoApplicableConstructorForContextFormat, _declaredType));
        }
        else //managed to bind at least constructor up front to registered targets or defaults
        {
          //get the greediest constructors with successfully bound parameters.
          var mostBound = ctorsWithBindingsGrouped[0];
          //get the first result
          var toBind = mostBound[0];
          //if there is only one, then we can move on to code generation
          if (mostBound.Length > 1)
          {
            //the question now is one of disambiguation: 
            //choose the one with the fewest number of targets with ITarget.UseFallback set to true
            //if we still can't disambiguate, then we have an exception.
            var fewestFallback = mostBound.GroupBy(a => a.bindings.Count(b => b.RezolvedArg.UseFallback)).FirstOrDefault().ToArray();
            if (fewestFallback.Length > 1)
              throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneBestConstructorFormat, DeclaredType, string.Join(", ", fewestFallback.Select(a => a.ctor))));
            toBind = fewestFallback[0];
          }
          ctor = toBind.ctor;
          boundArgs = toBind.bindings.Select(a => a.Parameter).ToArray();
        }
      }
      //we allow for no parameter bindings to be provided on construction, and have them dynamically determined
      else if (_parameterBindings.Length == 0 && ctor.GetParameters().Length != 0)
      {
        //just need to generate the bound parameters - nice and easy
        //because the constructor was provided up-front, we don't check whether the target can be resolved
        boundArgs = ParameterBinding.BindMethod(ctor, _suppliedArgs);// ParameterBinding.BindWithRezolvedArguments(ctor);
      }
      else
        boundArgs = _parameterBindings;

      BoundConstructorTarget baseTarget = new BoundConstructorTarget(ctor, boundArgs);
      ITarget target = null;
      if (_propertyBindingBehaviour != null)
        target = new MemberInitialiserTarget(baseTarget, _propertyBindingBehaviour.GetPropertyBindings(context, _declaredType));
      else
        target = baseTarget;
      return target.CreateExpression(context);
    }

    /// <summary>
    /// Implementation of <see cref="TargetBase.DeclaredType"/>.  Always returns the type passed into the 
    /// <see cref="ConstructorTarget.ConstructorTarget(Type, ConstructorInfo, IPropertyBindingBehaviour, ParameterBinding[])"/> constructor
    /// </summary>
    public override Type DeclaredType
    {
      get { return _declaredType; }
    }

    /// <summary>
    /// Generic version of the <see cref="Auto(Type, IPropertyBindingBehaviour)"/> method.
    /// </summary>
    /// <typeparam name="T">The type that is to be constructed when the new target is compiled and executed.</typeparam>
    /// <param name="propertyBindingBehaviour">See the documentation for the <paramref name="propertyBindingBehaviour"/> parameter
    /// on the non-generic version of this method.</param>
    /// <returns>A new <see cref="ITarget"/> that is (likely) to be either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> if
    /// <paramref name="type"/> is a generic type definition.</returns>
    public static ITarget Auto<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      return Auto(typeof(T), propertyBindingBehaviour);
    }

    /// <summary>
    /// Shortcut for calling the <see cref="ConstructorTarget.ConstructorTarget(Type, ConstructorInfo, IPropertyBindingBehaviour, ParameterBinding[])"/>
    /// with only the <paramref name="type"/> and <paramref name="propertyBindingBehaviour"/> arguments supplied.
    /// </summary>
    /// <param name="type">The type that is to be constructed when this target is compiled and executed.</param>
    /// <param name="propertyBindingBehaviour">Optional.  An object which selects properties on the new instance which are
    /// to be bound the container.</param>
    /// <returns>A new <see cref="ITarget"/> that is (likely) to be either a <see cref="ConstructorTarget"/> (or <see cref="GenericConstructorTarget"/> if
    /// <paramref name="type"/> is a generic type definition).</returns>
    public static ITarget Auto(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      //conduct a very simple search for the constructor with the most parameters
      type.MustNotBeNull(nameof(type));
      type.MustNot(t => TypeHelpers.IsInterface(t), "Type must not be an interface", nameof(type));
      type.MustNot(t => TypeHelpers.IsAbstract(t), "Type must not be abstract", nameof(type));

      if (TypeHelpers.IsGenericTypeDefinition(type))
        return new GenericConstructorTarget(type, propertyBindingBehaviour);

      return new ConstructorTarget(type, null, propertyBindingBehaviour, (ParameterBinding[])null);
    }

    private static IGrouping<int, ConstructorInfo>[] GetPublicConstructorGroups(Type declaredType)
    {
      var ctorGroups = TypeHelpers.GetConstructors(declaredType)
              .GroupBy(c => c.GetParameters().Length)
              .OrderByDescending(g => g.Key).ToArray();

      if (ctorGroups.Length == 0)
        throw new ArgumentException(
          string.Format(ExceptionResources.NoPublicConstructorsDefinedFormat, declaredType), "declaredType");
      return ctorGroups;
    }

    /// <summary>
    /// Creates a <see cref="ConstructorTarget"/> with a set of named targets which will be used to 
    /// bind the constructor when the target's <see cref="ITarget.CreateExpression(CompileContext)"/>
    /// is called.
    /// </summary>
    /// <typeparam name="T">The type whose constructor is to be bound</typeparam>
    /// <param name="args">The named arguments to be used when building the expression.</param>
    /// <returns>ITarget.</returns>
    /// <remarks>Both versions of this method will create a target which will try to find the best-matching
    /// constructor where all of the named arguments match, and with the fewest number of auto-resolved
    /// arguments.
    /// 
    /// So, a class with a constructor such as 
    /// 
    /// <code>Foo(IService1 s1, IService2 s2)</code>
    /// 
    /// Can happily be bound if you only provide a named argument for 's1'; the target will simply
    /// attempt to auto-resolve the argument for the <code>IService2 s2</code> parameter when constructing 
    /// the object - and will fail only if it can't be resolved at that point.
    /// </remarks>
    public static ITarget WithArgs<T>(IDictionary<string, ITarget> args)
    {
      args.MustNotBeNull("args");

      return WithArgsInternal(typeof(T), args);
    }

    /// <summary>
    /// Non-generic version of <see cref="WithArgs{T}(IDictionary{string, ITarget})"/>.
    /// </summary>
    /// <param name="declaredType">The type whose constructor is to be bound</param>
    /// <param name="args">The named arguments to be used when building the expression.</param>
    /// <returns>ITarget.</returns>
    public static ITarget WithArgs(Type declaredType, IDictionary<string, ITarget> args)
    {
      declaredType.MustNotBeNull("declaredType");
      args.MustNotBeNull("args");

      return WithArgsInternal(declaredType, args);
    }

    /// <summary>
    /// Similar to <see cref="WithArgs(Type, IDictionary{string, ITarget})"/> except this one creates
    /// a <see cref="ConstructorTarget"/> which is specifically bound to a particular constructor on a 
    /// given type, using any matched argument bindings from the provided <paramref name="args" /> dictionary,
    /// and resolving any that are not matched.
    /// </summary>
    /// <param name="declaredType">Type of the object to be constructed, or the type .</param>
    /// <param name="ctor">The ctor.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>ITarget.</returns>
    public static ITarget WithArgs(Type declaredType, ConstructorInfo ctor, IDictionary<string, ITarget> args)
    {
      declaredType.MustNotBeNull("declaredType");
      ctor.MustNotBeNull("ctor");
      args.MustNotBeNull("args");

      ParameterBinding[] bindings = ParameterBinding.BindMethod(ctor, args);

      return new ConstructorTarget(declaredType, ctor, null, bindings);
    }

    internal static ITarget WithArgsInternal(Type declaredType, IDictionary<string, ITarget> args)
    {
      MethodBase ctor = null;
      //var bindings = ParameterBinding.BindOverload(TypeHelpers.GetConstructors(declaredType), args, out ctor);

      return new ConstructorTarget(declaredType, (ConstructorInfo)ctor, null, args);
    }

    /// <summary>
    /// Creates a new <see cref="ConstructorTarget" /> from the passed lambda expression (whose <see cref="LambdaExpression.Body" /> must be a <see cref="NewExpression" />)
    /// </summary>
    /// <typeparam name="T">The type of the object to be created by the new <see cref="ConstructorTarget" /></typeparam>
    /// <param name="newExpr">Required.  The expression from which to create the target.</param>
    /// <param name="adapter">Optional.  The adapter to be used to convert any additional expressions in the lambda into <see cref="ITarget" /> instances (e.g. for argument values).
    /// If not provided, then the <see cref="TargetAdapter.Default" /> will be used.</param>
    /// <returns>An <see cref="ITarget" /> that actually be an intstance of <see cref="ConstructorTarget"/></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="newExpr"/> is null.</exception>
    /// <exception cref="ArgumentException">If the <paramref name="newExpr"/> does not have a NewExpression as its root (Body) node, or if the type of
    /// that expression does not equal <typeparamref name="T"/>
    /// </exception>
    /// <remarks>This method does not support member binding expressions - e.g. <code>c => new MyObject() { A = "hello" }</code> - these can be converted into
    /// targets using a (compliant) <see cref="ITargetAdapter"/> object's <see cref="ITargetAdapter.CreateTarget(Expression)"/> method.  At least, the supplied
    /// <see cref="TargetAdapter"/> class does support them.
    /// 
    /// When providing custom expressions to be used as targets in an <see cref="ITargetContainer"/>, it is possible to explicitly define properties/arguments as
    /// being resolved from the container itself, in exactly the same way as generated by the other factory methods such as <see cref="Auto{T}(IPropertyBindingBehaviour)"/>
    /// and <see cref="ITargetContainerExtensions.RegisterType{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/>.  To do this, simply call the 
    /// <see cref="RezolveContextExpressionHelper.Resolve{T}"/> function on the object passed into your expression (see the signature of the lambda <paramref name="newExpr"/>),
    /// and Rezolver will convert that call into a <see cref="RezolvedTarget"/>.
    /// </remarks>
    public static ITarget FromNewExpression<T>(Expression<Func<RezolveContextExpressionHelper, T>> newExpr, ITargetAdapter adapter = null)
    {
      newExpr.MustNotBeNull(nameof(newExpr));
      NewExpression newExprBody = null;

      newExprBody = newExpr.Body as NewExpression;
      if (newExprBody == null)
        throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyIsNotNewExpressionFormat, newExpr), nameof(newExpr));
      else if (newExprBody.Type != typeof(T))
        throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyNewExpressionIsWrongTypeFormat, newExpr, typeof(T)), nameof(newExpr));

      return FromNewExpression(typeof(T), newExprBody, adapter);
    }

    /// <summary>
    /// Non-generic version of <see cref="FromNewExpression{T}(Expression{Func{RezolveContextExpressionHelper, T}}, ITargetAdapter)"/>.  See the documentation
    /// on that method for more.
    /// </summary>
    /// <param name="declaredType">The of the object to be created by the new <see cref="ConstructorTarget"/></param>
    /// <param name="newExpr"></param>
    /// <param name="adapter"></param>
    /// <returns></returns>
    public static ITarget FromNewExpression(Type declaredType, NewExpression newExpr, ITargetAdapter adapter = null)
    {
      ConstructorInfo ctor = null;
      ParameterBinding[] parameterBindings = null;

      ctor = newExpr.Constructor;
      parameterBindings = ExtractParameterBindings(newExpr, adapter ?? TargetAdapter.Default).ToArray();

      return new ConstructorTarget(declaredType, ctor, null, parameterBindings);
    }

    private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr, ITargetAdapter adapter)
    {
      return newExpr.Constructor.GetParameters()
        .Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, adapter.CreateTarget(expression))).ToArray();
    }
  }
}