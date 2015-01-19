using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Similar to the <see cref="RegisterInstruction"/> except this specifically wraps the <see cref="IRezolverBuilder.RegisterMultiple"/>
	/// method instead.  Construction is largely identical, except where in the aforementioned class you pass a single target, here you
	/// pass multiple targets in a list.
	/// </summary>
	public class RegisterMultipleInstruction : RezolverBuilderInstruction
	{
		/// <summary>
		/// The types that the target will be registered with
		/// </summary>
		public List<Type> TargetTypes { get; private set; }

		/// <summary>
		/// Gets the targets hat'll be used for the registration.
		/// </summary>
		/// <value>The targets.</value>
		public List<IRezolveTarget> Targets { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RegisterMultipleInstruction" /> class.
		/// </summary>
		/// <param name="targetTypes">The target types for the registration.</param>
		/// <param name="targets">The targets.  Note that this is a list to support modification after
		/// the instruction is created; since this is a configuration API.</param>
		/// <param name="entry">The source entry for this instruction - allows the system to track the instruction
		/// back to the configuration it was loaded from.</param>
		/// <exception cref="System.ArgumentNullException">
		/// targetTypes
		/// or
		/// targets
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// List must contain one or more types;targetTypes
		/// or
		/// All types in list must be non-null;targetTypes
		/// or
		/// List must contain one or more targets;targets
		/// or
		/// All targets in list must be non-null;targets
		/// </exception>
		public RegisterMultipleInstruction(List<Type> targetTypes, List<IRezolveTarget> targets, IConfigurationEntry entry)
			: base(entry)
		{
			if (targetTypes == null) throw new ArgumentNullException("targetTypes");
			if (targetTypes.Count == 0) throw new ArgumentException("List must contain one or more types", "targetTypes");
			if (targetTypes.Any(t => t == null)) throw new ArgumentException("All types in list must be non-null", "targetTypes");
			if (targets == null) throw new ArgumentNullException("targets");
			if (targets.Count == 0) throw new ArgumentException("List must contain one or more targets", "targets");
			if (targets.Any(t => t == null)) throw new ArgumentException("All targets in list must be non-null", "targets");
			TargetTypes = targetTypes;
			Targets = targets;
		}
		public override void Apply(IRezolverBuilder builder)
		{
			//the registration occurs against potentially multiple types, but each individual target will
			//yield a unique object, even if that target is a singleton.
			foreach(var type in TargetTypes)
			{
				builder.RegisterMultiple(Targets, type);
			}
		}
	}
}
