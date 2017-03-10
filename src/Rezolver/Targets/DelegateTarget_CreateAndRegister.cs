// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information



namespace Rezolver
{
	using System;
	using Rezolver.Targets;

	public static partial class Target
	{
		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a parameterless factory delegate 
		/// which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		public static ITarget ForDelegate<TResult>(Func<TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}

		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a factory delegate which takes a 
		/// <see cref="ResolveContext" /> and which returns
		/// an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the delegate are injected from the container when executed</remarks>
		public static ITarget ForDelegate<TResult>(Func<ResolveContext, TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}
		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a factory delegate which takes 1 argument
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the delegate are injected from the container when executed</remarks>
		public static ITarget ForDelegate<T1, TResult>(Func<T1, TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}
		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a factory delegate which takes 2 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the delegate are injected from the container when executed</remarks>
		public static ITarget ForDelegate<T1, T2, TResult>(Func<T1, T2, TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}
		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a factory delegate which takes 3 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="T3">Type of the 3rd delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the delegate are injected from the container when executed</remarks>
		public static ITarget ForDelegate<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}
		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a factory delegate which takes 4 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="T3">Type of the 3rd delegate parameter</typeparam>
		/// <typeparam name="T4">Type of the 4th delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the delegate are injected from the container when executed</remarks>
		public static ITarget ForDelegate<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}
		/// <summary>Creates a <see cref="Rezolver.Targets.DelegateTarget" /> for a factory delegate which takes 5 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="T3">Type of the 3rd delegate parameter</typeparam>
		/// <typeparam name="T4">Type of the 4th delegate parameter</typeparam>
		/// <typeparam name="T5">Type of the 5th delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="factory">Required.  The factory delegate that is to be wrapped by the target.</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created,
		/// if different from <typeparamref name="TResult" /></param>
		/// <remarks>All arguments to the delegate are injected from the container when executed</remarks>
		public static ITarget ForDelegate<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> factory, Type declaredType = null)
		{
			if(factory == null) throw new ArgumentNullException(nameof(factory));
			return new DelegateTarget(factory, declaredType);
		}
	}
} // namespace Rezolver.Targets

namespace Rezolver
{
	using System;
	using Rezolver.Targets;

	public static partial class DelegateTargetContainerExtensions
	{
		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a parameterless factory delegate 
		/// which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<TResult>(this ITargetContainer targets, Func<TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}

		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a factory delegate which takes a 
		/// <see cref="ResolveContext" /> and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<TResult>(this ITargetContainer targets, Func<ResolveContext, TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}
		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a factory delegate which takes 1 argument
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<T1, TResult>(this ITargetContainer targets, Func<T1, TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}
		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a factory delegate which takes 2 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<T1, T2, TResult>(this ITargetContainer targets, Func<T1, T2, TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}
		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a factory delegate which takes 3 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="T3">Type of the 3rd delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<T1, T2, T3, TResult>(this ITargetContainer targets, Func<T1, T2, T3, TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}
		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a factory delegate which takes 4 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="T3">Type of the 3rd delegate parameter</typeparam>
		/// <typeparam name="T4">Type of the 4th delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<T1, T2, T3, T4, TResult>(this ITargetContainer targets, Func<T1, T2, T3, T4, TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}
		/// <summary>Registers a <see cref="Rezolver.Targets.DelegateTarget" /> built from a factory delegate which takes 5 arguments
		/// and which returns an instance of <typeparamref name="TResult" /></summary>
		/// <typeparam name="T1">Type of the 1st delegate parameter</typeparam>
		/// <typeparam name="T2">Type of the 2nd delegate parameter</typeparam>
		/// <typeparam name="T3">Type of the 3rd delegate parameter</typeparam>
		/// <typeparam name="T4">Type of the 4th delegate parameter</typeparam>
		/// <typeparam name="T5">Type of the 5th delegate parameter</typeparam>
		/// <typeparam name="TResult">The type of the object produced by the factory delegate.</typeparam>
		/// <param name="targets">Required.  The <see cref="ITargetContainer" /> into which the newly created target will be registered</param>
		/// <param name="factory">Required.  The factory delegate which is to be executed when an instance is resolved by a container</param>
		/// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType" /> of the target to be created
		/// if different from <typeparamref name="TResult" />.  Also overrides the type against which the registration will be made.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
		public static void RegisterDelegate<T1, T2, T3, T4, T5, TResult>(this ITargetContainer targets, Func<T1, T2, T3, T4, T5, TResult> factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
		{
			targets.RegisterDelegate((Delegate)factory, declaredType, scopeBehaviour);
		}
	}
}

