using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IMemberBindingBehaviour"/> which binds all non-indexer publicly writeable instance 
    /// properties on new instances to services from the container.
    /// </summary>
    public class BindPublicPropertiesBehaviour : BindAllMembersBehaviour
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BindPublicPropertiesBehaviour"/> class
        /// 
        /// Can only be created by Rezolver or through inheritance.
        /// </summary>
        protected internal BindPublicPropertiesBehaviour()
        {

        }

        /// <summary>
        /// Overrides the base class to avoid returning any fields.
        /// </summary>
        /// <param name="context">The compile context</param>
        /// <param name="type">The type to be bound.</param>
        /// <returns>Always returns an empty <see cref="IEnumerable{FieldInfo}"/></returns>
        protected sealed override IEnumerable<FieldInfo> GetBindableFields(ICompileContext context, Type type)
        {
            return Enumerable.Empty<FieldInfo>();
        }
    }
}
