using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    public interface IMemberBindingBehaviourBuilder<TInstance>
    {
        MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression);
        IMemberBindingBehaviour BuildBehaviour();
    }
}
