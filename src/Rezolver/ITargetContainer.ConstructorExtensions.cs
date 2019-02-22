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
        /// <seealso cref="Target.ForGenericConstructor{TExample}(Expression{Func{TExample}}, IMemberBindingBehaviour)"/>
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
        /// Creates and registers a target bound to the constructor of a generic type definition using the 
        /// <see cref="Target.ForGenericConstructor{TExample}(Expression{Func{TExample}}, IMemberBindingBehaviour)"/> factory method. 
        /// 
        /// See the documentation on that method for more.
        /// 
        /// The registration will be made against the open generic type.
        /// </summary>
        /// <typeparam name="TExample">Must be a generic type which represents a concrete generic whose generic type definition will be
        /// bound by the created target.  This type is also used as the service type for the registration.</typeparam>
        /// <param name="targets">The container into which the registration will be made.</param>
        /// <param name="newExpr">Exemplar expression which is used to identify the constructor to be bound.</param>
        /// <param name="memberBindingBehaviour">A member binding behaviour to be passed to the created target</param>
        /// <seealso cref="Target.ForGenericConstructor{TExample}(Expression{Func{TExample}}, IMemberBindingBehaviour)"/>
        public static void RegisterGenericConstructor<TExample>(
            this ITargetContainer targets,
            Expression<Func<TExample>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            targets.Register(Target.ForGenericConstructor(newExpr, memberBindingBehaviour));
        }

        /// <summary>
        /// Same as <see cref="RegisterGenericConstructor{TObject}(ITargetContainer, Expression{Func{TObject}}, IMemberBindingBehaviour)"/>
        /// except this creates the target and then registers it against a generic base or interface of the generic type definition identified
        /// from <typeparamref name="TExampleService"/>.
        /// </summary>
        /// <typeparam name="TExample">Must be a generic type which represents a concrete generic whose generic type definition will be
        /// bound by the target that is created and registered.  The type must inherit or implement the type <typeparamref name="TExampleService"/>.</typeparam>
        /// <typeparam name="TExampleService">Must be a generic type that is a base or interface of <typeparamref name="TExample"/>.  The registration will
        /// be made against this type's generic type definition.</typeparam>
        /// <param name="targets">The container into which the registration will be made.</param>
        /// <param name="newExpr">Exemplar expression which is used to identify the constructor to be bound.</param>
        /// <param name="memberBindingBehaviour">A member binding behaviour to be passed to the created target</param>
        /// <seealso cref="Target.ForGenericConstructor{TExample}(Expression{Func{TExample}}, IMemberBindingBehaviour)"/>
        public static void RegisterGenericConstructor<TExample, TExampleService>(
            this ITargetContainer targets,
            Expression<Func<TExample>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
            where TExample : TExampleService
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (!TypeHelpers.IsGenericType(typeof(TExampleService)))
            {
                throw new ArgumentException($"Service type {typeof(TExampleService)} is not a generic type", nameof(TExampleService));
            }

            targets.Register(Target.ForGenericConstructor(newExpr, memberBindingBehaviour), typeof(TExampleService).GetGenericTypeDefinition());
        }
    }
}
