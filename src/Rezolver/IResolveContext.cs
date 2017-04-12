using System;

namespace Rezolver
{
    //public interface IResolve
    //{
    //    object Resolve(Type type);
    //    TResult Resolve<TResult>();
    //}

    //public interface IResolveByContext
    //{
    //    object Resolve(IResolveContext context);
    //    TResult Resolve<TResult>(IResolveContext context);
    //}

    //public interface ICanResolve
    //{
    //    bool CanResolve(Type type);
    //    bool CanResolve<TResult>();
    //}

    //public interface ICanResolveByContext
    //{
    //    bool CanResolve(IResolveContext context);
    //}

    //public interface ITryResolve
    //{
    //    bool TryResolve(Type type, out object result);
    //    bool TryResolve<TResult>(out TResult result);
    //}

    //public interface ITryResolveByContext
    //{
    //    bool TryResolve(IResolveContext context, out object result);
    //    bool TryResolve<TResult>(IResolveContext context, out TResult result);
    //}

    /// <summary>
	/// Captures the state for a call to <see cref="IContainer.Resolve(IResolveContext)"/> 
	/// (or <see cref="IContainer.TryResolve(IResolveContext, out object)"/>), including the container on which
	/// the operation is invoked, any <see cref="IScopedContainer"/> that might be active for the call (if different), and the 
	/// type that is being resolved from the <see cref="IContainer"/>.
	/// </summary>
    /// <remarks>The context implements <see cref="IScopeFactory"/> and correctly handles whether a new child scope should
    /// be created either from the <see cref="Scope"/> or the <see cref="Container"/>.</remarks>
    public interface IResolveContext : IScopeFactory
    {
        /// <summary>
        /// Gets thee <see cref="IResolveContext"/> from which this context was created, if any.
        /// </summary>
        IResolveContext Previous { get; }

        /// <summary>
        /// Gets the type being requested from the container
        /// </summary>
        Type RequestedType { get; }

        /// <summary>
		/// The container for this context.
		/// </summary>
		/// <remarks>This is the container which received the original call to <see cref="IContainer.Resolve(IResolveContext)"/>,
		/// but is not necessarily the same container that will eventually end up resolving the object.</remarks>
        IContainer Container { get; }

        /// <summary>
		/// Gets the scope that's active for all calls for this context.
		/// </summary>
		/// <value>The scope.</value>
        IContainerScope Scope { get; }

        /// <summary>
        /// Provides a reference to the type of the instance that is currently being created by compiled targets
        /// within a container (if known).
        /// </summary>
        /// <remarks></remarks>
        Type TypeBeingCreated { get; }

        /// <summary>
		/// Resolves a new instance of a different type from the same scope/container that originally
		/// received the current Resolve operation.
		/// </summary>
		/// <param name="newRequestedType">New type to be resolved.</param>
		/// <remarks>Use this method, or the generic equivalent, to resolve dependency services in a 
		/// factory or expression.
		/// 
		/// If a scope is active then it will be honoured.</remarks>
        object Resolve(Type newRequestedType);

        
        /// <summary>
		/// Resolves a new instance of a different type from the same scope/container that originally
		/// received the current Resolve operation.
		/// </summary>
		/// <typeparam name="TResult">New type to be resolved.</typeparam>
		/// <remarks>Use this method, or the non-generic equivalent, to resolve dependency services in a 
		/// factory or expression.
		/// 
		/// If a scope is active then it will be honoured.</remarks>
        TResult Resolve<TResult>();

        bool TryResolve(Type newRequestedType, out object result);

        bool TryResolve<TResult>(out TResult result);

        /// <summary>
        /// Creates a new context from this one, typically with at least one of the properties
        /// changed according to the parameters you pass.
        /// 
        /// Note that if none of the parameters are provided; or if none of the parameters have different
        /// values from those already on the properties of the context, then the method can return
        /// the same instance on which it is called.
        /// </summary>
        /// <param name="newRequestedType"></param>
        /// <param name="newContainer"></param>
        /// <param name="newScope"></param>
        /// <returns></returns>
        IResolveContext New(Type newRequestedType = null,
            IContainer newContainer = null,
            IContainerScope newScope = null);

        IResolveContext SetTypeBeingCreated(Type type);
    }

    internal static class ResolveContextExtensions
    {
        internal static object Resolve(this IResolveContext context, Func<IResolveContext, object> factory, ScopeBehaviour behaviour)
        {
            return context.Scope.Resolve(context, factory, behaviour);
        }
    }

}