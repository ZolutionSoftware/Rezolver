using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// </summary>
	public class RezolverContainer : IRezolverContainer
	{

		private readonly IRezolverScope _scope;

		public RezolverContainer(IRezolverScope scope)
		{
			//TODO: check for null scope.
			_scope = scope;
		}

		public bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			return !GetCacheEntry(new CacheKey(type, name), CreateFactoryFunc).IsMiss;
		}

		public object Rezolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			CacheEntry entry = GetCacheEntry(new CacheKey(type, name), CreateFactoryFunc);

			if (entry.IsMiss)
				throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format("with name \"{0}\"", name) : ""));

			return entry.Factory(dynamicContainer);
		}

		private Func<IRezolverContainer, object> CreateFactoryFunc(Type type, string name)
		{

			var target = _scope.Fetch(type, name);
			if (target != null)
			{
				//slight issue here - if the target expression returns value type, then we're forcing a box/unbox
				//on this delegate and when it's called for the caller.
				//this perhaps could be fixed with a generic version of Rezolve

				//notice that despite the dynamic container being passed to this method, it's not the scope
				//that's provided to the CreateExpression call.  Instead, passing that container is deferred
				//until we execute the the delegate itself.  To support such dynamic scoping, a target must simply
				//reference the parameter expression ExpressionHelper.DynamicContainerParam.
				return ExpressionHelper.GetLambdaForTarget(this, type, target).Compile();
			}
			return null;

		}

		public class CacheKey : IEquatable<CacheKey>
		{
			private readonly Type _type;
			private readonly string _name;

			public Type Type { get { return _type; } }
			public string Name { get { return _name; } }

			public CacheKey(Type type, string name)
			{
				_type = type;
				_name = name;
			}

			public override int GetHashCode()
			{
				return _type.GetHashCode() ^ (_name != null ? _name.GetHashCode() : 0);
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as CacheKey);
			}

			public bool Equals(CacheKey other)
			{
				if (other != null)
				{
					return _type == other._type && _name == other._name;
				}
				return false;
			}
		}

		public class CacheEntry
		{
			private readonly Func<IRezolverContainer, object> _factory;
			public static readonly CacheEntry Miss = new CacheEntry();

			public bool IsMiss
			{
				get { return Miss == this; }
			}

			public Func<IRezolverContainer, object> Factory
			{
				get { return _factory; }
			}


			private CacheEntry()
			{
			}


			public CacheEntry(Func<IRezolverContainer, object> factory)
			{
				_factory = factory;
			}
		}

		private Dictionary<CacheKey, CacheEntry> _cache = new Dictionary<CacheKey, CacheEntry>();
		private CacheEntry GetCacheEntry(CacheKey key, Func<Type, string, Func<IRezolverContainer, object>> getFactory)
		{
			CacheEntry toReturn;

			if (_cache.TryGetValue(key, out toReturn))
				return toReturn;
			return _cache[key] = new CacheEntry(getFactory(key.Type, key.Name));
		}

		public void Register(IRezolveTarget target, Type type = null, RezolverScopePath path = null)
		{
			//you are not allowed to register targets directly into a container
			throw new NotSupportedException();
		}

		public IRezolveTarget Fetch(Type type, string name = null)
		{
			return _scope.Fetch(type, name);
		}

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named scopee, wwe don't support the call.
			if (create) throw new NotSupportedException();

			return _scope.GetNamedScope(path, false);
		}
	}
}