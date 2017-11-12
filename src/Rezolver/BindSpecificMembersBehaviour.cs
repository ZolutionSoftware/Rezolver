using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver
{
    public class BindSpecificMembersBehaviour : BindAllMembersBehaviour
    {
        private HashSet<MemberInfo> _members;
        public BindSpecificMembersBehaviour(IEnumerable<MemberInfo> members)
        {
            _members = new HashSet<MemberInfo>(members ?? throw new ArgumentNullException(nameof(members)));
        }

        protected override bool ShouldBind(PropertyInfo pi)
        {
            return _members.Contains(pi);
        }

        protected override bool ShouldBind(FieldInfo fi)
        {
            return _members.Contains(fi);
        }
    }
}
