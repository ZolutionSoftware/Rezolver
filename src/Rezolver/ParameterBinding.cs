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
		/// The target that will provide the argument to the parameter when an expression is built.
		/// 
		/// If null, then the parameter is not bound and <see cref="IsValid"/> will be <c>false</c>.
		/// </summary>
		public ITarget Target { get; private set; }

		/// <summary>
		/// Gets a boolean indicating whether the parameter binding is valid
		/// 
		/// Ultimately, this returns true if <see cref="Target"/> is non-null.
		/// </summary>
		public bool IsValid {  get { return Target != null; } }

		/// <summary>
		/// Gets a boolean indicating whether the parameter will be bound using the parameter's
		/// default value (if it is an Optional parameter).
		/// </summary>
		public bool IsDefault {  get { return Target is OptionalParameterTarget; } }

		/// <summary>
		/// Constructs a new instance of the <see cref="ParameterBinding"/> class.
		/// </summary>
		/// <param name="parameter">Required - the parameter being bound</param>
		/// <param name="target">Optional - the argument supplied for the parameter.  Note - if this is null,
		/// then technically the parameter binding is invalid.</param>
		public ParameterBinding(ParameterInfo parameter, ITarget target)
		{
			Parameter = parameter;
			Target = target;
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
			return method.GetParameters().Select(pi =>
			{
				RezolvedTarget temp = new RezolvedTarget(pi.ParameterType);
				if (temp.CanResolve(context))
					return new ParameterBinding(pi, temp);
				if (pi.IsOptional)
					return new ParameterBinding(pi, new OptionalParameterTarget(pi));

				//return a default binding.
				return new ParameterBinding(pi, null);
			}).ToArray();
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
				ParameterBinding[] bindings;
				BindMethod(m, args, out bindings);

				return new { method = m, bindings = bindings };
			}).Where(mp => mp.bindings != null).OrderByDescending(mp => mp.bindings.Length).ToArray();

			if (candidates.Length == 0)
				throw new InvalidOperationException("None of the methods could be bound to the supplied arguments");

			//return the first match
			resolvedMethod = candidates[0].method;
			return candidates[0].bindings;
		}

		public static bool BindMethod(MethodBase method, IDictionary<string, ITarget> args, out ParameterBinding[] bindings)
		{
			bindings = null;
			var temp = method.GetParameters().Select(p => new ParameterBinding(p, GetArgValue(p, args))).ToArray();
			if (temp.All(pb => pb.Target != null && TypeHelpers.IsAssignableFrom(pb.Parameter.ParameterType, pb.Target.DeclaredType)))
			{
				bindings = temp;
				return true;
			}
			return false;
		}

		private static ITarget GetDefaultValueTargetIfOptional(ParameterInfo p)
		{
			if (p.IsOptional)
			{
				//now check to see if a default is supplied in the IL with the method
				if ((p.Attributes & ParameterAttributes.HasDefault) ==
						ParameterAttributes.HasDefault)
					return new ObjectTarget(p.DefaultValue, p.ParameterType);  //use the supplied default
				else
					return new DefaultTarget(p.ParameterType); //use the FastDefault method
			}

			return null;
		}

		private static ITarget GetArgValue(ParameterInfo p, IDictionary<string, ITarget> args)
		{
			ITarget argValue = null;
			if (args.TryGetValue(p.Name, out argValue))
				return argValue;

			return GetDefaultValueTargetIfOptional(p);
		}
	}
}