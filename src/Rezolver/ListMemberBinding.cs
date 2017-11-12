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
    /// Represents a binding (i.e. like that produced by <see cref="IMemberBindingBehaviour"/>) to a property
    /// via dynamic collection initialiser.
    /// </summary>
    public class ListMemberBinding : MemberBinding
    {
        public Type ElementType { get; }
        public MethodInfo AddMethod { get; }

        public ListMemberBinding(MemberInfo member, ITarget target, Type elementType, MethodInfo addMethod)
        : base(member, target)
        {
            ElementType = elementType;
            AddMethod = addMethod;
        }
    }
}
