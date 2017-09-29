namespace Rezolver.Options
{
    /// <summary>
    /// Boolean option which, if configured before the <see cref="Configuration.InjectLists"/> configuration is applied, 
    /// will control whether automatic injection of <see cref="System.Collections.Generic.List{T}"/> (and related interfaces - see the documentation 
    /// on that type for more) will be enabled.
    /// The <see cref="Default"/> is equivalent to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// The injection behaviour controlled by this option most commonly piggybacks off of the behaviour that's enabled by the 
    /// <see cref="Configuration.InjectEnumerables"/> configuration.
    /// 
    /// However, although this option is therefore related to the <see cref="EnableEnumerableInjection"/> option, the two are independent.
    /// 
    /// If you disable automatic enumerable injection, it does not automatically disable automatic list injection.</remarks>
    /// <seealso cref="Configuration.InjectLists"/>
    /// <seealso cref="EnableEnumerableInjection"/>
    /// <seealso cref="Configuration.InjectEnumerables"/>
    public class EnableListInjection : ContainerOption<bool>
    {
        /// <summary>
        /// Default value for this option, equivalent to <c>true</c>
        /// </summary>
        public static EnableListInjection Default { get; } = true;

        /// <summary>
        /// Convenience operator for creating an instance of this option from a boolean.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableListInjection(bool value)
        {
            return new EnableListInjection() { Value = value };
        }
    }
}