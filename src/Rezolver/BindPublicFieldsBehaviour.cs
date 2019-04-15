// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rezolver.Compilation;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IMemberBindingBehaviour"/> which binds all public instance fields
    /// on new instances to services from the container.
    /// </summary>
    /// <remarks>
    /// See the [member binding guide](/developers/docs/member-injection/index.html) for more.
    /// </remarks>
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
