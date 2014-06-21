using System;
using System.Linq;
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
		/// <param name="parameter"></param>
		/// <param name="target"></param>
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
						binding = new ParameterBinding(parameter, parameter.DefaultValue.AsObjectTarget(parameter.ParameterType));
					else
						binding = new ParameterBinding(parameter, new DefaultTarget(parameter.ParameterType));
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
	}
}