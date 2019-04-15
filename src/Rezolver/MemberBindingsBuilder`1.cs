// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Class for building custom <see cref="IMemberBindingBehaviour"/> for instances of <typeparamref name="TInstance"/>,
    /// default implementation of <see cref="IMemberBindingBehaviourBuilder{TInstance}"/>.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <remarks>
    /// To create custom bindings for individual members, use the <see cref="Bind{TMember}(Expression{Func{TInstance, TMember}})"/>
    /// method to create a <see cref="MemberBindingBuilder{TInstance, TMember}"/> which contains methods for controlling how a
    /// property or field will be bound.  Calls to that class will return an <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> which then
    /// allow you to chain further member customisations via a fluent API.
    ///
    /// You can't create an instance of this class directly - the framework creates an instance for you, either returning it (in the case
    /// of <see cref="MemberBindingBehaviour.For{TInstance}"/>) or passing it as an argument to a callback (in the case of
    /// <see cref="Target.ForType{T}(Action{IMemberBindingBehaviourBuilder{T}})"/>, for example.
    /// </remarks>
    public class MemberBindingBehaviourBuilder<TInstance> : IMemberBindingBehaviourBuilder<TInstance>
    {
        private readonly Dictionary<MemberInfo, IMemberBindingBuilder> _bindingBuilders = new Dictionary<MemberInfo, IMemberBindingBuilder>();

        private void AddBinding(IMemberBindingBuilder builder)
        {
            if (this._bindingBuilders.ContainsKey(builder.Member))
            {
                throw new ArgumentException($"Member {builder.Member.Name} has already been bound", nameof(builder));
            }

            this._bindingBuilders[builder.Member] = builder;
        }

        internal MemberBindingBehaviourBuilder() { }

        /// <summary>
        /// Implementation of <see cref="IMemberBindingBehaviourBuilder{TInstance}.BuildBehaviour"/>.
        ///
        /// Creates a new member binding behaviour that can, for example, be passed to a <see cref="ConstructorTarget"/> or other .
        /// </summary>
        /// <returns>A new <see cref="IMemberBindingBehaviour"/> which will bind only the members which you've identified with
        /// one or more calls to the <see cref="Bind{TMember}(Expression{Func{TInstance, TMember}})"/> method of this or
        /// the <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> returned by the <see cref="MemberBindingBuilder{TInstance, TMember}"/>
        /// returned by the `Bind` method.</returns>
        public IMemberBindingBehaviour BuildBehaviour() => new BindSpecificMembersBehaviour(this._bindingBuilders.Values.Select(b => b.BuildBinding()));

        /// <summary>
        /// Marks a member of the type <typeparamref name="TInstance"/> to be bound.  The returned <see cref="MemberBindingBuilder{TInstance, TMember}"/>
        /// can be used to customise the binding; and/or it can be used to continue marking other members to be bound.
        /// </summary>
        /// <typeparam name="TMember">Type of the member represented by the expression <paramref name="memberBindingExpression"/>.</typeparam>
        /// <param name="memberBindingExpression">An expression that represents reading the member to be bound.  The body of the
        /// expression must be a <see cref="MemberExpression"/> with the <see cref="Expression.Type"/> of the
        /// <see cref="MemberExpression.Expression"/> equal to <typeparamref name="TInstance" />.</param>
        /// <returns>A builder that can be used to customise the binding for the member represented by the expression
        /// <paramref name="memberBindingExpression"/></returns>
        public MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression)
        {
            var member = Extract.Member(memberBindingExpression);
            if (member == null)
            {
                throw new ArgumentException($"The expression {{{memberBindingExpression}}} must have a member access expression as its body, and it must be a member belonging to the type {typeof(TInstance)}", nameof(memberBindingExpression));
            }

            var toAdd = new MemberBindingBuilder<TInstance, TMember>(member, this);

            try
            {
                AddBinding(toAdd);
            }
            catch (ArgumentException aex)
            {
                throw new ArgumentException($"The member '{member}', represented by the expression {{{memberBindingExpression}}} has already been bound", nameof(memberBindingExpression), aex);
            }

            return toAdd;
        }
    }
}
