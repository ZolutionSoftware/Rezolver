using System;
using System.Collections.Generic;
using Rezolver.Resources;

namespace Rezolver
{
	public class RezolverBuilder : IRezolverBuilder
	{
		//TODO: extract an abstract base implementation of this class that does away with the dictionary, with extension points in place of those to allow for future expansion.
		#region immediate type entries

		private readonly Dictionary<Type, IRezolveTarget> _targets = new Dictionary<Type, IRezolveTarget>();

		#endregion

		#region named child builders

		private readonly Dictionary<string, INamedRezolverBuilder> _namedBuilders = new Dictionary<string, INamedRezolverBuilder>();

		#endregion

		public void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			//TODO: Support name hierarchies by splitting the name by forward-slash and recursively creating a named-Builder tree.

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
				if (_targets.ContainsKey(type))
					throw new ArgumentException(string.Format(Exceptions.TypeIsAlreadyRegistered, type), "type");
				_targets[type] = target;
			}
			else
				throw new ArgumentException(string.Format(Exceptions.TargetDoesntSupportType_Format, type), "target");
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

		public IRezolveTarget Fetch(Type type, string name)
		{
			type.MustNotBeNull("type");

			if (name != null)
			{
				INamedRezolverBuilder namedBuilder;
				_namedBuilders.TryGetValue(name, out namedBuilder);

				// ReSharper disable once PossibleNullReferenceException
				return namedBuilder == null ? null : namedBuilder.Fetch(type);
			}

			IRezolveTarget target;
			return _targets.TryGetValue(type, out target) ? target : null;
		}

		public IRezolveTarget Fetch<T>(string name = null)
		{
			return Fetch(typeof(T), name);
		}

		public INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			if(!path.MoveNext())
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
	}
}
