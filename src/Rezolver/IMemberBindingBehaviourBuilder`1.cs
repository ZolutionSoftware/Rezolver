// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// An object responsible for building an <see cref="IMemberBindingBehaviour"/> for instances of the type <typeparamref name="TInstance"/>
    /// </summary>
    /// <typeparam name="TInstance">The type of object for which a member binding behaviour is to be built.</typeparam>
    /// <remarks>This interface is part of [the fluent API](/developers/docs/member-injection/fluent-api.html) which drastically 
    /// simplifies the way in which you can configure custom member bindings for objects of different types.
    /// The <see cref="MemberBindingBehaviour.For{TInstance}"/> method is the easiest way to get an instance of this to work with.</remarks>
    public interface IMemberBindingBehaviourBuilder<TInstance>
    {
        /// <summary>
        /// Adds a member binding to the builder - the <paramref name="memberBindingExpression"/> identifies the member to
        /// be built.  The returned <see cref="MemberBindingBuilder{TInstance, TMember}"/> can then be used to customise
        /// how the member will be bound.
        /// </summary>
        /// <typeparam name="TMember">The type of member to be bound.</typeparam>
        /// <param name="memberBindingExpression">An expression which identifies the member to be bound.  The expression *must*
        /// be a direct member access, otherwise an <see cref="ArgumentException"/> will be thrown.</param>
        /// <returns></returns>
        MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression);
        /// <summary>
        /// Builds an <see cref="IMemberBindingBehaviour"/> which, when applied to the object produced by a <see cref="ConstructorTarget"/>, will
        /// bind the members of the new instance according to the way it has been configured through calls to the
        /// <see cref="Bind{TMember}(Expression{Func{TInstance, TMember}})"/> method.
        /// </summary>
        /// <returns></returns>
        IMemberBindingBehaviour BuildBehaviour();
    }
}
