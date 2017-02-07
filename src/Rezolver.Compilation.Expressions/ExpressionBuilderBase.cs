// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Abstract starting point for implementing <see cref="IExpressionBuilder"/>.
	/// 
	/// Note that the interface is implemented explicitly; but exposes protected abstract or virtual 
	/// methods for inheritors to extend.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.IExpressionBuilder" />
	/// <remarks>This class takes care of checking the type requested in the <see cref="IExpressionCompileContext"/>
	/// is compatible with the target that's passed to the <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>
	/// method</remarks>
	public abstract class ExpressionBuilderBase : IExpressionBuilder
	{
		/// <summary>
		/// Gets the <see cref="IExpressionCompiler"/> to be used to build the expression for the given target for
		/// the given context, if different from one passed to this class' implementation of 
		/// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>.
		/// 
		/// This function is called by <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)"/> which will throw
		/// an exception if it returns null and no compiler was provided to <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>
		/// (typically via the explicit implementation of <see cref="IExpressionBuilder"/>).
		/// </summary>
		/// <param name="context">The current compile context.</param>
		/// <remarks>The base implementation simply attempts to resolve an instance of <see cref="IExpressionCompiler"/>
		/// from the <see cref="ICompileContext.Container"/> which should, with the default configuration, resolve to the root 
		/// <see cref="ExpressionCompiler"/>.  In order for this to work, it is imperative that the underlying registered target 
		/// implements the ICompiledTarget interface - so as to avoid needing a (or, more precisely, this) compiler needing 
		/// to compile it.</remarks>
		protected virtual IExpressionCompiler GetContextCompiler(IExpressionCompileContext context)
		{
			return context.Container.Resolve<IExpressionCompiler>();
		}

		/// <summary>
		/// The core expression build function - takes care of handling mismatched types between the target and
		/// the requested type in the context - both checking compatibility and producing conversion expressions
		/// where necessary.
		/// Also performs cyclic dependency checking.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="context">The context.</param>
		/// <param name="compiler">The compiler.</param>
		/// <exception cref="ArgumentException">targetType</exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <remarks>This class' implementation of <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> calls this,
		/// as does the derived abstract class <see cref="ExpressionBuilderBase{TTarget}" /> for its implementation
		/// of <see cref="IExpressionBuilder{TTarget}.Build(TTarget, IExpressionCompileContext, IExpressionCompiler)" />.
		/// 
		/// 
		/// It is this function that is responsible for calling the abstract <see cref="Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" />
		/// function, which deriving classes implement to actually produce their expression for the <paramref name="target"/>.
		/// </remarks>
		protected internal Expression BuildCore(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			if (context.TargetType != null && !target.SupportsType(context.TargetType))
				throw new ArgumentException(String.Format(ExceptionResources.TargetDoesntSupportType_Format, context.TargetType),
				  "targetType");

			if (!context.PushCompileStack(target))
				throw new InvalidOperationException(string.Format(ExceptionResources.CyclicDependencyDetectedInTargetFormat, target.GetType(), target.DeclaredType));

			try
			{
				var result = Build(target, context, compiler);
				Type convertType = context.TargetType ?? target.DeclaredType;

				result = new TargetExpressionRewriter(compiler, context).Visit(result);

				//always convert if types aren't the same.
				if (convertType != result.Type &&
					(TypeHelpers.IsAssignableFrom(convertType, target.DeclaredType)))
					result = Expression.Convert(result, convertType);

				//TODO: Rework scope tracking (probably via the target object itself so that the majority of the
				//expression crap can be removed.)

				//if scope tracking isn't disabled, either by this target or at the compile context level, then we 
				//add the boilerplate to add this object produced to the current scope.
				//if (!SuppressScopeTracking && !context.SuppressScopeTracking)
				//{
				//	result = CreateScopeTrackingExpression(context, result);
				//}

				return result;
			}
			finally
			{
				context.PopCompileStack();
			}
		}

		/// <summary>
		/// Explicit implementation of <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> -
		/// ultimately forwards the call to the <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> function.
		/// </summary>
		/// <param name="target">The target for which an expression is to be built</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">Optional. The compiler that's requesting the expression; and which can be used
		/// to compile other targets within the target.  If not provided, then the implementation attempts to locate
		/// the context compiler using the <see cref="GetContextCompiler(IExpressionCompileContext)"/> method, and will throw
		/// an <see cref="InvalidOperationException"/> if it cannot do so.</param>
		/// <exception cref="ArgumentNullException"><paramref name="target"/> is null or <paramref name="context"/> is null</exception>
		/// <exception cref="InvalidOperationException"><paramref name="compiler"/> is null and an IExpressionCompiler 
		/// couldn't be resolved for the current context (via <see cref="GetContextCompiler(IExpressionCompileContext)"/></exception>
		Expression IExpressionBuilder.Build(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			target.MustNotBeNull(nameof(target));
			context.MustNotBeNull(nameof(context));

			if(compiler == null)
			{
				compiler = GetContextCompiler(context);
				if (compiler == null)
					throw new InvalidOperationException("Unable to identify the IExpressionCompiler for the current context");
			}

			return BuildCore(target, context, compiler);
		}

		/// <summary>
		/// Abstract method used as part implementation of the
		/// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" />
		/// It's called by <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)" />.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="context">The context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target"/>.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided </param>
		protected abstract Expression Build(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler);

		/// <summary>
		/// Abstract method (implementation of <see cref="IExpressionBuilder.CanBuild(ITarget, IExpressionCompileContext)"/>) which
		/// determines whether this instance can build an expression for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="context">The compilation context.</param>
		public abstract bool CanBuild(ITarget target, IExpressionCompileContext context);
	}
}
