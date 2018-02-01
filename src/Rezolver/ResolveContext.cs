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
        /// <summary>
        /// A reference to the context from which this one was created.
        /// </summary>
        public IResolveContext Previous { get; private set; }

        /// <summary>
        /// Gets the type being requested from the container.
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
            this.Previous = previous;
            this.RequestedType = previous.RequestedType;
            this.Container = previous.Container;
            this.Scope = previous.Scope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="requestedType">The type of object to be resolved from the container.</param>
        public ResolveContext(IContainer container, Type requestedType)
          : this(container)
        {
            this.RequestedType = requestedType;
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
            this.Scope = scope;
            this.Container = scope.Container;
            this.RequestedType = requestedType;
        }

        private ResolveContext(IContainer container)
        {
            this.Container = container ?? StubContainer.Instance;
            // if the container is a scoped container, then we pull it out and set it into this context.
            this.Scope = (container as IScopedContainer)?.Scope;
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
            return (this.Scope?.Container ?? this.Container).Resolve(this.New(newRequestedType: newRequestedType));
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
            return (TResult)this.Resolve(typeof(TResult));
        }

        /// <summary>
        /// Mirror of the <see cref="IContainer.TryResolve(IResolveContext, out object)"/> method
        /// which works directly off this resolve context - taking into account the current
        /// <see cref="Container"/> and <see cref="Scope"/>
        /// </summary>
        /// <param name="newRequestedType">The type to be resolved.</param>
        /// <param name="result">Receives the result of a successful resolve operation.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        /// <remarks>Use this method, or the non-generic equivalent, to resolve dependency services in a
        /// factory or expression.
        ///
        /// If a scope is active then it will be honoured.</remarks>
        public bool TryResolve(Type newRequestedType, out object result)
        {
            return (this.Scope?.Container ?? this.Container).TryResolve(this.New(newRequestedType: newRequestedType), out result);
        }

        /// <summary>
        /// Generic equivalent of <see cref="TryResolve(Type, out object)"/>
        /// </summary>
        /// <typeparam name="TResult">The type to be resolved.</typeparam>
        /// <param name="result">Receives the result of a successful resolve operation.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        public bool TryResolve<TResult>(out TResult result)
        {
            bool success = this.TryResolve(typeof(TResult), out object temp);
            result = (TResult)temp;
            return success;
        }

        /// <summary>
        /// Creates a new context from this one, typically with at least one of the properties
        /// changed according to the parameters you pass.
        ///
        /// Note that if none of the parameters are provided; or if none of the parameters have different
        /// values from those already on the properties of the context, then the method can return
        /// the same instance on which it is called.
        /// </summary>
        /// <param name="newRequestedType">Optional - a new type to be resolved.  If a new context is created,
        /// then its <see cref="RequestedType"/> will be inherited from this context, unless a non-null type
        /// is passed to this parameter.</param>
        /// <param name="newContainer">Optional - a new container to be used for the new context.  If a new context
        /// is created, then its <see cref="Container"/> will be inherited from this context unless a non-null
        /// container is passed to this parameter.</param>
        /// <param name="newScope">Optional - a new scope to be used for the new context.  If a new context
        /// is created, then its <see cref="Scope"/> will be inherited from this context unless a non-null
        /// container is passed to this parameter.  Note the implication: once a context has a non-null <see cref="Scope"/>,
        /// it's not possible to create a new, child, context which has a null scope.</param>
        /// <returns></returns>
        public IResolveContext New(Type newRequestedType = null,
            IContainer newContainer = null,
            IContainerScope newScope = null)
        {
            ResolveContext newContext = null;

            if (newRequestedType != null && newRequestedType != this.RequestedType)
            {
                newContext = new ResolveContext(this)
                {
                    RequestedType = newRequestedType
                };
                if (newContainer != null && !Object.ReferenceEquals(newContainer, this.Container))
                {
                    newContext.Container = newContainer;
                }

                if (newScope != null && !Object.ReferenceEquals(newScope, this.Scope))
                {
                    newContext.Scope = newScope;
                }

                return newContext;
            }
            else if (newContainer != null && !Object.ReferenceEquals(newContainer, this.Container))
            {
                newContext = new ResolveContext(this)
                {
                    Container = newContainer
                };
                if (newScope != null && !Object.ReferenceEquals(newScope, this.Scope))
                {
                    newContext.Scope = newScope;
                }

                return newContext;
            }
            else if (newScope != null && !Object.ReferenceEquals(newScope, this.Scope))
            {
                newContext = new ResolveContext(this)
                {
                    Scope = newScope
                };
                return newContext;
            }

            return this;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            List<string> parts = new List<string>
            {
                $"Type: {this.RequestedType}",
                $"Container: {this.Container}"
            };
            if (this.Scope != null)
            {
                parts.Add($"Scope: {this.Scope}");
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
            return (((IScopeFactory)this.Scope) ?? this.Container).CreateScope();
        }
    }
}
