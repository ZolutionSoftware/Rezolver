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
        // TODO: Change this to one which accepts all bindings up front, and then use a dictionary to store each by the member that is bound.
        private Dictionary<MemberInfo, MemberBinding> _members;
        public BindSpecificMembersBehaviour(IEnumerable<MemberInfo> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            _members = members.ToDictionary(m => m, m => (MemberBinding)null);
        }

        public BindSpecificMembersBehaviour(IEnumerable<MemberBinding> memberBindings)
        {
            if (memberBindings == null)
                throw new ArgumentNullException(nameof(memberBindings));

            _members = memberBindings.ToDictionary(b => b.Member);
        }

        protected override bool ShouldBind(PropertyInfo pi)
        {
            return _members.ContainsKey(pi);
        }

        protected override bool ShouldBind(FieldInfo fi)
        {
            return _members.ContainsKey(fi);
        }

        protected override MemberBinding CreateBinding(ICompileContext context, Type type, FieldInfo field)
        {
            if (_members.TryGetValue(field, out var binding))
                return binding;

            return base.CreateBinding(context, type, field);
        }

        protected override MemberBinding CreateBinding(ICompileContext context, Type type, PropertyInfo prop)
        {
            if (_members.TryGetValue(prop, out var binding))
                return binding;

            return base.CreateBinding(context, type, prop);
        }
    }
}
