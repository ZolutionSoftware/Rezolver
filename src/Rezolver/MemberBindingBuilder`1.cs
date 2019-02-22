// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Builds individual <see cref="MemberBinding"/> objects for the <see cref="MemberBindingBehaviourBuilder{TInstance}"/> class.
    ///
    /// Created through the <see cref="MemberBindingBehaviourBuilder{TInstance}.Bind{TMember}(Expression{Func{TInstance, TMember}})"/>.
    /// </summary>
    /// <typeparam name="TInstance">The type of object whose member is to be bound during construction by the container.</typeparam>
    /// <typeparam name="TMember">The type of the member that is to be bound.</typeparam>
    /// <remarks>The class also implements the <see cref="IMemberBindingBehaviourBuilder{TInstance}"/></remarks>
    public class MemberBindingBuilder<TInstance, TMember> : IMemberBindingBuilder, IMemberBindingBehaviourBuilder<TInstance>
    {
        private static readonly BindableCollectionType CollectionTypeInfo = typeof(TMember).GetBindableCollectionTypeInfo();
        private Func<MemberBinding> BindingFactory;

        /// <summary>
        /// Gets the member that will be bound by the <see cref="MemberBinding"/>
        /// </summary>
        public MemberInfo Member { get; private set; }

        /// <summary>
        /// Gets the behaviour builder to which this belongs
        /// </summary>
        public IMemberBindingBehaviourBuilder<TInstance> Parent { get; private set; }

        internal MemberBindingBuilder(MemberInfo member, IMemberBindingBehaviourBuilder<TInstance> parent)
        {
            Parent = parent;
            Member = member;
        }

        internal IMemberBindingBehaviourBuilder<TInstance> SetFactory(Func<MemberBinding> factory)
        {
            if (this.BindingFactory != null)
            {
                throw new InvalidOperationException("Binding factory has already been set");
            }

            this.BindingFactory = factory;
            return Parent;
        }

        private static ITarget CreateResolvedEnumerableTarget(Type elementType = null)
        {
            return Target.Resolved(typeof(IEnumerable<>).MakeGenericType(elementType ?? CollectionTypeInfo.ElementType));
        }

        private MemberBinding DefaultBindingFactory()
        {
            // if the member is a property that's read-only, then we allow collection binding if it's a collection type
            if (Member is PropertyInfo prop)
            {
                if (!prop.IsPubliclyWritable())
                {
                    if (CollectionTypeInfo != null)
                    {
                        return new ListMemberBinding(Member, CreateResolvedEnumerableTarget(), CollectionTypeInfo.ElementType, CollectionTypeInfo.AddMethod);
                    }
                    else
                    {
                        throw new InvalidOperationException($"The member {Member.DeclaringType}.{Member} is not publicly writable and is not suitable for collection initialisation");
                    }
                }
            }

            // otherwise just build a default binding
            return new MemberBinding(Member);
        }

        MemberBinding IMemberBindingBuilder.BuildBinding()
        {
            return this.BindingFactory?.Invoke() ?? DefaultBindingFactory();
        }

        /// <summary>
        /// Creates the <see cref="IMemberBindingBehaviour"/> represented by the current set of bindings
        ///
        /// </summary>
        /// <returns></returns>
        public IMemberBindingBehaviour BuildBehaviour()
        {
            return Parent.BuildBehaviour();
        }

        /// <summary>
        /// Sets the binding for the member to a particular type - so when the member is bound, an
        /// instance of <typeparamref name="TTarget"/> is resolved.
        /// </summary>
        /// <typeparam name="TTarget">The type to be resolved.</typeparam>
        /// <returns>The bindings builder, in order that another member binding can be created and configured.</returns>
        public IMemberBindingBehaviourBuilder<TInstance> ToType<TTarget>()
            where TTarget : TMember
        {
            return SetFactory(() => new MemberBinding(Member, typeof(TTarget)));
        }

        /// <summary>
        /// Sets the binding for the member to a particular type - so when the member is bound, an
        /// instance of <paramref name="type"/> is resolved.
        /// </summary>
        /// <param name="type">The type to be resolved.</param>
        /// <returns>The bindings builder, in order that another member binding can be created and configured.</returns>
        public IMemberBindingBehaviourBuilder<TInstance> ToType(Type type)
        {
            return SetFactory(() => new MemberBinding(Member, type));
        }

        /// <summary>
        /// Sets the binding for the member to a particular target.
        /// </summary>
        /// <param name="target">The target that is to be used to bind the member.</param>
        /// <returns>The bindings builder, in order that another member binding can be created and configured.</returns>
        public IMemberBindingBehaviourBuilder<TInstance> ToTarget(ITarget target)
        {
            return SetFactory(() => new MemberBinding(Member, target));
        }

        /// <summary>
        /// Sets the binding for the member to a constant service - equivalent to
        /// calling <see cref="ToTarget(ITarget)"/> with an <see cref="Targets.ObjectTarget"/>
        /// </summary>
        /// <param name="obj">The value to be bound to the member</param>
        /// <returns></returns>
        public IMemberBindingBehaviourBuilder<TInstance> ToObject(TMember obj)
        {
            return ToTarget(Target.ForObject(obj));
        }

        /// <summary>
        /// Explicitly sets the member to be bound as a collection initialiser - i.e. instead of resolving an
        /// instance of the member type, Rezolver will resolve an enumerable of an element type, which is then
        /// added to the collection after construction using a publicly available `Add` method on the member type.
        ///
        /// Use this when a member is of a type which satisfies the usual requirements for collection
        /// initialisation, but is read/write (since collection initialisation only kicks in automatically for read-only
        /// collection properties).
        ///
        /// The element type of the collection will be determined from the type's implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <returns>The bindings builder, in order that another member binding can be created and configured.</returns>
        public IMemberBindingBehaviourBuilder<TInstance> AsCollection()
        {
            ValidateCollectionType();
            return AsCollectionInternal((Type)null);
        }

        /// <summary>
        /// Explicitly sets the member to be bound as a collection initialiser using the targets passed in
        /// the <paramref name="elementTargets"/> argument.
        ///
        /// </summary>
        /// <param name="elementTargets"></param>
        /// <returns></returns>
        public IMemberBindingBehaviourBuilder<TInstance> AsCollection(params ITarget[] elementTargets)
        {
            ValidateCollectionType();
            return AsCollectionInternal(new Targets.EnumerableTarget(elementTargets, CollectionTypeInfo.ElementType));
        }

        /// <summary>
        /// This binds a collection member to an enumerable whose elements are resolved at resolve-time
        /// via the types provided.  In effect, multiple resolve operations are performed and the results
        /// are then fed into the collection.
        /// </summary>
        /// <param name="elementTypes">The types of each individual element</param>
        /// <returns></returns>
        public IMemberBindingBehaviourBuilder<TInstance> AsCollection(params Type[] elementTypes)
        {
            return AsCollection(elementTypes.Select(et => Target.Resolved(et)).ToArray());
        }

        /// <summary>
        /// Explicitly sets the member to be bound as a collection initialiser - i.e. instead of resolving an
        /// instance of the member type, Rezolver will resolve an enumerable of the type <typeparamref name="TElement"/>,
        /// which is then added to the collection after construction using a publicly available `Add` method on the
        /// member type.
        ///
        /// Use this when a member is of a type which satisfies the usual requirements for collection
        /// initialisation, but is read/write (since collection initialisation only kicks in automatically for read-only
        /// collection properties.  Or when you want to fill a collection with elements of a particular type.
        /// </summary>
        /// <typeparam name="TElement">The element type of the enumerable that will be used to initialise the collection
        /// represented by the member.</typeparam>
        /// <returns>The bindings builder, in order that another member binding can be created and configured.</returns>
        public IMemberBindingBehaviourBuilder<TInstance> AsCollection<TElement>()
        {
            ValidateCollectionType();
            return AsCollection(typeof(TElement));
        }

        /// <summary>
        /// Non-generic version of the <see cref="AsCollection{TElement}"/> method.
        /// </summary>
        /// <param name="elementType">The element type of the enumerable that will be used to initialise the collection
        /// represented by the member.</param>
        /// <returns>The bindings builder, in order that another member binding can be created and configured.</returns>
        public IMemberBindingBehaviourBuilder<TInstance> AsCollection(Type elementType)
        {
            ValidateCollectionType();
            return AsCollectionInternal(elementType);
        }

        private void ValidateCollectionType()
        {
            if (CollectionTypeInfo == null)
            {
                throw new InvalidOperationException($"The type {typeof(TMember)} is not a type suitable for collection initialisation.");
            }
        }

        private IMemberBindingBehaviourBuilder<TInstance> AsCollectionInternal(Type elementType)
            => AsCollectionInternal(CreateResolvedEnumerableTarget(elementType));

        private IMemberBindingBehaviourBuilder<TInstance> AsCollectionInternal(ITarget target)
            => SetFactory(() => new ListMemberBinding(Member, target, CollectionTypeInfo.ElementType, CollectionTypeInfo.AddMethod));

        /// <summary>
        /// Called to commence building a binding for another member belonging to the type <typeparamref name="TInstance"/>.
        /// </summary>
        /// <typeparam name="TNextMember">Type of the member represented by the expression <paramref name="memberBindingExpression"/>.</typeparam>
        /// <param name="memberBindingExpression">An expression that represents reading the member to be bound.  The body of the
        /// expression must be a <see cref="MemberExpression"/> with the <see cref="Expression.Type"/> of the
        /// <see cref="MemberExpression.Expression"/> equal to <typeparamref name="TInstance" />.</param>
        /// <returns>A builder that can be used to customise the binding for the member represented by the expression
        /// <paramref name="memberBindingExpression"/></returns>
        public MemberBindingBuilder<TInstance, TNextMember> Bind<TNextMember>(Expression<Func<TInstance, TNextMember>> memberBindingExpression)
            => Parent.Bind(memberBindingExpression ?? throw new ArgumentNullException(nameof(memberBindingExpression)));
    }
}
