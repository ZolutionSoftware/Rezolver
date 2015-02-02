using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Suggested base class for implementations of <see cref="IRezolveTargetMetadata"/>.
	/// </summary>
	public abstract class RezolveTargetMetadataBase : IRezolveTargetMetadata
	{
		/// <summary>
		/// The type of rezolve target that is expected to be produced from this metadata
		/// </summary>
		/// <value>The type.</value>
		public RezolveTargetMetadataType Type
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RezolveTargetMetadataBase"/> class.
		/// </summary>
		/// <param name="type">The type of target.</param>
		/// <exception cref="System.ArgumentException">If type is RezolveTargetMetadataType.Extension this instance must implement IRezolveTargetMetadataExtension</exception>
		protected RezolveTargetMetadataBase(RezolveTargetMetadataType type)
		{
			if (type == RezolveTargetMetadataType.Extension && !(this is IRezolveTargetMetadataExtension))
				throw new ArgumentException("If type is RezolveTargetMetadataType.Extension this instance must implement IRezolveTargetMetadataExtension");
			Type = type;
		}

		/// <summary>
		/// Implementation of <see cref="IRezolveTargetMetadata.CreateRezolveTarget"/> - see that documentation for more detail about the intentions
		/// of this method.
		/// 
		/// This implementation will check all the arguments (for nulls etc), including that all <paramref name="targetTypes"/> have a common base;
		/// then it will invoke the abstract method <see cref="CreateRezolveTargetBase"/>.
		/// </summary>
		/// <param name="targetTypes">Required.  One or more target types that the returned target is expected to be compatible with (i.e.
		/// able to build an instance of). Generally, this will be the target types for the configuration entry that is passed in
		/// <paramref name="entry" />.</param>
		/// <param name="context">The current context - provides access to the builder currently being constructed, as well as methods
		/// for resolving type names from <see cref="ITypeReference" /> instances or strings (and more).</param>
		/// <param name="entry">If provided, this is a reference to the configuration entry (typically an <see cref="ITypeRegistrationEntry" />)
		/// against which this target will be registered.
		/// Please note - this doesn't mean that the target that is returned will become the target of the registration.  It might be that the
		/// target is one that is used by a parent target that will become the target of the registration.</param>
		/// <returns>IRezolveTarget.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// context
		/// or
		/// targetTypes
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Array must contain at least one target type;targetTypes
		/// or
		/// All items in the array must be non-null;targetTypes
		/// </exception>
		public virtual IRezolveTarget CreateRezolveTarget(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry = null)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (targetTypes == null) throw new ArgumentNullException("targetTypes");
			if (targetTypes.Length == 0) throw new ArgumentException("Array must contain at least one target type", "targetTypes");
			if (targetTypes.Any(t => t == null)) throw new ArgumentException("All items in the array must be non-null", "targetTypes");
			{
				var commonBase = targetTypes.FirstOrDefault(t => targetTypes.All(tt => tt != t || t.IsAssignableFrom(tt)));

				if (commonBase == null)
				{
					context.AddError(new ConfigurationError("If multiple types are provided for a target, they must all be part of a common hierarchy", entry));
					return null;
				}
			}

			return CreateRezolveTargetBase(targetTypes, context, entry);
		}

		/// <summary>
		/// Called by <see cref="CreateRezolveTarget"/> to create the rezolve target that will be registered into the <see cref="IRezolverBuilder"/>
		/// currently being built (available on the <paramref name="context"/>)
		/// 
		/// If an error occurs, you indicate that by adding to the <paramref name="context"/>'s errors collection, and return null.
		/// 
		/// You can also throw an exception, which will be caught and added to the errors collection for you.
		/// </summary>
		/// <param name="targetTypes">The target types.</param>
		/// <param name="context">The context.</param>
		/// <param name="entry">The entry.</param>
		/// <returns>IRezolveTarget.</returns>
		protected abstract IRezolveTarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry);
	}
}
