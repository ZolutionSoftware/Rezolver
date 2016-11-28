using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extension methods for ITargetContainer designed to simplify the registration of <see cref="DelegateTarget"/> and its
	/// numerous generic variants.
	/// </summary>
    public static partial class DelegateTargetContainerExtensions
    {
		/// <summary>
		/// Constructs a <see cref="DelegateTarget"/> from the passed factory delegate (optionally with the given <paramref name="declaredType"/>)
		/// and registers it in the target container.
		/// </summary>
		/// <param name="targetContainer"></param>
		/// <param name="factory"></param>
		/// <param name="declaredType"></param>
		public static void RegisterDelegate(this ITargetContainer targetContainer, Delegate factory, Type declaredType = null)
		{
			targetContainer.MustNotBeNull(nameof(targetContainer));
			factory.MustNotBeNull(nameof(factory));
			targetContainer.Register(factory.AsDelegateTarget(declaredType));
		}
    }
}
