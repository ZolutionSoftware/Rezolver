// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Captures the state for a call to <see cref="IContainer.Resolve(ResolveContext)"/> 
	/// (or <see cref="IContainer.TryResolve(ResolveContext, out object)"/>), including the container on which
	/// the operation is invoked, any <see cref="IScopedContainer"/> that might be active for the call (if different), and the 
	/// type which is being resolved from the <see cref="IContainer"/>.
	/// </summary>
	public class ResolveContext : IScopeFactory
	{
		/// <summary>
		/// Gets a comparer for <see cref="ResolveContext"/> which treats two contexts as being equal
		/// if they're both the same reference (including null) or, if both have the same <see cref="RequestedType"/>
		/// </summary>
		public static IEqualityComparer<ResolveContext> RequestedTypeComparer { get; } = new RequestedTypeEqualityComparer();
		/// <summary>
		/// An equality comparer for ResolveContext which treats two contexts
		/// as equal if they are both null, or have the same <see cref="ResolveContext.RequestedType"/>.
		/// </summary>
		/// <seealso cref="System.Collections.Generic.IEqualityComparer{T}" />
		internal class RequestedTypeEqualityComparer : IEqualityComparer<ResolveContext>
		{
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
			/// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
			public bool Equals(ResolveContext x, ResolveContext y)
			{
				return x == y || x?.RequestedType == y?.RequestedType;
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
			public int GetHashCode(ResolveContext obj)
			{
				return obj?.RequestedType?.GetHashCode() ?? 0;
			}
		}
		/// <summary>
		/// Gets the type being requested from the container
		/// </summary>
		/// <value>The type of the requested.</value>
		public Type RequestedType { get; private set; }

		/// <summary>
		/// The container for this context.
		/// </summary>
		/// <remarks>This is the container which received the original call to <see cref="IContainer.Resolve(ResolveContext)"/>,
		/// but is not necessarily the same container that will eventually end up resolving the object.</remarks>
		public IContainer Container { get; private set; }

		/// <summary>
		/// Gets the scope that's active for all calls for this context.
		/// </summary>
		/// <value>The scope.</value>
		public IContainerScope Scope { get; private set; }

		private ResolveContext() { }

		//TODO: Need to think about potentially tracking parent/child relationships as new contexts are spawned from others
		//      So that conditional targets can be implemented.  At the moment, you can only see which type(s) might be requesting
		//		an instance during compilation.

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolveContext"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		public ResolveContext(IContainer container, Type requestedType)
		  : this(container)
		{
			RequestedType = requestedType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolveContext"/> class from the given scope.
		/// 
		/// The <see cref="Container"/> is inherited from the scope's <see cref="IContainerScope.Container"/>.
		/// </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="requestedType">The of object to be resolved from the container.</param>
		public ResolveContext(IContainerScope scope, Type requestedType)
		{
			Scope = scope;
			Container = scope.Container;
			RequestedType = requestedType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolveContext"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		/// <param name="scope">The scope for this context.</param>
		public ResolveContext(IContainer container, Type requestedType, IContainerScope scope)
		  : this(container)
		{
			RequestedType = requestedType;
			Scope = scope;
		}

		private ResolveContext(IContainer container)
		{
			Container = container ?? StubContainer.Instance;
		}

		/// <summary>
		/// Resolves a new instance of a different type from the same scope/container that originally
		/// received the current Resolve operation.
		/// </summary>
		/// <param name="newRequestedType">New type to be resolved.</param>
		/// <remarks>Use this method, or the generic equivalent, to resolve dependency services in a 
		/// factory or expression.
		/// 
		/// If a scope is active then it will be honoured.</remarks>
		public object Resolve(Type newRequestedType)
		{
			return (Scope?.Container ?? Container).Resolve(CreateNew(newRequestedType));
		}

		/// <summary>
		/// Resolves a new instance of a different type from the same scope/container that originally
		/// received the current Resolve operation.
		/// </summary>
		/// <typeparam name="TResult">New type to be resolved.</typeparam>
		/// <remarks>Use this method, or the non-generic equivalent, to resolve dependency services in a 
		/// factory or expression.
		/// 
		/// If a scope is active then it will be honoured.</remarks>
		public TResult Resolve<TResult>()
		{
			return (TResult)Resolve(typeof(TResult));
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the type.
		/// </summary>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		internal ResolveContext CreateNew(Type requestedType)
		{
			return new ResolveContext()
			{
				Container = Container,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="Container"/>.
		/// </summary>
		/// <param name="container">The container for the new context.</param>
		internal ResolveContext CreateNew(IContainer container)
		{
			return new ResolveContext()
			{
				Container = container,
				RequestedType = RequestedType,
				Scope = Scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="Scope"/>.
		/// </summary>
		/// <param name="scope">The scope for the new context.</param>
		internal ResolveContext CreateNew(IContainerScope scope)
		{
			return new ResolveContext()
			{
				Container = Container,
				RequestedType = RequestedType,
				Scope = scope
			};
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		public override string ToString()
		{
			List<string> parts = new List<string>();

			parts.Add($"Type: {RequestedType}");
			parts.Add($"Container: {Container}");
			if (Scope != null)
			{
				parts.Add($"Scope: {Scope}");
			}

			return $"({string.Join(", ", parts)})";
		}

		/// <summary>
		/// Creates a new scope either through the <see cref="Scope"/> or, if that's null, then the <see cref="Container"/>.
		/// </summary>
		IContainerScope IScopeFactory.CreateScope()
		{
			return (((IScopeFactory)Scope) ?? Container).CreateScope();
		}
	}
}
