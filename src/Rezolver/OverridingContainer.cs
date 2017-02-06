// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;
using Rezolver.Targets;

namespace Rezolver
{
	/// <summary>
	/// An <see cref="IContainer"/> which can override the resolve operations of another.  This is useful when you have a 
	/// core application-wide container, with some objects being customised based on some ambient information,
	/// e.g. configuration, MVC Area/Controller, Brand (in a multi-tenant application for example) or more.
	/// 
	/// The scoping version of this is called <see cref="OverridingScopedContainer"/>.
	/// </summary>
	/// <remarks>When overriding another <see cref="IContainer"/>, you are overriding the <see cref="ICompiledTarget"/> objects that
	/// will be returned when <see cref="IContainer.FetchCompiled(ResolveContext)"/> is called on that container and, therefore,
	/// the compiled target which is executed when the <see cref="IContainer.Resolve(ResolveContext)"/> method is called.
	/// 
	/// This has the side effect of overriding automatically resolved arguments (bound to a <see cref="RezolvedTarget"/>) compiled 
	/// in the overridden container by virtue of the fact that the overriding container is a different reference, because the
	/// <see cref="RezolvedTarget"/> expects a check to be performed, at resolve-time, that the <see cref="ResolveContext.Container"/>
	/// is the same container as the one that was active when it was originally compiled.
	/// 
	/// In essence, when resolving an instance as a dependency the RezolvedTarget does something like this:
	/// 
	/// <code>resolveContext.Container == compileContext.Container ? (execute compile-time target) : resolveContext.Container.Resolve(type)</code>
	/// </remarks>
	public class OverridingContainer : Container
	{
		private readonly IContainer _inner;

		/// <summary>
		/// Creates a new instance of the <see cref="OverridingContainer"/>
		/// </summary>
		/// <param name="inner">Required.  The inner container that this one combines with.  Any dependencies not served
		/// by the new combined container's own targets will be sought from this container.  Equally, any targets in the base which
		/// are resolved when the overriding container is the root container for a resolve operation, will resolve
		/// their dependencies from this container.</param>
		/// <param name="targets">Optional. A specific target container to be used for this container's own registrations.</param>
		/// <param name="compilerConfig">Optional.  An object which will be used to configure this container and its targets to use a specific compilation
		/// strategy.  If <c>null</c> (recommended), then the <paramref name="inner"/> container's configuration will be inherited.</param>
		public OverridingContainer(IContainer inner, ITargetContainer targets = null, ICompilerConfigurationProvider compilerConfig = null)
	  : base(targets, compilerConfig ?? NoCompilerConfiguration)
		{
			inner.MustNotBeNull("inner");
			_inner = inner;
		}

		/// <summary>
		/// Called to determine if this container is able to resolve the type specified in the passed <paramref name="context"/>.
		/// </summary>
		/// <param name="context">Required.  The <see cref="ResolveContext"/>.</param>
		/// <returns></returns>
		public override bool CanResolve(ResolveContext context)
		{
			return base.CanResolve(context) || _inner.CanResolve(context);
		}

		/// <summary>
		/// Overrides the base implementation to pass the lookup for an <see cref="ITarget"/> to the inner container - this
		/// is how dependency chaining from this container to the inner container is achieved.
		/// </summary>
		/// <param name="context">Required.  The <see cref="ResolveContext"/>.</param>
		/// <returns></returns>
		protected override ICompiledTarget GetFallbackCompiledRezolveTarget(ResolveContext context)
		{
			return _inner.FetchCompiled(context);
		}
	}
}
