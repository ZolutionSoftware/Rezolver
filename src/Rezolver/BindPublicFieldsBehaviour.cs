using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IMemberBindingBehaviour"/> which binds all public instance fields
    /// on new instances to services from the container.
    /// </summary>
    public class BindPublicFieldsBehaviour : BindAllMembersBehaviour
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BindPublicFieldsBehaviour"/> class.
        /// 
        /// Can only be created by Rezolver or through inheritance.
        /// </summary>
        protected internal BindPublicFieldsBehaviour()
        {

        }

        /// <summary>
        /// Overrides the base class to avoid returning any properties.
        /// </summary>
        /// <param name="context">The compile context</param>
        /// <param name="type">The type to be bound.</param>
        /// <returns>Always returns an empty <see cref="IEnumerable{PropertyInfo}"/></returns>
        protected sealed override IEnumerable<PropertyInfo> GetBindableProperties(ICompileContext context, Type type)
        {
            return Enumerable.Empty<PropertyInfo>();
        }
    }
}
