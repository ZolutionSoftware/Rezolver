using System;
using System.Collections.Generic;
using Rezolver.Resources;

namespace Rezolver
{
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Linq.Expressions;
	using RegistrationEntry = KeyValuePair<RezolveContext, IRezolveTarget>;

	/// <summary>
	/// Stores the underlying registrations used by an <see cref="IRezolver"/> instance (assuming a conforming
	/// implementation).
	/// 
	/// This is the builder type used by default for the <see cref="DefaultRezolver"/> and <see cref="DefaultLifetimeScopeRezolver"/> when you don't
	/// supply an instance of an <see cref="IRezolverBuilder"/> explicitly on construction.
	/// </summary>
	public class RezolverBuilder : IRezolverBuilder
	{
		//TODO: extract an abstract base implementation of this class that does away with the dictionary, with extension points in place of those to allow for future expansion.
		#region immediate type entries

		private readonly Dictionary<Type, IRezolveTargetEntry> _targets = new Dictionary<Type, IRezolveTargetEntry>();

		#endregion

		#region named child builders

		private readonly Dictionary<string, INamedRezolverBuilder> _namedBuilders = new Dictionary<string, INamedRezolverBuilder>();

		#endregion

		private class RezolverBuilderWalker : IEnumerable<RegistrationEntry>
		{
			private RezolverBuilder _builder;
			private IEnumerator<RegistrationEntry> _enumerator;
			public RezolverBuilderWalker(RezolverBuilder builder)
			{
				_builder = builder;
				_enumerator = Enumerate().GetEnumerator();
			}

			private IEnumerable<RegistrationEntry> Enumerate()
			{
				foreach (var registration in _builder._targets)
				{
					yield return new RegistrationEntry(new RezolveContext(null, registration.Key), registration.Value);
				}
				foreach (var namedBuilderEntry in _builder._namedBuilders)
				{
					foreach (var registration in namedBuilderEntry.Value.AllRegistrations)
					{
						yield return new RegistrationEntry(registration.Key.CreateNew(registration.Key.RequestedType, namedBuilderEntry.Key + RezolverPath.DefaultPathSeparator + registration.Key.Name), registration.Value);
					}
				}
			}

			public IEnumerator<RegistrationEntry> GetEnumerator()
			{
				return Enumerate().GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class EnumerableFallbackTargetEntry : IRezolveTargetEntry
		{
			private readonly Type _elementType;
			private readonly Type _enumerableType;

			public EnumerableFallbackTargetEntry(Type elementType)
			{
				_elementType = elementType;
				_enumerableType = typeof(IEnumerable<>).MakeGenericType(_elementType);
			}

			public Type DeclaredType
			{
				get
				{
					return typeof(IEnumerable<>).MakeGenericType(_elementType);
				}
			}

			public IRezolveTarget DefaultTarget
			{
				get
				{
					return this;
				}
			}

			public IEnumerable<IRezolveTarget> Targets
			{
				get
				{
					return new[] { this };
				}
			}

			public Expression CreateExpression(CompileContext context)
			{
				return Expression.Call(
						MethodCallExtractor.ExtractCalledMethod(() => Enumerable.Empty<object>()).GetGenericMethodDefinition().MakeGenericMethod(_elementType));
			}

			public bool SupportsType(Type type)
			{
				return type.Equals(_enumerableType);
			}

			public void AddTarget(IRezolveTarget target, bool checkForDuplicates = false)
			{
				throw new NotImplementedException();
			}
		}

		public void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			if (path != null)
			{
				if (path.Next == null)
					throw new ArgumentException(Exceptions.PathIsAtEnd, "path");

				//get the named Builder.  If it doesn't exist, create one.
				var builder = GetNamedBuilder(path, true);
				//note here we don't pass the name through.
				//when we support named scopes, we would be lopping off the first item in a hierarchical name to allow for the recursion.
				builder.Register(target, type);
				return;
			}

			if (type == null)
				type = target.DeclaredType;

			if (target.SupportsType(type))
			{
				IRezolveTargetEntry entry = null;
				if (_targets.TryGetValue(type, out entry))
				{
					entry.AddTarget(target);
				}
				else
				{
					entry = CreateEntry(type, target);
					_targets[type] = entry;
				}
			}
			else
				throw new ArgumentException(string.Format(Exceptions.TargetDoesntSupportType_Format, type), "target");
		}

		protected virtual IRezolveTargetEntry CreateEntry(Type type, params IRezolveTarget[] targets)
		{
			return new RezolveTargetEntry(type, targets);
		}

		/// <summary>
		/// Called to create a new instance of a Named Builder with the given name, optionally for the given target and type.
		/// </summary>
		/// <param name="name">The name of the Builder to be created.  Please note - this could be being created
		/// as part of a wider path of scopes.</param>
		/// <param name="target">Optional - a target that is to be added to the named Builder after creation.</param>
		/// <returns></returns>
		protected virtual INamedRezolverBuilder CreateNamedBuilder(string name, IRezolveTarget target)
		{
			//base class simply creates a NamedRezolverBuilder
			return new NamedRezolverBuilder(this, name);
		}

		public virtual IRezolveTargetEntry Fetch(Type type, string name)
		{
			type.MustNotBeNull("type");
			IRezolveTargetEntry entry;
			if (name != null)
			{
				var namedBuilder = GetBestNamedBuilder(name);
				if (namedBuilder != null)
				{
					return namedBuilder.Fetch(type);
				}
			}

			var result = _targets.TryGetValue(type, out entry);
			if (!result && TypeHelpers.IsGenericType(type))
			{
				//generate a generic type list for searching
				foreach (var searchType in DeriveGenericTypeSearchList(type))
				{
					if (_targets.TryGetValue(searchType, out entry))
						return entry;
				}

				//If we still don't find anything, then we see if the type is IEnumerable<T>.
				//If it is, we look for the T (we recurse, though to keep the logic simple)
				if (type.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
				{
					Type elementType = TypeHelpers.GetGenericArguments(type)[0];
					entry = Fetch(elementType, name);

					//because it's an enumerable the caller is after, we return an entry that
					//will return an empty enumerable of the requested type.
					if (entry == null)
						return new EnumerableFallbackTargetEntry(elementType);
				}
			}
			return entry;
		}

		public INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			if (!path.MoveNext())
				throw new ArgumentException(Exceptions.PathIsAtEnd, "path");

			INamedRezolverBuilder namedBuilder;

			if (!_namedBuilders.TryGetValue(path.Current, out namedBuilder))
			{
				if (!create)
					return null;
				_namedBuilders[path.Current] = namedBuilder = CreateNamedBuilder(path.Current, null);
			}
			//then walk to the next part of the path and create it if need be
			return path.Next != null ? namedBuilder.GetNamedBuilder(path, true) : namedBuilder;
		}

		public INamedRezolverBuilder GetBestNamedBuilder(RezolverPath path)
		{
			//retrieves the last builder found along the path supplied.  If the path is a.b.c and
			//we only get to a.b, then you'll get the a.b builder.
			//If the method returns null, then this builder doesn't have a first-level child with the name.
			if (!path.MoveNext())
				throw new ArgumentException(Exceptions.PathIsAtEnd, "path");

			INamedRezolverBuilder namedBuilder;

			if (!_namedBuilders.TryGetValue(path.Current, out namedBuilder))
				return this as INamedRezolverBuilder; //if this is a named builder that we've descended to, then 
																							//this is the best match.  A route RezolverBuilder (if using
																							//the default types) will return null here.

			//then walk to the next part of the path and carry on
			return path.Next != null ? namedBuilder.GetBestNamedBuilder(path) : namedBuilder;
		}

		public IEnumerable<RegistrationEntry> AllRegistrations
		{
			get
			{
				return new RezolverBuilderWalker(this);
			}
		}


		private IEnumerable<Type> DeriveGenericTypeSearchList(Type type)
		{
			//using an iterator method is not the best for performance, but fetching type
			//registrations from a rezolver builder is an operation that, so long as a caching
			//resolver is used, shouldn't be repeated often.

			if (!TypeHelpers.IsGenericType(type) || TypeHelpers.IsGenericTypeDefinition(type))
			{
				yield return type;
				yield break;
			}

			//for every generic type, there is at least two versions - the closed and the open
			//when you consider, then, that a generic parameter might also be a generic
			var typeParams = TypeHelpers.GetGenericArguments(type);
			var typeParamSearchLists = typeParams.Select(t => DeriveGenericTypeSearchList(t).ToArray()).ToArray();
			var genericType = type.GetGenericTypeDefinition();

			foreach (var combination in CartesianProduct(typeParamSearchLists))
			{
				yield return genericType.MakeGenericType(combination.ToArray());
			}
			yield return genericType;
		}

		static IEnumerable<IEnumerable<T>> CartesianProduct<T>
		(IEnumerable<IEnumerable<T>> sequences)
		{
			//thank you Eric Lippert...
			IEnumerable<IEnumerable<T>> emptyProduct =
				new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate(
				emptyProduct,
				(accumulator, sequence) =>
					from accseq in accumulator
					from item in sequence
					select accseq.Concat(new[] { item }));
		}
	}
}
