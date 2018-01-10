using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
    public class BindSpecificMembersBehaviour : BindAllMembersBehaviour
    {
        //required because members don't, apparently, hash the same in some versions of .Net
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
                return Member.DeclaringType.GetHashCode() ^ Member.Name.GetHashCode();
            }
        }
        private Dictionary<MemberInfoKey, MemberBinding> _members;
        public BindSpecificMembersBehaviour(IEnumerable<MemberInfo> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            _members = members.ToDictionary(m => new MemberInfoKey(m), m => (MemberBinding)null);
        }

        public BindSpecificMembersBehaviour(IEnumerable<MemberBinding> memberBindings)
        {
            if (memberBindings == null)
                throw new ArgumentNullException(nameof(memberBindings));

            _members = memberBindings.ToDictionary(b => new MemberInfoKey(b.Member));
        }

        protected override bool ShouldBind(PropertyInfo pi)
        {
            return _members.ContainsKey(new MemberInfoKey(pi));
        }

        protected override bool ShouldBind(FieldInfo fi)
        {
            return _members.ContainsKey(new MemberInfoKey(fi));
        }

        protected override MemberBinding CreateBinding(ICompileContext context, Type type, FieldInfo field)
        {
            if (_members.TryGetValue(new MemberInfoKey(field), out var binding))
                return binding;

            return base.CreateBinding(context, type, field);
        }

        protected override MemberBinding CreateBinding(ICompileContext context, Type type, PropertyInfo prop)
        {
            if (_members.TryGetValue(new MemberInfoKey(prop), out var binding))
                return binding;

            return base.CreateBinding(context, type, prop);
        }
    }
}
