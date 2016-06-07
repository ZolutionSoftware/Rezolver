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
	public class Builder : ITargetContainerOwner
	{
		//TODO: extract an abstract base implementation of this class that does away with the dictionary, with extension points in place of those to allow for future expansion.
		//private readonly Dictionary<Type, ITargetContainer> _targets = new Dictionary<Type, ITargetContainer>();
		private readonly TargetDictionaryContainer _targets = new TargetDictionaryContainer();

		public virtual void Register(ITarget target, Type type = null)
		{
			target.MustNotBeNull(nameof(target));
			type = type ?? target.DeclaredType;

			if (!target.SupportsType(type))
				throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, type), "target");

			ITargetContainer container = null;
			//if the type we're registering is a generic type, then we use a generic container and register it inside that
			if (TypeHelpers.IsGenericType(type))
			{
				var genericTypeDef = type.GetGenericTypeDefinition();
				container = _targets.FetchContainer(genericTypeDef);
				if (container == null)
					_targets.RegisterContainer(genericTypeDef, container = new GenericTargetContainer(genericTypeDef));
				
				container.Register(target, type);
			}
			else
			{
				_targets.Register(target, type);
			}
		}

		/// <summary>
		/// Obtains the container that is primarily responsible for serving targets for the
		/// passed <paramref name="type"/>.  This implementation prioritises generic type definitions
		/// over their closed variants.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public ITargetContainer FetchContainer(Type type)
		{
			if (TypeHelpers.IsGenericType(type))
				return _targets.FetchContainer(type.GetGenericTypeDefinition());

			return _targets.FetchContainer(type);
		}

		public virtual ITarget Fetch(Type type)
		{
			type.MustNotBeNull(nameof(type));

			ITargetContainer entry = FetchContainer(type);
			if (entry != null)
				return entry.Fetch(type);
			return null;
		}

		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			type.MustNotBeNull(nameof(type));

			var entry = FetchContainer(type);
			if (entry != null)
				return entry.FetchAll(type);
			return Enumerable.Empty<ITarget>();
		}

		public void RegisterContainer(Type type, ITargetContainer container)
		{
			//it's possible that this needs to do the same generic type definition trick as
			//seen in the Register method - i.e. check against the generic type definition first
			_targets.RegisterContainer(type, container);
		}

		public ITargetContainerOwner CombineWith(ITargetContainerOwner existing, Type type)
		{
			throw new NotSupportedException("The builder type cannot be inserted into another container");
		}

		public Builder(bool autoRezolveIEnumerable = true)
		{
			//TODO: Change this 
			if (autoRezolveIEnumerable)
			{
				this.EnableEnumerableResolving();
			}
		}
	}
}
