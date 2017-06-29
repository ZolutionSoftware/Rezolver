namespace Rezolver.Options
{
    /// <summary>
    /// Boolean option which, if configured before the <see cref="Configuration.AutoLists"/> configuration is applied, 
    /// will control whether automatic injection of <see cref="System.Collections.Generic.List{T}"/> and <see cref="System.Collections.Generic.IList{T}"/>
    /// will be enabled (most commonly piggybacking off of the behaviour that's enabled by the <see cref="Configuration.AutoEnumerables"/> configuration).
    /// 
    /// The <see cref="Default"/> is equivalent to <c>true</c>.
    /// </summary>
    /// <remarks>IMPORTANT - Although this option is related to the <see cref="EnableAutoEnumerable"/> option, the two are independent.
    /// 
    /// If you disable automatic enumerable injection, it does not automatically disable automatic list injection.</remarks>
    public class EnableAutoLists : ContainerOption<bool>
    {
        /// <summary>
        /// Default value for this option, equivalent to <c>true</c>
        /// </summary>
        public static EnableAutoLists Default { get; } = true;

        /// <summary>
        /// Convenience operator for creating an instance of this option from a boolean.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableAutoLists(bool value)
        {
            return new EnableAutoLists() { Value = value };
        }
    }
}