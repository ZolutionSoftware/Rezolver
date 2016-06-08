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

			_targets.Register(target, type);
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
			return _targets.FetchContainer(type);
		}

		public virtual ITarget Fetch(Type type)
		{
			return _targets.Fetch(type);
		}

		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			return _targets.FetchAll(type);
		}

		public void RegisterContainer(Type type, ITargetContainer container)
		{
			//it's possible that this needs to do the same generic type definition trick as
			//seen in the Register method - i.e. check against the generic type definition first
			_targets.RegisterContainer(type, container);
		}

		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
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
