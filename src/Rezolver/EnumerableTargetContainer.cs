// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class EnumerableTargetContainer : GenericTargetContainer
	{
		ITargetContainer _parent;
		public EnumerableTargetContainer(ITargetContainer parent) : base(typeof(IEnumerable<>))
		{
			_parent = parent;
		}

		public override ITarget Fetch(Type type)
		{
			if (!TypeHelpers.IsGenericType(type))
				throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
			Type genericType = type.GetGenericTypeDefinition();
			if (genericType != typeof(IEnumerable<>))
				throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
			//we allow for specific IEnumerable<T> registrations
			var result = base.Fetch(type);

			if (result != null)
				return result;

			var enumerableType = TypeHelpers.GetGenericArguments(type)[0];

			var targets = _parent.FetchAll(enumerableType);
			return new ListTarget(enumerableType, targets, true);
		}

		public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			//caters for the situation where our extension method EnableEnumerableResolving() is called more than once.
			if (existing is EnumerableTargetContainer) return existing;
			return base.CombineWith(existing, type);
		}
	}

	/// <summary>
	/// Houses an extension method which enables native resolving of IEnumerables of services on 
	/// <see cref="ITargetContainer"/> containers which, in turn, enables it for any <see cref="ContainerBase"/>
	/// containers which use that target container.
	/// </summary>
	public static class EnumerableTargetBuilderExtensions
	{
		/// <summary>
		/// Enables resolving of enumerables of services on the target container.
		/// </summary>
		/// <param name="targetContainer">The target container.</param>
		/// <remarks>
		/// After calling this, you can immediately request a target for <see cref="IEnumerable{T}"/> of
		/// any type and you will receive a <see cref="ListTarget"/> (with <see cref="ListTarget.AsArray"/>
		/// set to true) which contains all the targets which have previously been
		/// registered for the type <c>T</c>, in the order they were registered.
		///  
		/// If a service has not been registered, then the returned <see cref="ListTarget"/> will be empty
		/// and its <see cref="ListTarget.UseFallback"/> property will be <c>true</c>.</remarks>
		public static void EnableEnumerableResolving(this ITargetContainerOwner targetContainer)
		{
			targetContainer.MustNotBeNull(nameof(targetContainer));
			targetContainer.RegisterContainer(typeof(IEnumerable<>), new EnumerableTargetContainer(targetContainer));
		}
	}
}
