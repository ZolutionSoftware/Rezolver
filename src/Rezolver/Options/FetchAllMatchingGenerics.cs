using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    /// <summary>
    /// A boolean option for target containers that controls which registrations will be returned
    /// when <see cref="ITargetContainer.FetchAll(Type)"/> is called with a closed generic type, when using the 
    /// built in target containers.
    /// 
    /// This ultimately affects the objects that are materialised in automatically generated enumerables
    /// (when the <see cref="Behaviours.AutoEnumerableConfig"/> is enabled - which it is, by default)
    /// when the element type is itself a generic type.
    /// </summary>
    /// <remarks>This option is primarily used to control the <see cref="GenericTargetContainer"/>
    /// and the targets it returns from its implementation of <see cref="ITargetContainer.FetchAll(Type)"/>
    /// where the input type is a closed generic.
    /// 
    /// When <c>true</c> (the <see cref="Default"/>), all targets which match **both** the closed generic
    /// ***and*** which have been registered for all applicable open generics will be returned.
    /// 
    /// When <c>false</c> then the function will return only the first matching group of targets sharing 
    /// a common matching type.  So, if there are five registered against the open generic, and one against 
    /// the closed generic; then the one will 'beat' the five.</remarks>
    public class FetchAllMatchingGenerics : ContainerOption<bool>
    {
        /// <summary>
        /// The default value for the <see cref="FetchAllMatchingGenerics"/> option - equivalent to <c>true</c>.
        /// </summary>
        public static FetchAllMatchingGenerics Default { get; } = true;

        /// <summary>
        /// Implicit conversion operator to <see cref="FetchAllMatchingGenerics"/> from <see cref="bool"/> to simplify 
        /// reading and writing the option as a boolean.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator FetchAllMatchingGenerics(bool value)
        {
            return new FetchAllMatchingGenerics() { Value = value };
        }
    }
}
