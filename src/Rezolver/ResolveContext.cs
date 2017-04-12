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
    /// Standard implementation of <see cref="Rezolver.IResolveContext"/>
    /// </summary>
	public class ResolveContext : IResolveContext
    {
        #region equality comparer (used for some of the caches)
        /// <summary>
        /// Gets a comparer for <see cref="IResolveContext"/> which treats two contexts as being equal
        /// if they're both the same reference (including null) or, if both have the same <see cref="RequestedType"/>
        /// </summary>
        public static IEqualityComparer<IResolveContext> RequestedTypeComparer { get; } = new RequestedTypeEqualityComparer();
		/// <summary>
		/// An equality comparer for ResolveContext which treats two contexts
		/// as equal if they are both null, or have the same <see cref="IResolveContext.RequestedType"/>.
		/// </summary>
		/// <seealso cref="System.Collections.Generic.IComparer{T}" />
		internal class RequestedTypeEqualityComparer : IEqualityComparer<IResolveContext>
		{
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			public bool Equals(IResolveContext x, IResolveContext y)
			{
				return x == y || x?.RequestedType == y?.RequestedType;
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object" /> for which a hash code is to be returned.</param>
			public int GetHashCode(IResolveContext obj)
			{
				return obj?.RequestedType?.GetHashCode() ?? 0;
			}
		}
        #endregion

        public IResolveContext Previous { get; private set; }

        public Type TypeBeingCreated { get; private set; }

        /// <summary>
        /// Gets the type being requested from the container
        /// </summary>
        public Type RequestedType { get; private set; }

		/// <summary>
		/// The container for this context.
		/// </summary>
		/// <remarks>This is the container which received the original call to <see cref="IContainer.Resolve(IResolveContext)"/>,
		/// but is not necessarily the same container that will eventually end up resolving the object.</remarks>
		public IContainer Container { get; private set; }

		/// <summary>
		/// Gets the scope that's active for all calls for this context.
		/// </summary>
		/// <value>The scope.</value>
		public IContainerScope Scope { get; private set; }

        /// <summary>
        /// Creates a new context which is initially a clone of the one passed, but with
        /// <see cref="Previous"/> set to the one passed.
        /// </summary>
        /// <param name="previous"></param>
        private ResolveContext(IResolveContext previous)
        {
            Previous = previous;
            RequestedType = previous.RequestedType;
            TypeBeingCreated = previous.TypeBeingCreated;
            Container = previous.Container;
            Scope = previous.Scope;
        }

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
			return (Scope?.Container ?? Container).Resolve(New(newRequestedType: newRequestedType));
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

        public bool TryResolve(Type newRequestedType, out object result)
        {
            return (Scope?.Container ?? Container).TryResolve(New(newRequestedType: newRequestedType), out result);
        }

        public bool TryResolve<TResult>(out TResult result)
        {
            object temp;
            bool success = TryResolve(typeof(TResult), out temp);
            result = (TResult)temp;
            return success;
        }

        public IResolveContext New(Type newRequestedType = null,
            IContainer newContainer = null,
            IContainerScope newScope = null)
        {
            ResolveContext newContext = null;

            if (newRequestedType != null && newRequestedType != RequestedType)
            {
                newContext = new ResolveContext(this)
                {
                    RequestedType = newRequestedType,
                    TypeBeingCreated = null
                };
                if (newContainer != null && !Object.ReferenceEquals(newContainer, Container))
                    newContext.Container = newContainer;
                if (newScope != null && !Object.ReferenceEquals(newScope, Scope))
                    newContext.Scope = newScope;
                return newContext;
            }
            else if (newContainer != null && !Object.ReferenceEquals(newContainer, Container))
            {
                newContext = new ResolveContext(this)
                {
                    Container = newContainer
                };
                if (newScope != null && !Object.ReferenceEquals(newScope, Scope))
                    newContext.Scope = newScope;
                return newContext;
            }
            else if (newScope != null && !Object.ReferenceEquals(newScope, Scope))
            {
                newContext = new ResolveContext(this)
                {
                    Scope = newScope
                };
                return newContext;
            }

            return this;
        }

        public IResolveContext SetTypeBeingCreated(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if(TypeBeingCreated == type)
            {
                return this;
            }
            else if(TypeBeingCreated != null)
            {
                return new ResolveContext(this)
                {
                    TypeBeingCreated = type
                };
            }
            TypeBeingCreated = type;
            return this;
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
        /// <remarks>This interface implementation is present for when an object wants to be able to inject a scope factory
        /// in order to create child scopes which are correctly parented either to another active scope or the container.</remarks>
		public IContainerScope CreateScope()
		{
			return (((IScopeFactory)Scope) ?? Container).CreateScope();
		}
	}
}
