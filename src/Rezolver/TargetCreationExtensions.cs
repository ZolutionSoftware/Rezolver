using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions which provide shortcuts for creating some of the targets in the <c>Rezolver.Targets</c>
	/// namespace.
	/// </summary>
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
		public static DelegateTarget AsDelegateTarget(this Delegate factory, Type declaredType = null)
		{
			return new DelegateTarget(factory, declaredType);
		}

		/// <summary>
		/// Creates a <see cref="ScopedTarget"/> from the target on which this method is invoked.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static ScopedTarget Scoped(this ITarget target)
		{
			target.MustNotBeNull(nameof(target));
			return new ScopedTarget(target);
		}

		/// <summary>
		/// Creates an <see cref="UnscopedTarget"/> from the target on which this method is invoked.
		/// </summary>
		/// <param name="target">The target.</param>
		public static UnscopedTarget Unscoped(this ITarget target)
		{
			target.MustNotBeNull(nameof(target));
			return new UnscopedTarget(target);
		}

		/// <summary>
		/// Constructs a <see cref="SingletonTarget"/> that wraps the target on which the method is invoked.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static SingletonTarget Singleton(this ITarget target)
		{
			target.MustNotBeNull(nameof(target));
			return new SingletonTarget(target);
		}
	}
}
