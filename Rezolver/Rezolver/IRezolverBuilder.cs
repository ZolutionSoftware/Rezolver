using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	public interface IRezolverBuilder
	{
		/// <summary>
		/// Allows a caller to introspect all the registrations that have been added to this builder.
		/// </summary>
		IEnumerable<KeyValuePair<RezolveContext, IRezolveTarget>> AllRegistrations { get; }
		/// <summary>
		/// Registers a target, optionally for a particular target type and optionally
		/// under a particular name.
		/// </summary>
		/// <param name="target">Required.  The target to be registereed</param>
		/// <param name="type">Optional.  The type thee target is to be registered against, if different
		/// from the declared type on the <paramref name="target"/></param>
		/// <param name="path">Optional.  The path under which this target is to be registered.  One or more
		/// new named rezolvers could be created to accommodate the registration.</param>
		void Register(IRezolveTarget target, Type type = null, RezolverPath path = null);
		/// <summary>
		/// Called to register multiple rezolve targets against a shared contract, optionally replacing any 
		/// existing registration(s) or extending them.  In the case of a builder that is a child of another, 
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="commonServiceType"></param>
		/// <param name="path"></param>
		/// <param name="append"></param>
		void RegisterMultiple(IEnumerable<IRezolveTarget> targets, Type commonServiceType = null, RezolverPath path = null, bool append=true);
		/// <summary>
		/// Searches for a target for a particular type and optionally
		/// under a particular named Builder.
		/// </summary>
		/// <param name="type">Required.  The type to be searched.</param>
		/// <param name="name">Optional.  The named builder to be searched.</param>
		/// <returns></returns>
		IRezolveTarget Fetch(Type type, string name = null);
		IRezolveTarget Fetch<T>(string name = null);
		

		/// <summary>
		/// Retrieves, after optionally creating, a named Builder from this Builder.
		/// </summary>
		/// <param name="path">Required.  The path of the Builder to be retrieved or created.</param>
		/// <param name="create">If the Builder(s) do/does not exist, this parameter is used to specify whether you
		///   want it/them to be created.</param>
		/// <returns>Null if no Builder is found.  Otherwise the Builder that was found or created.</returns>
		INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false);
	}

	public static class RezolverBuilderExtensions
	{
		public static void Register<T>(this IRezolverBuilder builder, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, RezolverPath path = null, IRezolveTargetAdapter adapter = null)
		{
			builder.MustNotBeNull("builder");
			expression.MustNotBeNull("expression");
			var target = (adapter ?? RezolveTargetAdapter.Default).GetRezolveTarget(expression);
			builder.Register(target, type ?? typeof(T), path);
		}

		//public static void RegisterMultiple(this IRezolverBuilder builder, Type targetType = null, RezolverPath path = null, bool append = true, params IRezolveTarget[] targets)
		//{
		//	builder.MustNotBeNull("builder");
		//	targets.MustNotBeNull("targets");
		//	if (targets.Length == 0)
		//		throw new ArgumentException("You must provide at least one target to be registered", "targets");
		//	builder.RegisterMultiple(((IEnumerable<IRezolveTarget>)targets), targetType, path, append);
		//}
	}
}
