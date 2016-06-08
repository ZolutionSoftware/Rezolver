using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// An <see cref="ITargetContainer"/> that stores multiple targets in a list.
	/// 
	/// This is not a type that you would typically use directly in your code, unless you are writing 
	/// custom logic/behaviour for <see cref="ITarget"/> registration.
	/// </summary>
	/// <remarks>
	/// This type is not thread-safe, nor does it perform any type checking on the targets
	/// that are added to it.
	/// </remarks>
	public class TargetListContainer : ITargetContainer
	{
		private List<ITarget> _targets;

		/// <summary>
		/// Provides deriving classes a means to manipulate the underlying list.
		/// </summary>
		protected List<ITarget> TargetsList { get { return _targets; } }

		public IEnumerable<ITarget> Targets { get { return _targets.AsReadOnly(); } }


		public Type RegisteredType { get; }

		public ITarget DefaultTarget
		{
			get
			{
				if (_targets.Count == 0) return null;

				return _targets[_targets.Count - 1];
			}
		}

		public int Count { get { return TargetsList.Count; } }

		public TargetListContainer(Type registeredType, params ITarget[] targets)
		{
			RegisteredType = registeredType;
			_targets = new List<ITarget>(targets ?? new ITarget[0]);
		}

		public virtual void Register(ITarget target, Type registeredType = null)
		{
			target.MustNotBeNull(nameof(target));
			TargetsList.Add(target);
		}

		public virtual ITarget Fetch(Type type)
		{
			return DefaultTarget;
		}

		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			return TargetsList.AsReadOnly();
		}

		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}

		public ITarget this[int index]
		{
			get
			{
				if (index < 0 || index > (TargetsList.Count - 1))
					throw new IndexOutOfRangeException();

				return TargetsList[index];
			}
		}
	}
}
