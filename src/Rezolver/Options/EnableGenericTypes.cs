namespace Rezolver.Options
{
    /// <summary>
    /// Container options which controls whether the <see cref="Configuration.GenericTypes"/> configuration 
    /// is enabled on a <see cref="TargetContainer"/> - which provides all of the documented generic type 
    /// functionality (including mapping open generics to closed generics at runtime, and any other 
    /// 'clever' behaviour associated with generics.  
    /// 
    /// The <see cref="Default"/> is <c>true</c> - and, in general, there should never be any need to switch
    /// it off.
    /// </summary>
    public class EnableGenericTypes : ContainerOption<bool>
    {
        /// <summary>
        /// The default value for this option - is equivalent to <c>true</c> - meaning that
        /// Rezolver's internal handling of generics is enabled by default.
        /// </summary>
        public static EnableGenericTypes Default = true;

        /// <summary>
        /// Convenience implicit conversion operator between the option value type (<see cref="bool"/>)
        /// and a new instance of <see cref="EnableGenericTypes"/>.
        /// </summary>
        /// <param name="value">The boolean value that is to be captured in a new insteance of the 
        /// <see cref="EnableGenericTypes"/> option.</param>
        public static implicit operator EnableGenericTypes(bool value)
        {
            return new EnableGenericTypes() { Value = value };
        }
    }
}