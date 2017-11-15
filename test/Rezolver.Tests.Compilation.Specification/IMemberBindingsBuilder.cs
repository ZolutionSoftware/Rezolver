using System;
using System.Linq.Expressions;

namespace Rezolver.Tests.Compilation.Specification
{
    public interface IMemberBindingsBuilder<TInstance>
    {
        MemberBindingsBuilder<TInstance>.MemberBindingBuilder<TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression);
        IMemberBindingBehaviour CreateBehaviour();
    }
}