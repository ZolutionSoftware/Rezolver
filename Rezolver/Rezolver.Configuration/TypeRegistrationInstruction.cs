using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// The basic workhorse of a parsed configuration - an instruction to register a target in an IRezolverBuilder instance
	/// against one or more types with a given name.
	/// </summary>
	public class TypeRegistrationInstruction : RezolverBuilderInstruction
	{
		/// <summary>
		/// The types that the target will be registered with
		/// </summary>
		public List<Type> TargetTypes { get; private set; }

		/// <summary>
		/// The target that is to be registered.
		/// </summary>
		public IRezolveTarget Target { get; private set; }

		/// <summary>
		/// Constructs a new instance of the <see cref="TypeRegistrationInstruction"/> class.
		/// </summary>
		/// <param name="targetTypes">The types that the target will be registered with</param>
		/// <param name="target">The target to be registered.</param>
		/// <param name="entry">The original configuration entry from which this instruction was built.</param>
		public TypeRegistrationInstruction(List<Type> targetTypes, IRezolveTarget target, IConfigurationEntry entry)
			: base(entry)
		{
			TargetTypes = targetTypes;
			Target = target;
		}

		/// <summary>
		/// The implementation will register the target for the given types.
		/// </summary>
		/// <param name="builder"></param>
		public override void Apply(IRezolverBuilder builder)
		{
			foreach (var type in TargetTypes)
			{
				builder.Register(Target, type);
			}
		}
	}
}
