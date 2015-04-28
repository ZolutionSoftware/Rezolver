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
		/// The rezolve target that will provide the argument to the parameter when an expression is built.
		/// </summary>
		public IRezolveTarget Target { get; private set; }

		/// <summary>
		/// Constructs a new instance of the <see cref="ParameterBinding"/> class.
		/// </summary>
		/// <param name="parameter">Required - the parameter being bound</param>
		/// <param name="target">Optional - the argument supplied for the parameter.  Note - if this is null,
		/// then technically the parameter binding is invalid.</param>
		public ParameterBinding(ParameterInfo parameter, IRezolveTarget target)
		{
			Parameter = parameter;
			Target = target;
		}

		/// <summary>
		/// Automatically derives default parameter bindings for the passed method.  This typically means default values
		/// for optional parameters and type defaults (0, null etc) for any required parameters.
		/// 
		/// This is not *incredibly* useful except in cases where you simply want to bind a method or constructor
		/// without requiring any resolved instances/values.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static ParameterBinding[] DeriveDefaultParameterBindings(MethodBase method)
		{
			method.MustNotBeNull("method");
			var parameters = method.GetParameters();
			ParameterBinding[] toReturn = new ParameterBinding[parameters.Length];
			int current = 0;
			ParameterBinding binding = null;
			foreach (var parameter in parameters)
			{
				if (parameter.IsOptional)
				{
					if((parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
						binding = new ParameterBinding(parameter, new ExpressionTarget(Expression.Constant(parameter.DefaultValue, parameter.ParameterType)));
					else
						binding = new ParameterBinding(parameter, new ExpressionTarget(Expression.Default(parameter.ParameterType)));
				}
				toReturn[current++] = binding;
			}
			return toReturn;
		}

		public static ParameterBinding[] DeriveAutoParameterBindings(MethodBase method)
		{
			method.MustNotBeNull("method");
			var parameters = method.GetParameters();
			return parameters.Select(pi => new ParameterBinding(pi, new RezolvedTarget(pi.ParameterType))).ToArray();
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
		public static ParameterBinding[] BindOverload(MethodBase[] methods, IDictionary<string, IRezolveTarget> args, out MethodBase resolvedMethod)
		{
			resolvedMethod = null;
			methods.MustNotBeNull("methods");
			if (methods.Length == 0) throw new ArgumentException("The methods array must contain at least one item", "methods");

			//notice that the list is ordered most-greedy first.  This means that given a choice between two methods where the only difference
			///is a bunch of optional arguments in one, then that one will win out.
			var candidates = methods.Select(m => {
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

		public static bool BindMethod(MethodBase method, IDictionary<string, IRezolveTarget> args, out ParameterBinding[] bindings)
		{
			bindings = null;
			var temp = method.GetParameters().Select(p => new ParameterBinding(p, GetArgValue(p, args))).ToArray();
			if(temp.All(pb => pb.Target != null && pb.Parameter.ParameterType.IsAssignableFrom(pb.Target.DeclaredType)))
			{
				bindings = temp;
				return true;
			}
			return false;
		}

		private static IRezolveTarget GetArgValue(ParameterInfo p, IDictionary<string, IRezolveTarget> args)
		{
			IRezolveTarget argValue = null;
			if (args.TryGetValue(p.Name, out argValue))
				return argValue;
			else if (p.IsOptional)
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
	}
}