using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    internal interface IMemberBindingBuilder
    {
        MemberInfo Member { get; }
        MemberBinding BuildBinding();
    }
}
