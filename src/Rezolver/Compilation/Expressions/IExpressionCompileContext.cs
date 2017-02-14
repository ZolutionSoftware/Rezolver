// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Extension to the <see cref="ICompileContext"/> interface which provides additional state
	/// and functionality for the <see cref="IExpressionCompiler"/> and the <see cref="IExpressionBuilder"/> implementations
	/// which are used by the default expression compiler, the <see cref="ExpressionCompiler"/> class.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.ICompileContext" />
	public interface IExpressionCompileContext : ICompileContext
	{
		/// <summary>
		/// Gets the parent context.
		/// </summary>
		/// <remarks>Note that this property hides the inherited <see cref="ICompileContext.ParentContext"/> property,
		/// since an <see cref="IExpressionCompileContext"/> can only be a child of another <see cref="IExpressionCompileContext"/>.</remarks>
		new IExpressionCompileContext ParentContext { get; }
		/// <summary>
		/// Gets an expression which gives a reference to the <see cref="IContainer" /> for this context -
		/// i.e. the same reference as given by the <see cref="ICompileContext.Container" /> property.
		/// </summary>
		/// <remarks>Note that this is *not* the same as <see cref="ContextContainerPropertyExpression"/> - but is provided
		/// to allow expressions to be compiled which compare the container supplied at compile time to the one from the 
		/// <see cref="ResolveContext.Container"/> at resolve-time.</remarks>
		Expression ContainerExpression { get; }
		/// <summary>
		/// Gets an expression for reading the <see cref="ResolveContext.Container"/> property of the <see cref="ResolveContext"/>
		/// that's in scope when the <see cref="ICompiledTarget"/> (which is built from the compiled expression) is executed.
		/// </summary>
		MemberExpression ContextContainerPropertyExpression { get; }
		/// <summary>
		/// Gets an expression for reading the <see cref="ResolveContext.Scope"/> property of the <see cref="ResolveContext"/>
		/// that's in scope when the <see cref="ICompiledTarget"/> (which is built from the compiled expression) is executed.
		/// </summary>
		MemberExpression ContextScopePropertyExpression { get; }
		/// <summary>
		/// This is the parameter expression which represents the <see cref="ResolveContext" /> that is passed to the
		/// <see cref="ICompiledTarget" /> at resolve-time.
		/// The other expressions - <see cref="ContextContainerPropertyExpression" /> and <see cref="ContextScopePropertyExpression" />
		/// are both built from this too.
		/// </summary>
		/// <remarks>If the code produced by the <see cref="IExpressionBuilder"/> for a given target needs to read or use the
		/// <see cref="ResolveContext"/> that was originally passed to the <see cref="IContainer.Resolve(ResolveContext)"/> method,
		/// then it does it by using this expression, which will be set as the only parameter on the lambda expression which is 
		/// eventually compiled (in the case of the default expression compiler, <see cref="ExpressionCompiler"/>.</remarks>
		ParameterExpression ResolveContextExpression { get; }
		/// <summary>
		/// Gets a read-only enumerable of all the shared expressions that have been inherited from any parent context or added
		/// via calls to <see cref="GetOrAddSharedExpression(Type, string, Func{Expression}, Type)"/> or
		/// <see cref="GetOrAddSharedLocal(Type, string, Type)"/>.
		/// </summary>
		/// <value>The shared expressions.</value>
		IEnumerable<Expression> SharedExpressions { get; }

		/// <summary>
		/// Creates a new <see cref="IExpressionCompileContext"/> using this one as a seed.  This function is identical to
		/// <see cref="ICompileContext.NewContext(Type, ScopeBehaviour?)"/> but allows you to control whether the <see cref="SharedExpressions"/>
		/// are inherited (the default); and is more convenient because it returns another <see cref="IExpressionCompileContext"/>.
		/// </summary>
		/// <param name="targetType">Optional.  The type for which the target is to be compiled, if different from this
		/// context's <see cref="CompileContext.TargetType"/>.</param>
		/// <param name="inheritSharedExpressions">If <c>true</c> then the shared expressions in this context will be inherited
		/// by the new context by reference.  That is, when the new context goes out of scope, any new shared expressions it created
		/// will still be available.
		/// If false, then the new context will get a brand new, empty, set of shared expressions.</param>
		/// <param name="scopeBehaviourOverride">Override the <see cref="CompileContext.ScopeBehaviourOverride"/> to be used for the target that is compiled with the new context.
		/// This is never inherited automatically from one context to another.</param>
		/// <remarks>When you have a reference to an <see cref="IExpressionCompileContext"/> the compiler will favour this method to
		/// the one defined on the <see cref="ICompileContext"/> interface because it is 'closer', even if you do not explicitly provide
		/// an argument for the <paramref name="inheritSharedExpressions"/> parameter.</remarks>
		IExpressionCompileContext NewContext(Type targetType = null, bool inheritSharedExpressions = true, ScopeBehaviour? scopeBehaviourOverride = null);
		/// <summary>
		/// Gets or adds an expression which is potentially shared between multiple targets' expression trees.
		/// </summary>
		/// <param name="type">Required - the type of the expression.</param>
		/// <param name="name">Required - the caller-defined name for this expression.</param>
		/// <param name="expressionFactory">Required - Delegate to call to create the expression if it does not already exist.</param>
		/// <param name="requestingType">Optional - the type of the object requesting this shared expression.  If this is provided,
		/// then the search for an existing shared expression will only work if the same requesting type was passed previously.</param>
		/// <remarks>Using shared expressions opens the door to potentially multiple optimisations, depending on the type of
		/// expression in question.  For example, conditional expressions which share the same operand and comparand can all be
		/// merged into one with all the 'true' and 'false' branches being combined into one of each, thus saving multiple identical
		/// comparisons.</remarks>
		Expression GetOrAddSharedExpression(Type type, string name, Func<Expression> expressionFactory, Type requestingType = null);
		/// <summary>
		/// Similar to <see cref="GetOrAddSharedExpression(Type, string, Func{Expression}, Type)"/>, except this is used when expression
		/// builders want to use local variables in block expressions to store the result of some operation in the expression tree built
		/// for a particular target.  Reusing one local variable is more efficient than declaring the same local multiple times.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="requestingType">Optional - the type of the object requesting this shared expression.  If this is provided,
		/// then the search for an existing shared expression will only work if the same requesting type was passed previously.</param>
		/// <remarks>When multiple expression trees from multiple targets are brought together into one lambda, there will
		/// often be many duplicate variables which could be shared.  So, if an <see cref="IExpressionBuilder"/> needs a local variable 
		/// for a block, instead of simply declaring it directly through the <see cref="Expression.Parameter(Type, string)"/> function,
		/// it can use this function instead, which will return a previously created one if available.</remarks>
		ParameterExpression GetOrAddSharedLocal(Type type, string name, Type requestingType = null);
	}
}