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
    /// 
    /// </summary>
    public sealed class ResolveContext
    {
        /// <summary>
        /// Gets the type being requested from the container.
        /// </summary>
        public Type RequestedType { get; private set; }

        /// <summary>
        /// The container for this context.
        /// </summary>
        /// <remarks>This is a wrapper for the <see cref="ContainerScope2.Container"/> property of the <see cref="Scope"/></remarks>
        public Container Container => Scope.Container;

        /// <summary>
        /// Gets the scope that's active for all calls for this context.
        /// </summary>
        /// <value>The scope.</value>
        public ContainerScope2 Scope { get; private set; }

        /// <summary>
        /// Creates a new context which is initially a clone of the one passed, but with
        /// <see cref="Previous"/> set to the one passed.
        /// </summary>
        /// <param name="previous"></param>
        private ResolveContext(ResolveContext previous)
        {
            RequestedType = previous.RequestedType;
            Scope = previous.Scope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="requestedType">The type of object to be resolved from the container.</param>
        public ResolveContext(Container container, Type requestedType)
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
        public ResolveContext(ContainerScope2 scope, Type requestedType)
        {
            Scope = scope;
            RequestedType = requestedType;
        }

        private ResolveContext(Container container)
        {
            // if the container is a scoped container, then we pull it out and set it into this context.
            Scope = Container.Scope;
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

        /// <summary>
        /// Mirror of the <see cref="IContainer.TryResolve(ResolveContext, out object)"/> method
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
            return (Scope?.Container ?? Container).TryResolve(New(newRequestedType: newRequestedType), out result);
        }

        /// <summary>
        /// Generic equivalent of <see cref="TryResolve(Type, out object)"/>
        /// </summary>
        /// <typeparam name="TResult">The type to be resolved.</typeparam>
        /// <param name="result">Receives the result of a successful resolve operation.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        public bool TryResolve<TResult>(out TResult result)
        {
            bool success = TryResolve(typeof(TResult), out object temp);
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
        /// <param name="newScope">Optional - a new scope to be used for the new context.  If a new context
        /// is created, then its <see cref="Scope"/> will be inherited from this context unless a non-null
        /// container is passed to this parameter.  Note the implication: once a context has a non-null <see cref="Scope"/>,
        /// it's not possible to create a new, child, context which has a null scope.</param>
        /// <returns></returns>
        public ResolveContext New(
            Type newRequestedType = null,
            ContainerScope2 newScope = null)
        {
            return new ResolveContext(newScope ?? Scope, newRequestedType ?? RequestedType);
        }

        public ResolveContext New(Type newRequestedType)
        {
            if (newRequestedType != RequestedType)
                return new ResolveContext(Scope, newRequestedType);

            return this;
        }

        public ResolveContext New(Container newContainer)
        {
            // we have to create a new 'fake' scope which proxies the current one,
            // but with a different container.
            if (newContainer != Container)
                return new ResolveContext(new ContainerScopeProxy(Scope, newContainer), RequestedType);

            return this;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            List<string> parts = new List<string>
            {
                $"Type: {RequestedType}",
                $"Container: {Container}"
            };
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
        public ContainerScope2 CreateScope()
        {
            return Scope.CreateScope();
        }
    }
}
