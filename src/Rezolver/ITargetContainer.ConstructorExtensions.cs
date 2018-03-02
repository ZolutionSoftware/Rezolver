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
    /// Contains registration extensions for the <see cref="ITargetContainer"/> type
    /// </summary>
    public static partial class TargetContainerRegistrationExtensions
    {
        /// <summary>
        /// Registers a type by constructor alone.
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="constructor">The constructor to be bound. The <see cref="MemberInfo.DeclaringType"/> will be used
        /// as the service type to be registered against.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, passing a member binding
        /// behaviour here.</param>
        /// <remarks>If the <paramref name="constructor"/> belongs to an open generic type, then a <see cref="Targets.GenericConstructorTarget"/>
        /// will be created and registered.</remarks>
        public static void RegisterConstructor(
            this ITargetContainer targets,
            ConstructorInfo constructor,
            IMemberBindingBehaviour memberBindingBehaviour = null)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            targets.Register(Target.ForConstructor(constructor ?? throw new ArgumentNullException(nameof(constructor)), memberBindingBehaviour));
        }

        /// <summary>
        /// Registers a type by constructor against the service type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="constructor">The constructor to be bound.  The <see cref="MemberInfo.DeclaringType"/> must
        /// be compatible with the <paramref name="serviceType"/> otherwise an exception will occur.</param>
        /// <param name="serviceType">The type against which the registration will be made.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, passing a member binding
        /// behaviour here.</param>
        /// <remarks>If the <paramref name="constructor"/> belongs to an open generic type, then a <see cref="Targets.GenericConstructorTarget"/>
        /// will be created and registered.</remarks>
        public static void RegisterConstructor(
            this ITargetContainer targets,
            ConstructorInfo constructor,
            Type serviceType,
            IMemberBindingBehaviour memberBindingBehaviour = null)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            targets.Register(Target.ForConstructor(constructor ?? throw new ArgumentNullException(nameof(constructor)), memberBindingBehaviour), serviceType);
        }

        /// <summary>
        /// Register a type by constructor (represented by the expression <paramref name="newExpr"/>).
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="targets"></param>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of <typeparamref name="TObject"/>.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, passing a member binding
        /// behaviour here.</param>
        /// <remarks>Note that you can achieve a similar result by simply registering an expression which
        /// represents a call to a type's constructor.</remarks>
        public static void RegisterConstructor<TObject>(
            this ITargetContainer targets,
            Expression<Func<TObject>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            var ctor = Extract.Constructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression", nameof(newExpr));
            }

            targets.Register(Target.ForConstructor(ctor, memberBindingBehaviour));
        }

        /// <summary>
        /// Register a type by constructor (represented by the expression <paramref name="newExpr"/>).
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="targets"></param>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of <typeparamref name="TObject"/>.  An exception
        /// will be thrown if the lambda does not follow this pattern.</param>
        /// <param name="configureMemberBinding">A callback which configures a custom member binding via the
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> interface.  A new builder will be created and passed to this
        /// callback.</param>
        /// <remarks>Note that you can achieve a similar result by simply registering an expression which
        /// represents a call to a type's constructor.
        ///
        /// **Generic constructors**
        /// If you wish to register an open generic type by constructor through the use of an expression, then you
        /// need to use the <see cref="RegisterGenericConstructor{TObject}(ITargetContainer, Expression{Func{TObject}}, IMemberBindingBehaviour)"/>
        /// overload.</remarks>
        public static void RegisterConstructor<TObject>(
            this ITargetContainer targets,
            Expression<Func<TObject>> newExpr,
            Action<IMemberBindingBehaviourBuilder<TObject>> configureMemberBinding)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (configureMemberBinding == null)
            {
                throw new ArgumentNullException(nameof(configureMemberBinding));
            }

            var ctor = Extract.Constructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression", nameof(newExpr));
            }

            var behaviour = MemberBindingBehaviour.For<TObject>();
            configureMemberBinding(behaviour);
            targets.Register(Target.ForConstructor(ctor, behaviour.BuildBehaviour()));
        }

        /// <summary>
        /// Register a type by constructor (represented by the expression <paramref name="newExpr"/>)
        /// to be created when <typeparamref name="TService"/> is requested from the container.
        /// </summary>
        /// <typeparam name="TObject">The type whose constructor is to be called</typeparam>
        /// <typeparam name="TService">The service against which the registration is to be made.  An exception
        /// will be thrown if the lambda does not follow this pattern.</typeparam>
        /// <param name="targets"></param>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of <typeparamref name="TObject"/>.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, passing a member binding
        /// behaviour here.</param>
        /// <remarks>Note that you can achieve a similar result by simply registering an expression which
        /// represents a call to a type's constructor.</remarks>
        public static void RegisterConstructor<TObject, TService>(
            this ITargetContainer targets,
            Expression<Func<TObject>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
            where TObject : TService
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            var ctor = Extract.Constructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression", nameof(newExpr));
            }

            targets.Register(Target.ForConstructor(ctor, memberBindingBehaviour), typeof(TService));
        }

        /// <summary>
        /// Register a type by constructor (represented by the expression <paramref name="newExpr"/>)
        /// to be created when <typeparamref name="TService"/> is requested from the container.
        /// </summary>
        /// <typeparam name="TObject">The type whose constructor is to be called</typeparam>
        /// <typeparam name="TService">The service against which the registration is to be made.  An exception
        /// will be thrown if the lambda does not follow this pattern.</typeparam>
        /// <param name="targets"></param>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of <typeparamref name="TObject"/>.</param>
        /// <param name="configureMemberBinding">A callback which configures a custom member binding via the
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> interface.  A new builder will be created and passed to this
        /// callback.</param>
        /// <remarks>Note that you can achieve a similar result by simply registering an expression which
        /// represents a call to a type's constructor.</remarks>
        public static void RegisterConstructor<TObject, TService>(
            this ITargetContainer targets,
            Expression<Func<TObject>> newExpr,
            Action<IMemberBindingBehaviourBuilder<TObject>> configureMemberBinding)
            where TObject : TService
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            var ctor = Extract.Constructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression", nameof(newExpr));
            }

            var behaviour = MemberBindingBehaviour.For<TObject>();
            configureMemberBinding(behaviour);
            targets.Register(Target.ForConstructor(ctor, behaviour), typeof(TService));
        }

        /// <summary>
        /// Register a generic type by constructor (represented by the model expression <paramref name="newExpr"/>).
        /// Note - a concrete generic is used as an *example* - the equivalent open generic constructor is located
        /// and registered against the open generic of the type you actually invoke this method for; e.g. if
        /// <typeparamref name="TObject"/> is MyGeneric&lt;Foo, Bar&gt;, then a target bound to the equivalent
        /// constructor on the open generic MyGeneric&lt;,&gt; will be what is actually registered.
        /// </summary>
        /// <typeparam name="TObject">Must be a generic type.  It doesn't matter what the type arguments are, however,
        /// as the target that is created will be for the generic type definition of this type.</typeparam>
        /// <param name="targets"></param>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of <typeparamref name="TObject"/>.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, passing a member binding
        /// behaviour here.</param>
        /// <remarks>Note that you can achieve a similar result by simply registering an expression which
        /// represents a call to a type's constructor.</remarks>
        public static void RegisterGenericConstructor<TObject>(
            this ITargetContainer targets,
            Expression<Func<TObject>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (!TypeHelpers.IsGenericType(typeof(TObject)))
            {
                throw new ArgumentException($"{typeof(TObject)} is not a generic type.");
            }

            var ctor = Extract.GenericConstructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression invoking a generic type's constructor.", nameof(newExpr));
            }

            targets.Register(Target.ForConstructor(ctor, memberBindingBehaviour));
        }

        /// <summary>
        /// Register a generic type by constructor (represented by the model expression <paramref name="newExpr"/>)
        /// as an implementation of another open generic based on <typeparamref name="TService"/>.
        /// Note - a concrete generic is used as an *example* - the equivalent open generic constructor is located
        /// and registered against the open generic of the type you actually invoke this method for; e.g. if
        /// <typeparamref name="TObject"/> is MyGeneric&lt;Foo, Bar&gt; , then a target bound to the equivalent
        /// constructor on the open generic MyGeneric&lt;,&gt; will be what is actually registered.
        /// Also, if <typeparamref name="TService"/> is IMyGeneric&lt;Foo, Bar&gt;, then the service type that
        /// the new target is registered against will be IMyGeneric&lt;,&gt;.
        /// </summary>
        /// <typeparam name="TObject">Must be a generic type.  It doesn't matter what the type arguments are, however,
        /// as the target that is created will be for the generic type definition of this type.</typeparam>
        /// <typeparam name="TService">Must also be a generic type that is a base or interface of <typeparamref name="TObject"/></typeparam>
        /// <param name="targets">The target container into which the registration will be made.</param>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of <typeparamref name="TObject"/>.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, passing a member binding
        /// behaviour here.</param>
        /// <remarks>Note that you can achieve a similar result by simply registering an expression which
        /// represents a call to a type's constructor.</remarks>
        public static void RegisterGenericConstructor<TObject, TService>(
            this ITargetContainer targets,
            Expression<Func<TObject>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
            where TObject : TService
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (!TypeHelpers.IsGenericType(typeof(TObject)))
            {
                throw new ArgumentException($"Object type {typeof(TObject)} is not a generic type.", nameof(TObject));
            }

            if (!TypeHelpers.IsGenericType(typeof(TService)))
            {
                throw new ArgumentException($"Service type {typeof(TService)} is not a generic type", nameof(TService));
            }

            var ctor = Extract.GenericConstructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression invoking a generic type's constructor.", nameof(newExpr));
            }

            targets.Register(Target.ForConstructor(ctor, memberBindingBehaviour), typeof(TService).GetGenericTypeDefinition());
        }
    }
}
