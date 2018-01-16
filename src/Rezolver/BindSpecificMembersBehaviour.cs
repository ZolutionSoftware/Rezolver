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
    public class BindSpecificMembersBehaviour : BindAllMembersBehaviour
    {
        // required because members don't, apparently, hash the same in some versions of .Net
        // in .Net Standard 1.1 there is no method handle, and some of our functionality fails
        // when relying on two instances of MemberInfo being the same when they point to the same
        // member on the same type, but obtained in different ways.
        private struct MemberInfoKey : IEquatable<MemberInfoKey>
        {
            public MemberInfo Member { get; }
            public MemberInfoKey(MemberInfo member)
            {
                Member = member;
            }

            public bool Equals(MemberInfoKey other)
            {
                return Member == other.Member ||
                    (Member.DeclaringType == other.Member.DeclaringType && Member.Name == other.Member.Name);
            }

            public override bool Equals(object obj)
            {
                if(obj is MemberInfoKey key)
                    return Equals(key);
                return false;
            }

            public override int GetHashCode()
            {
                //cheap and nasty
                return Member.DeclaringType.GetHashCode() ^ Member.Name.GetHashCode();
            }
        }
        private Dictionary<MemberInfoKey, MemberBinding> _members;
        /// <summary>
        /// Constructs a new instance of the <see cref="BindSpecificMembersBehaviour"/> which will auto-bind only those
        /// members which are passed in the <paramref name="members"/> enumerable.
        /// </summary>
        /// <param name="members">An enumerable of members to be bound on new instances.  When an instance is created, those
        /// members will be auto-bound by resolving instances of the associated member types.</param>
        public BindSpecificMembersBehaviour(IEnumerable<MemberInfo> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            _members = members.ToDictionary(m => new MemberInfoKey(m), m => new MemberBinding(m));
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="BindSpecificMembersBehaviour"/> which will apply only those bindings
        /// which are passed in the <paramref name="memberBindings"/> enumerable.
        /// </summary>
        /// <param name="memberBindings">An enumerable of bindings to be applied to members of newly created objects.</param>
        public BindSpecificMembersBehaviour(IEnumerable<MemberBinding> memberBindings)
        {
            if (memberBindings == null)
                throw new ArgumentNullException(nameof(memberBindings));

            _members = memberBindings.ToDictionary(b => new MemberInfoKey(b.Member));
        }

        /// <summary>
        /// Overrides the base method to return <c>true</c> only for those members which were supplied on construction.
        /// </summary>
        /// <param name="pi">A property that is potentially to be bound.</param>
        /// <returns></returns>
        protected override bool ShouldBind(PropertyInfo pi)
        {
            return _members.ContainsKey(new MemberInfoKey(pi));
        }

        /// <summary>
        /// Overrides the base method to return <c>true</c> only for those members which were supplied on construction.
        /// </summary>
        /// <param name="fi">A field that is potentially to be bound.</param>
        /// <returns></returns>
        protected override bool ShouldBind(FieldInfo fi)
        {
            return _members.ContainsKey(new MemberInfoKey(fi));
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
            if (_members.TryGetValue(new MemberInfoKey(field), out var binding))
                return binding;

            return null;
        }

        /// <summary>
        /// Overrides the base to return bindings only for those members that were passed on construction.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">The type whose members are being bound (note this could be a type that's
        /// derived from the one on which the property identified by <paramref name="prop"/> is declared.</param>
        /// <param name="prop">The property being bound.</param>
        /// <returns>The binding to be applied to the passed <paramref name="prop"/> if it's a known
        /// property, otherwise <c>null</c></returns>
        protected override MemberBinding CreateBinding(ICompileContext context, Type type, PropertyInfo prop)
        {
            if (_members.TryGetValue(new MemberInfoKey(prop), out var binding))
                return binding;

            return null;
        }
    }
}
