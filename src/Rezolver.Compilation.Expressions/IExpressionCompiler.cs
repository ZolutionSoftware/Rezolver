using System;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Interface for an object which is responsible for coordinating the production of expressions for targets
	/// during the compilation phase.
	/// 
	/// Objects implementing this are expected to be implementations of <see cref="ITargetCompiler"/>; this library
	/// provides the one implementation, too: <see cref="ExpressionCompiler"/>.
	/// </summary>
	/// <remarks>
	/// All expressions are built to be called from the <see cref="ICompiledTarget.GetObject(RezolveContext)"/> function which,
	/// in turn, is typically called in response to a container's <see cref="IContainer.Resolve(RezolveContext)"/> function being
	/// called.
	/// 
	/// Note that the <see cref="Build(ITarget, CompileContext)"/> method declared here is effectively an 
	/// analogue to the <see cref="IExpressionBuilder.Build(ITarget, CompileContext, IExpressionCompiler)"/>.  Indeed, the default 
	/// implementation resolves <see cref="IExpressionBuilder"/> instances to delegate the building of expressions.
	/// </remarks>
	/// <seealso cref="Rezolver.ITargetCompiler" />
	public interface IExpressionCompiler: ITargetCompiler
	{
		/// <summary>
		/// Gets an unoptimised expression containing the logic required to create or fetch an instance of the <paramref name="target"/>'s
		/// <see cref="ITarget.DeclaredType"/> when invoked for a particular <see cref="RezolveContext"/>.
		/// 
		/// Use this method if you want the raw expression for a target (possibly when integrating it into your own expressions during custom
		/// compilation).
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="context">The current compilation context.</param>
		/// <remarks>Whilst the returned expression can be directly used as the body for a new 
		/// <c>Expression&lt;Func&lt;RezolveContext, object&gt;&gt;</c>, you should not use it for that, because the expression will
		/// not have had potential duplicate logic and variables optimised away.  As mentioned in the summary, it's primarily useful
		/// when you want to incporate the code for one target inside that of another.
		/// 
		/// If you want the optimised code for the passed target, you should use the <see cref="BuildResolveLambda(ITarget, CompileContext)"/>
		/// function instead.</remarks>
		Expression Build(ITarget target, CompileContext context);
		/// <summary>
		/// Similar to the <see cref="Build(ITarget, CompileContext)"/> function, except the returned lambda will be optimised and can be 
		/// immediately compiled into a delegate and executed; or quoted inside another expression as a callback.
		/// 
		/// The <see cref="CompileContext.RezolveContextExpression"/> will be passed to define the single parameter for the lambda 
		/// that is created.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="context">The current compilation context.</param>
		Expression<Func<RezolveContext, object>> BuildResolveLambda(ITarget target, CompileContext context);
	}
}