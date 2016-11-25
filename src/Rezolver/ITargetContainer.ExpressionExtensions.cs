using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for to simplify registering expressions in an <see cref="ITargetContainer"/>.
	/// </summary>
    public static class ExpressionTargetContainerExtensions
    {
		/// <summary>
		/// Registers an expression to be used as a factory for obtaining an instance when the registration matches a resolve request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="targetContainer"></param>
		/// <param name="expression">The expression to be analysed and used as a factory.  The argument that is received by this expression can be used to emit explicit
		/// calls back into the resolver to indicate that a particular argument/property value or whatever should be resolved.</param>
		/// <param name="type">Optional.  The type against which the registration is to be made, if different from <typeparamref name="T"/>.</param>
		/// <param name="adapter">Optional.  The adapter that will be used to convert the <paramref name="expression"/> into an <see cref="ITarget"/>.  This defaults
		/// to <see cref="TargetAdapter.Default"/>.  Extending this is an advanced topic and shouldn't be required in most cases.</param>
		/// <remarks>This is not the same as registering a factory delegate for creating objects - where the code you supply is already compiled and ready to go.  The expression that
		/// is passed is analysed by the <paramref name="adapter"/> (or the default) and rewritten according to the expressions container within.  In general, there is a one to one
		/// mapping between the code you provide and the code that's produced, but it's not guaranteed.  In particular, calls back to the resolver to resolve dependencies are
		/// identified and turned into a different representation internally, so that dependency resolution works inside your code in just the same way as it does when using the
		/// higher-level targets.</remarks>
		public static void RegisterExpression<T>(this ITargetContainer targetContainer, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, ITargetAdapter adapter = null)
		{
			targetContainer.MustNotBeNull("builder");
			expression.MustNotBeNull("expression");
			var target = (adapter ?? TargetAdapter.Default).CreateTarget(expression);
			targetContainer.Register(target, type ?? typeof(T));
		}

		//TODO: add functionality for expressions with additional parameters, similar to what we've done for the Func<> delegate
	}
}
