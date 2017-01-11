// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
  public class ParameterBinding
  {
    /// <summary>
    /// Represents an empty parameter bindings array.
    /// </summary>
    public static readonly ParameterBinding[] None = new ParameterBinding[0];

    /// <summary>
    /// The parameter to be bound
    /// </summary>
    public ParameterInfo Parameter { get; private set; }
    /// <summary>
    /// The initial target that was bound to this parameter.
    /// </summary>
    public ITarget Target { get; private set; }

    /// <summary>
    /// Gets a boolean indicating whether the parameter binding is valid
    /// 
    /// Ultimately, this returns true if <see cref="Target"/> is non-null.
    /// </summary>
    public bool IsValid { get { return Target != null; } }

    /// <summary>
    /// Constructs a new instance of the <see cref="ParameterBinding"/> class.
    /// </summary>
    /// <param name="parameter">Required - the parameter being bound</param>
    /// <param name="target">Optional - the argument supplied for the parameter.</param>
    public ParameterBinding(ParameterInfo parameter, ITarget target = null)
    {
      Parameter = parameter;
      Target = target ?? BindRezolvedArgument(parameter);
    }

    /// <summary>
    /// Fetch the target that would be bound to this parameter given the passed <see cref="CompileContext"/>
    /// </summary>
    /// <param name="context">The current compile context - a new one is created for the <see cref="Parameter"/> type</param>
    /// <returns>The target that should be used for the parameter, or null if no target could be found.
    /// 
    /// Note that if the returned target's <see cref="ITarget.UseFallback"/> property is set to <c>true</c>,
    /// then it means either the parameter's default value is being used, or that the target fetched from the 
    /// target container in the context is a stub (e.g. empty enumerable)</returns>
    /// <remarks>During compilation - you should not use the target returned by this function as a direct
    /// part of your expression tree - you should </remarks>
    public virtual ITarget Resolve(CompileContext context)
    {
      if (Target is RezolvedTarget)
        return ((RezolvedTarget)Target).Resolve(context.New(Parameter.ParameterType));

      return Target;
    }

    /// <summary>
    /// This is a compile-time helper method which produces a binding for each parameter where arguments that can be resolved 
    /// from the passed <paramref name="context"/> will be bound with <see cref="RezolvedTarget"/>s, and those which cannot, 
    /// but which are optional, will be bound with <see cref="OptionalParameterTarget"/>s.
    /// 
    /// If the parameter is not optional, and no target can be resolved, then an invalid binding will be returned for that parameter.
    /// </summary>
    /// <param name="method">Required - the method whose parameters are to be bound.</param>
    /// <param name="context">Required - the current compile context which will be used to test whether arguments can be resolved
    /// at compile time.  If they can, then the associated parameter will be bound with a <see cref="RezolvedTarget"/>.</param>
    /// <returns></returns>
    public static ParameterBinding[] BindWithRezolvedOrOptionalDefault(MethodBase method, CompileContext context)
    {
      method.MustNotBeNull(nameof(method));
      context.MustNotBeNull(nameof(context));
      return method.GetParameters().Select(pi => new ParameterBinding(pi)).ToArray();
    }

    /// <summary>
    /// Creates parameter bindings for each parameter in the passed method where each value will be resolved.
    /// 
    /// For any optional parameters - their default values will be used as a fallback.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ParameterBinding[] BindWithRezolvedArguments(MethodBase method)
    {
      method.MustNotBeNull("method");
      var parameters = method.GetParameters();
      return parameters.Select(pi =>
        new ParameterBinding(pi, BindRezolvedArgument(pi))).ToArray();
    }

    private static RezolvedTarget BindRezolvedArgument(ParameterInfo pi)
    {
      return new RezolvedTarget(pi.ParameterType, pi.IsOptional ? new OptionalParameterTarget(pi) : null);
    }

    /// <summary>
    /// Searches for a method in the <paramref name="methods"/> collection whose parameters can be filled by the targets provided in the <paramref name="args"/>
    /// dictionary, returning the parameter bindings, and passing out the resolved target method in <paramref name="resolvedMethod"/> if found.
    /// 
    /// Note - if no match can be found, or if more than one method could be bound, then an InvalidOperationException will occur.
    /// </summary>
    /// <param name="methods">The methods.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="resolvedMethod">The resolved method.</param>
    /// <returns>ParameterBinding[].</returns>
    public static ParameterBinding[] BindOverload(MethodBase[] methods, IDictionary<string, ITarget> args, out MethodBase resolvedMethod)
    {
      resolvedMethod = null;
      methods.MustNotBeNull("methods");
      if (methods.Length == 0) throw new ArgumentException("The methods array must contain at least one item", "methods");

      //notice that the list is ordered most-greedy first.  This means that given a choice between two methods where the only difference
      //is a bunch of optional arguments in one, then that one will win out.
      var candidates = methods.Select(m =>
      {
        return new { method = m, bindings = BindMethod(m, args) };
      }).OrderByDescending(mp => mp.bindings.Length).ToArray();

      if (candidates.Length == 0)
        throw new InvalidOperationException("None of the methods could be bound to the supplied arguments");

      //return the first match
      resolvedMethod = candidates[0].method;
      return candidates[0].bindings;
    }

    /// <summary>
    /// Matches named targets in <paramref name="args"/> to parameters on the passed <paramref name="method"/>,
    /// creating default <see cref="ParameterBinding"/>s for any parameters for which named targets cannot be found.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static ParameterBinding[] BindMethod(MethodBase method, IDictionary<string, ITarget> args)
    {
      //TODO: Create logic in here to provide the option only to bind the method if each of the parameters
      //in the method has a matching named entry in the args dictionary AND that there are no additional
      //args NOT matched by parameters.  In this case, the method would return null.
      return method.GetParameters().Select(p => new ParameterBinding(p, GetNamedArgTarget(p, args))).ToArray();
    }

    private static ITarget GetNamedArgTarget(ParameterInfo p, IDictionary<string, ITarget> args)
    {
      ITarget argValue = null;
      if (args.TryGetValue(p.Name, out argValue))
        return argValue;
      return null;
    }
  }
}