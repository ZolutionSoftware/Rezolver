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
	public class RegisterInstruction : RezolverBuilderInstruction
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
		/// Constructs a new instance of the <see cref="RegisterInstruction" /> class.
		/// </summary>
		/// <param name="targetTypes">The types that the target will be registered with</param>
		/// <param name="target">The target to be registered.</param>
		/// <param name="entry">The original configuration entry from which this instruction was built.</param>
		/// <exception cref="System.ArgumentNullException">
		/// targetTypes
		/// or
		/// target
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// List must contain one or more types;targetTypes
		/// or
		/// All types in list must be non-null;targetTypes
		/// </exception>
		public RegisterInstruction(List<Type> targetTypes, IRezolveTarget target, IConfigurationEntry entry)
			: base(entry)
		{
			if (targetTypes == null) throw new ArgumentNullException("targetTypes");
			if (targetTypes.Count == 0) throw new ArgumentException("List must contain one or more types", "targetTypes");
			if (targetTypes.Any(t => t == null)) throw new ArgumentException("All types in list must be non-null", "targetTypes");
			if (target == null) throw new ArgumentNullException("target");

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
