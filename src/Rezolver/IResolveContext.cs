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
        /// Gets the <see cref="IResolveContext"/> from which this context was created, if any.
        /// </summary>
        IResolveContext Previous { get; }

        /// <summary>
        /// Gets the type being requested from the container via a call to its <see cref="IContainer.Resolve(IResolveContext)"/> (or 
        /// similar) method(s).
        /// </summary>
        /// <remarks>During any given <see cref="IContainer.Resolve(IResolveContext)"/> (or similar) operation, this
        /// type will not necessarily equal the type being constructed at that time.
        /// 
        /// For example, if you inject the context into a constructor (enabled by default via the <see cref="Configuration.InjectResolveContext"/>
        /// configuration) and instrospect it, this type will only equal a service against which that type is registered *if* that object
        /// is being created directly for that resolve operation.
        /// 
        /// If it is being created in order to be injected into another object, then this property is more likely to equal that type's registered
        /// service type.</remarks>
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
		/// Resolves a new instance of a different type from the same scope/container that originally
		/// received the current Resolve operation.
		/// </summary>
		/// <param name="newRequestedType">New type to be resolved.</param>
		/// <remarks>Use this method, or the generic equivalent, to resolve dependency services in a 
		/// factory or expression using the same container that's serving the current resolve operation.
		/// 
		/// If a scope is active then it will be honoured.</remarks>
        object Resolve(Type newRequestedType);

        
        /// <summary>
		/// Resolves a new instance of a different type from the same scope/container that originally
		/// received the current Resolve operation.  This is a deliberate mirror of the same method
        /// on <see cref="IContainer"/>.
		/// </summary>
		/// <typeparam name="TResult">New type to be resolved.</typeparam>
		/// <remarks>Use this method, or the non-generic equivalent, to resolve dependency services in a 
		/// factory or expression.
		/// 
		/// If a scope is active then it will be honoured.</remarks>
        TResult Resolve<TResult>();

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
        bool TryResolve(Type newRequestedType, out object result);

        /// <summary>
        /// Generic equivalent of <see cref="TryResolve(Type, out object)"/>
        /// </summary>
        /// <typeparam name="TResult">The type to be resolved.</typeparam>
        /// <param name="result">Receives the result of a successful resolve operation.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        bool TryResolve<TResult>(out TResult result);

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
        IResolveContext New(Type newRequestedType = null,
            IContainer newContainer = null,
            IContainerScope newScope = null);
    }

    internal static class ResolveContextExtensions
    {
        internal static object Resolve(this IResolveContext context, ITarget target, Func<IResolveContext, object> factory, ScopeBehaviour behaviour)
        {
            return context.Scope.Resolve(context, target, factory, behaviour);
        }
    }

}