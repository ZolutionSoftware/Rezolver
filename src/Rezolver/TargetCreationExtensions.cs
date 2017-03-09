using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Has been replaced by the <see cref="Target"/> static class, and will be removed from 1.2
    /// </summary>
    [Obsolete("This static class has been replaces by the Target static class and will be obsoleted in 1.2, please remove all references to it")]
	public static class TargetCreationExtensions
    {
		/// <summary>
		/// Wraps the instance on which this is invoked as an <see cref="ObjectTarget"/> that can be registered into an <see cref="ITargetContainer"/>.
		/// 
		/// The parameters are direct analogues of the parameters on the type's constructor (see <see cref="ObjectTarget.ObjectTarget(object, Type, ScopeBehaviour)"/>).
		/// </summary>
		/// <typeparam name="T">The type of object being wrapped</typeparam>
		/// <param name="obj">the object being wrapped</param>
		/// <param name="declaredType">Optional.  The type which is to be set as the <see cref="ObjectTarget.DeclaredType"/> of the created target.</param>
		/// <param name="scopeBehaviour">Controls how the object will interact the the scope.  By default, object targets must be disposed by you.</param>
		/// <returns>A new object target that wraps the object <paramref name="obj"/>.</returns>
        [Obsolete("This method will be removed in 1.2 in favour of Target.ForObject<T>(T)")]
		public static ObjectTarget AsObjectTarget<T>(this T obj, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.None)
		{
			return new ObjectTarget(obj, declaredType ?? typeof(T), scopeBehaviour: scopeBehaviour);
		}

		/// <summary>
		/// Creates a <see cref="DelegateTarget"/> from the <paramref name="factory"/> which can be registered in an 
		/// <see cref="ITargetContainer"/> to resolve an instance of a type compatible with the delegate's return type
		/// and, optionally, with the <paramref name="declaredType" />
		/// </summary>
		/// <param name="factory">The delegate to be used as a factory.</param>
		/// <param name="declaredType">Optional type to set as the <see cref="DelegateTarget.DeclaredType"/> of the target,
		/// if not passed, then the return type of the delegate will be used.</param>
        [Obsolete("This method will be removed in 1.2 in favour of the Target.ForFactory static methods")]
		public static DelegateTarget AsDelegateTarget(this Delegate factory, Type declaredType = null)
		{
			return new DelegateTarget(factory, declaredType);
		}
	}
}
