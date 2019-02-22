// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
    /// <summary>
    /// Overrides the <see cref="BindAllMembersBehaviour"/> to provide explicit per-member bindings.
    /// </summary>
    /// <remarks>
    /// See [the fluent API documentation](/developers/docs/member-injection/fluent-api.html) for more.
    /// </remarks>
    public class BindSpecificMembersBehaviour : BindAllMembersBehaviour
    {
        private readonly Dictionary<MemberInfoKey, MemberBinding> members;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindSpecificMembersBehaviour"/> class
        /// which will auto-bind only those members which are passed in the <paramref name="members"/> enumerable.
        /// </summary>
        /// <param name="members">An enumerable of members to be bound on new instances.  When an instance is created, those
        /// members will be auto-bound by resolving instances of the associated member types.</param>
        public BindSpecificMembersBehaviour(IEnumerable<MemberInfo> members)
        {
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            this.members = members.ToDictionary(m => new MemberInfoKey(m), m => new MemberBinding(m));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindSpecificMembersBehaviour"/> class which will apply only those bindings
        /// which are passed in the <paramref name="memberBindings"/> enumerable.
        /// </summary>
        /// <param name="memberBindings">An enumerable of bindings to be applied to members of newly created objects.</param>
        public BindSpecificMembersBehaviour(IEnumerable<MemberBinding> memberBindings)
        {
            if (memberBindings == null)
            {
                throw new ArgumentNullException(nameof(memberBindings));
            }

            this.members = memberBindings.ToDictionary(b => new MemberInfoKey(b.Member));
        }

        /// <summary>
        /// Overrides the base method to return <c>true</c> only for those members which were supplied on construction.
        /// </summary>
        /// <param name="pi">A property that is potentially to be bound.</param>
        /// <returns>A boolean indicating whether the given property is one of the members that were specified
        /// on construction</returns>
        protected override bool ShouldBind(PropertyInfo pi)
        {
            return this.members.ContainsKey(new MemberInfoKey(pi));
        }

        /// <summary>
        /// Overrides the base method to return <c>true</c> only for those members which were supplied on construction.
        /// </summary>
        /// <param name="fi">A field that is potentially to be bound.</param>
        /// <returns>A boolean indicating whether the given field is one of the members that were specified
        /// on construction.</returns>
        protected override bool ShouldBind(FieldInfo fi)
        {
            return this.members.ContainsKey(new MemberInfoKey(fi));
        }

        /// <summary>
        /// Overrides the base to return bindings only for those members that were passed on construction.
        /// </summary>
        /// <param name="context">The current compile context</param>
        /// <param name="type">The type whose members are being bound (note this could be a type that's
        /// derived from the one on which the <paramref name="field"/> is declared.</param>
        /// <param name="field">The field being bound.</param>
        /// <returns>The binding to be applied to passed <paramref name="field"/> if it's a known field,
        /// otherwise <c>null</c></returns>
        protected override MemberBinding CreateBinding(ICompileContext context, Type type, FieldInfo field)
        {
            if (this.members.TryGetValue(new MemberInfoKey(field), out var binding))
            {
                return binding;
            }

            return null;
        }

        /// <summary>
        /// Overrides the base to return bindings only for those members that were passed on construction.
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are being bound (note this could be a type that's
        /// derived from the one on which the property identified by <paramref name="prop"/> is declared.</param>
        /// <param name="prop">The property being bound.</param>
        /// <returns>The binding to be applied to the passed <paramref name="prop"/> if it's a known
        /// property, otherwise <c>null</c></returns>
        protected override MemberBinding CreateBinding(ICompileContext context, Type type, PropertyInfo prop)
        {
            if (this.members.TryGetValue(new MemberInfoKey(prop), out var binding))
            {
                return binding;
            }

            return null;
        }

        // required because members don't, apparently, hash the same in some versions of .Net
        // in .Net Standard 1.1 there is no method handle, and some of our functionality fails
        // when relying on two instances of MemberInfo being the same when they point to the same
        // member on the same type, but obtained in different ways.
        private struct MemberInfoKey : IEquatable<MemberInfoKey>
        {
            public MemberInfoKey(MemberInfo member)
            {
                Member = member;
            }

            public MemberInfo Member { get; }

            public bool Equals(MemberInfoKey other)
            {
                return Member == other.Member ||
                    (Member.DeclaringType == other.Member.DeclaringType && Member.Name == other.Member.Name);
            }

            public override bool Equals(object obj)
            {
                if (obj is MemberInfoKey key)
                {
                    return Equals(key);
                }

                return false;
            }

            public override int GetHashCode()
            {
                // cheap and nasty
                return Member.DeclaringType.GetHashCode() ^ Member.Name.GetHashCode();
            }
        }
    }
}
