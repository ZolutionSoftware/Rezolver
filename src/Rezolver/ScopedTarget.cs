// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// A target that produces a single instance of an object within a lifetime scope.
	/// </summary>
	/// <remarks>On its own, this target doesn't do anything - it's designed to wrap another target such that
	/// the code generated from the expression it produces is executed only once for each lifetime scope.
	/// 
	/// Outside of that, the target generates wrapper code that forcibly caches the instance that is produced (whether
	/// it's IDiposable or not) into the current scope's cache (using <see cref="IScopedContainer.AddToScope(object, RezolveContext)"/>)
	/// and retrieves previous instances from that scope (using <see cref="IScopedContainer.GetFromScope(RezolveContext)"/>.</remarks>
	public class ScopedTarget : TargetBase
	{
		public ITarget InnerTarget { get; }

		public override Type DeclaredType
		{
			get { return InnerTarget.DeclaredType; }
		}

		protected override bool SuppressScopeTracking
		{
			get
			{
				return true;
			}
		}

		public ScopedTarget(ITarget innerTarget)
		{
			innerTarget.MustNotBeNull("innerTarget");
			InnerTarget = innerTarget;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{

		}

		public override bool SupportsType(Type type)
		{
			return InnerTarget.SupportsType(type);
		}
	}

	/// <summary>
	/// Extension method(s) to convert targets into scoped targets.
	/// </summary>
	public static class IRezolveTargetScopingExtensions
	{
		/// <summary>
		/// Creates a <see cref="ScopedTarget"/> from the target on which this method is invoked.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static ScopedTarget Scoped(this ITarget target)
		{
			target.MustNotBeNull(nameof(target));
			return new ScopedTarget(target);
		}
	}
}
