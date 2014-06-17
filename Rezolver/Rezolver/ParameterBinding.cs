using System.Reflection;

namespace Rezolver
{
	public class ParameterBinding
	{
		/// <summary>
		/// Represents an empty parameter bindings array.
		/// </summary>
		public static readonly ParameterBinding[] None = new ParameterBinding[0];

		public ParameterInfo Parameter { get; private set; }
		public RezolveTargetBase Target { get; private set; }

		public ParameterBinding(ParameterInfo parameter, RezolveTargetBase target)
		{
			Parameter = parameter;
			Target = target;
		}
	}
}