// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information



namespace Rezolver
{
	using System;
	using System.Linq.Expressions;
	using Rezolver.Targets;

	public static partial class Target
	{
		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a parameterless lambda expression 
		/// which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		public static ITarget ForExpression<TResult>(Expression<Func<TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}

		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a lambda expression which takes a
		/// <see cref="ResolveContext" /> and which returns
		/// an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static ITarget ForExpression<TResult>(Expression<Func<ResolveContext, TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}
		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a lambda expression which takes 1 argument
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static ITarget ForExpression<T1, TResult>(Expression<Func<T1, TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}
		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a lambda expression which takes 2 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static ITarget ForExpression<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}
		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a lambda expression which takes 3 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static ITarget ForExpression<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}
		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a lambda expression which takes 4 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static ITarget ForExpression<T1, T2, T3, T4, TResult>(Expression<Func<T1, T2, T3, T4, TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}
		/// <summary>Creates an <see cref="Rezolver.Targets.ExpressionTarget" /> for a lambda expression which takes 5 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the lambda expression.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="lambda">Required.  The lambda expression that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static ITarget ForExpression<T1, T2, T3, T4, T5, TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> lambda, Type declaredType = null)
		{
			if(lambda == null) throw new ArgumentNullException(nameof(lambda));
			return new ExpressionTarget(lambda, declaredType);
		}
	}
} // namespace Rezolver.Targets

namespace Rezolver
{
	using System;
	using System.Linq.Expressions;
	using Rezolver.Targets;

	public static partial class ExpressionTargetContainerExtensions
	{
		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a parameterless lambda expression 
		/// which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterExpression<TResult>(this ITargetContainer targets, Expression<Func<TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}

		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a lambda expression which takes a 
		/// <see cref="ResolveContext" /> and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static void RegisterExpression<TResult>(this ITargetContainer targets, Expression<Func<ResolveContext, TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}
		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a lambda expression which takes 1 argument
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static void RegisterExpression<T1, TResult>(this ITargetContainer targets, Expression<Func<T1, TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}
		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a lambda expression which takes 2 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static void RegisterExpression<T1, T2, TResult>(this ITargetContainer targets, Expression<Func<T1, T2, TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}
		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a lambda expression which takes 3 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static void RegisterExpression<T1, T2, T3, TResult>(this ITargetContainer targets, Expression<Func<T1, T2, T3, TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}
		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a lambda expression which takes 4 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static void RegisterExpression<T1, T2, T3, T4, TResult>(this ITargetContainer targets, Expression<Func<T1, T2, T3, T4, TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}
		/// <summary>Registers an <see cref="Rezolver.Targets.ExpressionTarget" /> built from a lambda expression which takes 5 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st parameter of the lambda expression.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the lambda expression.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the lambda expression.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the lambda expression.</typeparam>
		/// <typeparam name="TResult">The return type of the lambda expression.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="lambda">Required.  The lambda expression which is to be compiled and executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
		/// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		/// <remarks>All arguments to the lambda are injected from the container when compiled and executed</remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, TResult>(this ITargetContainer targets, Expression<Func<T1, T2, T3, T4, T5, TResult>> lambda, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterExpression((Expression)lambda, declaredType, scopeBehaviour);
		}
	}
}

