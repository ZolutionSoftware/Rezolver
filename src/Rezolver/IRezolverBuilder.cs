using Rezolver.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
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
		/// <param name="type">Optional.  The type the target is to be registered against, if different
		/// from the declared type on the <paramref name="target"/></param>
		/// <param name="path">Optional.  The path under which this target is to be registered.  One or more
		/// new named rezolvers could be created to accommodate the registration.</param>
		void Register(IRezolveTarget target, Type type = null, RezolverPath path = null);
		/// <summary>
		/// Searches for the target for a particular type and optionally
		/// under a particular named Builder.
		/// </summary>
		/// <param name="type">Required.  The type to be searched.</param>
		/// <param name="name">Optional.  The named builder to be searched.</param>
		/// <returns></returns>
		IRezolveTargetEntry Fetch(Type type, string name = null);		

		/// <summary>
		/// Retrieves, after optionally creating, a named Builder from this Builder.
		/// </summary>
		/// <param name="path">Required.  The path of the Builder to be retrieved or created.</param>
		/// <param name="create">If the Builder(s) do/does not exist, this parameter is used to specify whether you
		///   want it/them to be created.</param>
		/// <returns>Null if no Builder is found.  Otherwise the Builder that was found or created.</returns>
		INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false);
		/// <summary>
		/// Searches for the best-matched named child builder, or none if not applicable.
		/// 
		/// No creation on the fly is supported, obviously.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		INamedRezolverBuilder GetBestNamedBuilder(RezolverPath path);
	}

	public static class RezolverBuilderExtensions
	{
		//TODO: get parity with the extension methods that I've added for IRezolver that wrap around its Builder property.
		public static void Register<T>(this IRezolverBuilder builder, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, RezolverPath path = null, IRezolveTargetAdapter adapter = null)
		{
			builder.MustNotBeNull("builder");
			expression.MustNotBeNull("expression");
			var target = (adapter ?? RezolveTargetAdapter.Default).GetRezolveTarget(expression);
			builder.Register(target, type ?? typeof(T), path);
		}

		/// <summary>
		/// Called to register multiple rezolve targets against a shared contract, optionally replacing any 
		/// existing registration(s) or extending them.
		/// 
		/// It is analogous to calling <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/> multiple times
		/// with the different targets.
		/// </summary>
		/// <param name="builder">The builder in which the registration is to be performed.</param>
		/// <param name="targets">The targets to be registered - all must support a common service type (potentially
		/// passed in the <paramref name="commonServiceType"/> argument.</param>
		/// <param name="commonServiceType">Optional - instead of determining the common service type automatically,
		/// you can provide it in advance through this parameter.  Note that all targets must support this type.</param>
		/// <param name="path">Optional path under which this registration is to be made.</param>
		/// <param name="append"></param>
		public static void RegisterMultiple(this IRezolverBuilder builder, IEnumerable<IRezolveTarget> targets, Type commonServiceType = null, RezolverPath path = null)
		{
			targets.MustNotBeNull("targets");
			var targetArray = targets.ToArray();
			if (targets.Any(t => t == null))
				throw new ArgumentException("All targets must be non-null", "targets");

			if (path != null)
			{
				if (path.Next == null)
					throw new ArgumentException(Exceptions.PathIsAtEnd, "path");

				//get the named Builder.  If it doesn't exist, create one.
				var childBuilder = builder.GetNamedBuilder(path, true);
				//note here we don't pass the name through.
				//when we support named scopes, we would be lopping off the first item in a hierarchical name to allow for the recursion.
				childBuilder.RegisterMultiple(targets, commonServiceType);
				return;
			}

			//for now I'm going to take the common type from the first target.
			if (commonServiceType == null)
			{
				commonServiceType = targetArray[0].DeclaredType;
			}

			if (targetArray.All(t => t.SupportsType(commonServiceType)))
			{
				IRezolveTargetEntry existing = builder.Fetch(commonServiceType);
				//MultipleRezolveTarget multipleTarget = null;
				//Type targetType = MultipleRezolveTarget.MakeEnumerableType(commonServiceType);

				if (existing != null)
				{
					foreach (var target in targets)
					{
						existing.AddTarget(target);
					}
				}
				else
				{
					foreach (var target in targets)
					{
						builder.Register(target, commonServiceType, null);
					}
				}
			}
			else
				throw new ArgumentException(string.Format(Exceptions.TargetDoesntSupportType_Format, commonServiceType), "target");
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
