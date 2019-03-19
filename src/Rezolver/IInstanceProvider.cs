namespace Rezolver
{
    /// <summary>
    /// Interface for an object (usually a <see cref="ITarget"/> implementation) which can provide an instance
    /// for a given <see cref="ResolveContext"/> without needing to be compiled.
    /// </summary>
    public interface IInstanceProvider
    {
        /// <summary>
        /// Method to call to get an instance for a particular <see cref="ResolveContext"/>
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The object</returns>
        object GetInstance(ResolveContext context);
    }

    /// <summary>
    /// Strongly-typed version of <see cref="IInstanceProvider"/>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public interface IInstanceProvider<out TService>
    {
        /// <summary>
        /// Gets an instance of <typeparamref name="TService"/> 
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>An instance of <typeparamref name="TService"/></returns>
        TService GetInstance(ResolveContext context);
    }
}
