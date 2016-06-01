using System;
using System.Collections.Generic;

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

			public EnumerableFallbackTargetEntry(IRezolverBuilder parentBuilder, Type elementType)
			{
				_elementType = elementType;
				_enumerableType = typeof(IEnumerable<>).MakeGenericType(_elementType);
				ParentBuilder = parentBuilder;
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

			public bool UseFallback
			{
				get
				{
					return true;
				}
			}

			public IRezolverBuilder ParentBuilder
			{
				get;
			}

			public Type RegisteredType
			{
				get
				{
					return DeclaredType;
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

		public virtual void Register(IRezolveTarget target, Type type = null)
		{
			if (type == null)
				type = target.DeclaredType;

			if (target.SupportsType(type))
			{
				IRezolveTargetEntry entry = null;
				_targets.TryGetValue(type, out entry);
				var decoratorTarget = target as DecoratorTarget;
					//(hopefully) temporary hack
				if (decoratorTarget == null)
				{
					if (entry != null)
					{
						entry.AddTarget(target);
					}
					else
					{
						_targets[type] = CreateEntry(type, target); ;
					}
				}
				else
				{
					//note we always 
					_targets[type] = new DecoratingEntry(decoratorTarget, entry);
				}
			}
			else
				throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, type), "target");
		}

		protected virtual IRezolveTargetEntry CreateEntry(Type type, params IRezolveTarget[] targets)
		{
			return new RezolveTargetEntry(this, type, targets);
		}

		public virtual IRezolveTargetEntry Fetch(Type type)
		{
			type.MustNotBeNull("type");
			IRezolveTargetEntry entry;
			
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
					entry = Fetch(elementType);

					//because it's an enumerable the caller is after, we return an entry that
					//will return an empty enumerable of the requested type.
					if (entry == null)
						return new EnumerableFallbackTargetEntry(this, elementType);
				}
			}
			return entry;
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
