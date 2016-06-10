using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
	/// <summary>
	/// This is the type used by default for the <see cref="IContainer.Builder"/> of the <see cref="Container"/> 
	/// and <see cref="ScopedContainer"/> when you don't supply an instance of an 
	/// <see cref="ITargetContainer"/> explicitly on construction.
	/// </summary>
	public class Builder : TargetDictionaryContainer
	{
		public Builder(bool autoRezolveIEnumerable = true)
		{
			//TODO: Change this 
			if (autoRezolveIEnumerable)
			{
				this.EnableEnumerableResolving();
			}
		}

		protected override ITargetContainer CreateContainer(Type serviceType, ITarget target)
		{
			if (TypeHelpers.IsGenericType(serviceType))
			{
				serviceType = serviceType.GetGenericTypeDefinition();
				var created = CreateGenericTypeDefContainer(serviceType, target);
				RegisterContainer(serviceType, created);
				return created;
			}
			else
				return base.CreateContainer(serviceType, target);
		}

		/// <summary>
		/// Called by the base implementation of <see cref="CreateContainer( Type,ITarget)"/>
		/// </summary>
		/// <param name="genericTypeDefinition">Will be an open generic type (generic type definition)</param>
		/// <param name="target">The initial target for which the container is being constructed</param>
		/// <returns>The base implementation always creates an instance of <see cref="CreateGenericTypeDefContainer( Type,ITarget)"/></returns>
		protected virtual ITargetContainer CreateGenericTypeDefContainer(Type genericTypeDefinition, ITarget target)
		{
			return new GenericTargetContainer(genericTypeDefinition);
		}

		public override ITargetContainer FetchContainer(Type type)
		{
			if (TypeHelpers.IsGenericType(type))
				return base.FetchContainer(type.GetGenericTypeDefinition());

			return base.FetchContainer(type);
		}

		public override void Register(ITarget target, Type serviceType = null)
		{
			target.MustNotBeNull(nameof(target));
			if (serviceType != null && !target.SupportsType(serviceType))
				throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, serviceType), nameof(target));

			base.Register(target, serviceType);
		}
	}
}
