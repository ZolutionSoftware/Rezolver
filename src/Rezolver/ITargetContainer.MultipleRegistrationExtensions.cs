using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for registering multiple targets individually and against the same type
	/// </summary>
	public static class MultipleTargetContainerExtensions
    {
		/// <summary>
		/// Called to register multiple targets against the same type.
		/// 
		/// It is the same as calling <see cref="ITargetContainer.Register(ITarget, Type)"/> multiple times
		/// with the different targets.
		/// </summary>
		/// <param name="targetContainer">The container on which the registration is to be performed.</param>
		/// <param name="targets">The targets to be registered - all must support a common service type (potentially
		/// passed in the <paramref name="commonServiceType"/> argument.</param>
		/// <param name="commonServiceType">Optional - if provided, then this will be used as the common service type
		/// for registration.  If not provided, then the <see cref="ITarget.DeclaredType"/> of the first target
		/// will be used.</param>
		/// <remarks>If the container has the capability to handle enumerables, then each target will be returned
		/// when an IEnumerable of the common service type is requested.  This is an opt-in behaviour in Rezolver -
		/// implemented by the <see cref="EnumerableTargetContainer"/> and can be added to a target container
		/// with the extension method <see cref="EnumerableTargetBuilderExtensions.EnableEnumerableResolving(TargetContainer)"/>.
		/// 
		/// Note that default behaviour of <see cref="TargetContainer"/> is for this to be enabled.</remarks>
		public static void RegisterMultiple(this ITargetContainer targetContainer, IEnumerable<ITarget> targets, Type commonServiceType = null)
		{
			targets.MustNotBeNull("targets");
			var targetArray = targets.ToArray();
			if (targets.Any(t => t == null))
				throw new ArgumentException("All targets must be non-null", "targets");

			//for now I'm going to take the common type from the first target.
			if (commonServiceType == null)
			{
				commonServiceType = targetArray[0].DeclaredType;
			}

			if (targetArray.All(t => t.SupportsType(commonServiceType)))
			{
				foreach (var target in targets)
				{
					targetContainer.Register(target, commonServiceType);
				}
			}
			else
				throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, commonServiceType), "target");
		}

		/// <summary>
		/// Batch-registers multiple targets against their <see cref="ITarget.DeclaredType"/>.
		/// 
		/// This is the same as calling <see cref="ITargetContainer.Register(ITarget, Type)"/> for each of the 
		/// <paramref name="targets"/>, except the type cannot be overriden from the target's DeclaredType.
		/// </summary>
		/// <param name="targetContainer">The target container on which the registrations are to be performed.</param>
		/// <param name="targets">The targets to be registered</param>
		public static void RegisterAll(this ITargetContainer targetContainer, IEnumerable<ITarget> targets)
		{
			targetContainer.MustNotBeNull(nameof(targetContainer));

			foreach (var target in targets)
			{
				targetContainer.Register(target);
			}
		}

		/// <summary>
		/// Performs the same operations as <see cref="RegisterAll(ITargetContainer, IEnumerable{ITarget})"/> except
		/// via a variable number of <see cref="ITarget"/> arguments.
		/// </summary>
		/// <param name="targetContainer">The target container on which the registrations are to be performed.</param>
		/// <param name="targets">The targets to be registered.</param>
		public static void RegisterAll(this ITargetContainer targetContainer, params ITarget[] targets)
		{
			RegisterAll(targetContainer, targets.AsEnumerable());
		}

	}
}
