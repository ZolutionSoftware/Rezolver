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
	/// Note that the <see cref="Build(ITarget, CompileContext)"/> method declared here is effectively an 
	/// analogue to the <see cref="IExpressionBuilder.Build(ITarget, CompileContext, IExpressionCompiler)"/>.  Indeed, the default 
	/// implementation resolves <see cref="IExpressionBuilder"/> instances to delegate the building of expressions.
	/// </remarks>
	/// <seealso cref="Rezolver.ITargetCompiler" />
	public interface IExpressionCompiler: ITargetCompiler
	{
		/// <summary>
		/// Gets an expressino which, if compiled will resolve the object represented by the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="context">The current compilation context.</param>
		Expression Build(ITarget target, CompileContext context);
	}
}