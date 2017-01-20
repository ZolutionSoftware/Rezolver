// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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

		/// <summary>
		/// Gets the targets stored in this list container.
		/// </summary>
		public IEnumerable<ITarget> Targets { get { return _targets.AsReadOnly(); } }

		/// <summary>
		/// Gets the type against which this list container is registered in its <see cref="ITargetContainerOwner"/>.
		/// </summary>
		public Type RegisteredType { get; }

		/// <summary>
		/// Gets the default target for this list - which will always be the last target added to the list, or 
		/// <c>null</c> if no targets have been added yet.
		/// </summary>
		public ITarget DefaultTarget
		{
			get
			{
				if (_targets.Count == 0) return null;

				return _targets[_targets.Count - 1];
			}
		}

		/// <summary>
		/// Gets the number of targets which have been added to the list.
		/// </summary>
		/// <value>The count.</value>
		public int Count { get { return TargetsList.Count; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetListContainer"/> class.
		/// </summary>
		/// <param name="registeredType">Required - the type against which this list will be registered.</param>
		/// <param name="targets">Optional array of targets with which to initialise the list.</param>
		public TargetListContainer(Type registeredType, params ITarget[] targets)
		{
			registeredType.MustNotBeNull(nameof(registeredType));

			RegisteredType = registeredType;
			_targets = new List<ITarget>(targets ?? new ITarget[0]);
		}

		/// <summary>
		/// Registers the specified target into the list.  Note - the target is not checked to see
		/// if it supports this list's <see cref="RegisteredType"/>.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="registeredType">Ignored.</param>
		public virtual void Register(ITarget target, Type registeredType = null)
		{
			target.MustNotBeNull(nameof(target));
			TargetsList.Add(target);
		}

		/// <summary>
		/// Always returns the <see cref="DefaultTarget"/>
		/// </summary>
		/// <param name="type">Ignored.</param>
		public virtual ITarget Fetch(Type type)
		{
			return DefaultTarget;
		}

		/// <summary>
		/// Retrieves an enumerable of all targets that have been registered to this list.
		/// </summary>
		/// <param name="type">Ignored.</param>
		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			return Targets;
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <param name="existing">Ignored</param>
		/// <param name="type">Ignored.</param>
		/// <exception cref="NotSupportedException">Always</exception>
		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the <see cref="ITarget"/> at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns>ITarget.</returns>
		/// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is less than zero, or
		/// if <see cref="Count"/> is zero, or if <paramref name="index"/> represents an index greater
		/// than last item's index</exception>
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
