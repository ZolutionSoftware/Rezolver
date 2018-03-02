// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Rezolver.TypeExtensions;

namespace Rezolver
{
    /// <summary>
    /// Represents a binding (i.e. like that produced by <see cref="IMemberBindingBehaviour"/>) to a collection-like
    /// property via dynamic collection initialiser.
    /// </summary>
    public class ListMemberBinding : MemberBinding
    {
        /// <summary>
        /// Gets the element type of the enumerable (from the <see cref="MemberBinding.Target"/> whose contents will
        /// be added to the collection represented by the <see cref="MemberBinding.Member"/>.
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// Gets the method to be called on the object exposed by the <see cref="MemberBinding.Member"/> which will add
        /// elements to the collection.  Expected to be a void instance method accepting one parameter of the type
        /// <see cref="ElementType"/>
        /// </summary>
        public MethodInfo AddMethod { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListMemberBinding"/> class.
        /// </summary>
        /// <param name="member">Required. The member to be bound with a collection initialiser.</param>
        /// <param name="target">Required. The target whose elements (it must be an enumerable with element type equal
        /// to <paramref name="elementType"/>) will be added to the <paramref name="member"/> when an instance
        /// is bound.</param>
        /// <param name="elementType">Required. The type of objects to be added to the collection exposed through the
        /// <paramref name="member"/></param>
        /// <param name="addMethod">Required. The method to call on the <paramref name="member"/> that will be used
        /// to add elements to the collection when initialisation occurs.</param>
        public ListMemberBinding(MemberInfo member, ITarget target, Type elementType, MethodInfo addMethod)
        : base(member, target)
        {
            this.ElementType = elementType;
            this.AddMethod = addMethod;
        }
    }
}
