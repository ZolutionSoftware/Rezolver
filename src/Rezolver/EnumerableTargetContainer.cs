// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	using CompiledListFactory = Func<IEnumerable<ITarget>, bool, ITarget>;

	internal class EnumerableTargetContainer : GenericTargetContainer
	{
		#region CompiledListTarget
		/// <summary>
		/// Special variant of ListTarget which is used when auto-resolving an enumerable of objects where all
		/// the resolved targets are ICompiledTarget implementations.  This type also implements ICompiledTarget,
		/// which, in its implementation of GetObject, returns a fully materialised array or list of the objects
		/// returned by each target's GetObject call - bypassing the need to compile each target first, or compiling
		/// the list first.
		/// </summary>
		/// <typeparam name="TElement">The type of the t elemeent.</typeparam>
		/// <seealso cref="Rezolver.Targets.ListTarget" />
		/// <seealso cref="Rezolver.ICompiledTarget" />
		internal class CompiledListTarget<TElement> : ListTarget, ICompiledTarget
		{
			private readonly IEnumerable<ICompiledTarget> _compiledTargets;
            public ITarget SourceTarget => this;

			public CompiledListTarget(IEnumerable<ITarget> targets, bool asArray = false)
				: base(typeof(TElement), targets, asArray)
			{
				_compiledTargets = targets.Cast<ICompiledTarget>();
			}

			public object GetObject(IResolveContext context)
			{
				var elements = _compiledTargets.Select(i => (TElement)i.GetObject(context.New(newRequestedType: typeof(TElement))));
				return AsArray ? (object)elements.ToArray() : new List<TElement>(elements);
			}
		}

		#endregion

		private static readonly ConcurrentDictionary<Type, Lazy<CompiledListFactory>> _compiledTargetListFactories 
			= new ConcurrentDictionary<Type, Lazy<CompiledListFactory>>();

		internal static ITarget CreateListTarget(Type elementType, IEnumerable<ITarget> targets, bool asArray = false)
		{
			if (!targets.Any() || !targets.All(t => t is ICompiledTarget))
				return new ListTarget(elementType, targets, asArray);

			//get or build (and add) a dynamic binding to the generic CompiledTargetListTarget constructor
			return _compiledTargetListFactories.GetOrAdd(elementType, t => {
				return new Lazy<CompiledListFactory>(() => {
					var listTargetType = typeof(CompiledListTarget<>).MakeGenericType(t);
					var targetsParam = Expression.Parameter(typeof(IEnumerable<ITarget>), "targets");
					var asArrayParam = Expression.Parameter(typeof(bool), "asArray");

					return Expression.Lambda<CompiledListFactory>(
						Expression.Convert(
							Expression.New(TypeHelpers.GetConstructor(listTargetType, new[] { typeof(IEnumerable<ITarget>), typeof(bool) }), targetsParam, asArrayParam), 
							typeof(ITarget)
						),
						targetsParam,
						asArrayParam
					).Compile();
				});
			}).Value(targets, asArray);
		}


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

			//the method below has a shortcut for an enumerable of targets which are all ICompiledTarget
			//this enables containers to bypass compilation for an IEnumerable when all the underlying
			//targets are already able to return their objects (added to support expression compiler).
			return CreateListTarget(enumerableType, targets, true);
		}

		public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			//caters for the situation where our extension method EnableEnumerableResolving() is called more than once.
			if (existing is EnumerableTargetContainer) return existing;
			return base.CombineWith(existing, type);
		}
	}
}
