// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rezolver.Compilation;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Represents a binding between a method parameter and an <see cref="ITarget"/>
    /// </summary>
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
        /// Fetch the target that would be bound to this parameter given the passed <see cref="ICompileContext"/>
        /// </summary>
        /// <param name="context">The current compile context - a new one is created for the <see cref="Parameter"/> type</param>
        /// <returns>The target that should be used for the parameter, or null if no target could be found.
        ///
        /// Note that if the returned target's <see cref="ITarget.UseFallback"/> property is set to <c>true</c>,
        /// then it means either the parameter's default value is being used, or that the target fetched from the
        /// target container in the context is a stub (e.g. empty enumerable)</returns>
        /// <remarks>During compilation - you should not use the target returned by this function as a direct
        /// part of your expression tree - you should </remarks>
        public virtual ITarget Resolve(ICompileContext context)
        {
            if (Target is ResolvedTarget rezolvedTarget)
            {
                return rezolvedTarget.Bind(context.NewContext(Parameter.ParameterType));
            }

            return Target;
        }

        /// <summary>
        /// Creates parameter bindings for each parameter in the passed method where each value will be resolved.
        ///
        /// For any optional parameters - their default values will be used as a fallback if the <see cref="ResolvedTarget"/>
        /// cannot either resolve a target at compile time or from the <see cref="IContainer"/> at resolve-time.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static ParameterBinding[] BindWithRezolvedArguments(MethodBase method)
        {
            // TODO: allow some bindings to be passed up front, so that they can be used instead of the default resolved binding; falling back to a resolved binding if not provided.
            if(method == null) throw new ArgumentNullException(nameof(method));
            var parameters = method.GetParameters();
            return parameters.Select(pi =>
              new ParameterBinding(pi, BindRezolvedArgument(pi))).ToArray();
        }

        private static ResolvedTarget BindRezolvedArgument(ParameterInfo pi)
        {
            return new ResolvedTarget(pi.ParameterType, pi.IsOptional ? new OptionalParameterTarget(pi) : null);
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
            if(methods == null) throw new ArgumentNullException(nameof(methods));
            if (methods.Length == 0)
            {
                throw new ArgumentException("The methods array must contain at least one item", "methods");
            }

            // notice that the list is ordered most-greedy first.  This means that given a choice between two methods where the only difference
            // is a bunch of optional arguments in one, then that one will win out.
            var candidates = methods.Select(m =>
            {
                return new { method = m, bindings = BindMethod(m, args) };
            }).OrderByDescending(mp => mp.bindings.Length).ToArray();

            if (candidates.Length == 0)
            {
                throw new InvalidOperationException("None of the methods could be bound to the supplied arguments");
            }

            // return the first match
            resolvedMethod = candidates[0].method;
            return candidates[0].bindings;
        }

        /// <summary>
        /// Matches named targets in <paramref name="args"/> to parameters on the passed <paramref name="method"/>,
        /// creating default <see cref="ParameterBinding"/>s (which will be resolved from the compile or run-time container),
        /// for any parameters for which named targets cannot be found.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ParameterBinding[] BindMethod(MethodBase method, IDictionary<string, ITarget> args)
        {
            // TODO: Create logic in here to provide the option only to bind the method if each of the parameters (continued)...
            // in the method has a matching named entry in the args dictionary AND that there are no additional
            // args NOT matched by parameters.  In this case, the method would return null.
            return method.GetParameters().Select(p => new ParameterBinding(p, GetNamedArgTarget(p, args))).ToArray();
        }

        private static ITarget GetNamedArgTarget(ParameterInfo p, IDictionary<string, ITarget> args)
        {
            ITarget argValue = null;
            if (args.TryGetValue(p.Name, out argValue))
            {
                return argValue;
            }

            return null;
        }

        /// <summary>
        /// Binds the method using explicit bindings for each parameter supplied in the <paramref name="suppliedBindings"/> array,
        /// or defaults (which will be resolved from the compile or run-time container) if not present.
        /// </summary>
        /// <param name="method">The method to be bound</param>
        /// <param name="suppliedBindings">Optional.  The supplied bindings for the parameters of the method.  Any parameters
        /// not matched from this array will be automatically bound with default (resolved from the container).</param>
        public static ParameterBinding[] BindMethod(MethodBase method, ParameterBinding[] suppliedBindings)
        {
            suppliedBindings = suppliedBindings ?? new ParameterBinding[0];
            return method.GetParameters().Select(p => suppliedBindings.FirstOrDefault(pb => pb.Parameter == p) ?? new ParameterBinding(p)).ToArray();
        }
    }
}