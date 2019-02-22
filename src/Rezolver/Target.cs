// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Provides static factory methods (including extension methods) for creating numerous types of targets
    /// from the <see cref="Rezolver.Targets"/> namespace.
    /// </summary>
    /// <remarks>
    /// Although most of these methods create targets of a known type (e.g. <see cref="Scoped(ITarget)"/>
    /// returns a <see cref="ScopedTarget"/>), the methods all return <see cref="ITarget"/> to allow for
    /// changes in implementation in the future.</remarks>
    public static partial class Target
    {
        /// <summary>
        /// Extension method which creates a <see cref="ScopedTarget"/> from the target on which this
        /// method is invoked.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ITarget Scoped(this ITarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new ScopedTarget(target);
        }

        /// <summary>
        /// Extension method which creates an <see cref="UnscopedTarget"/> from the target
        /// on which this method is invoked.
        /// </summary>
        /// <param name="target">The target.</param>
        public static ITarget Unscoped(this ITarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new UnscopedTarget(target);
        }

        /// <summary>
        /// Extension method which constructs a <see cref="SingletonTarget"/> that wraps the
        /// target on which the method is invoked.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ITarget Singleton(this ITarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new SingletonTarget(target);
        }

        /// <summary>
        /// Extension method which creates a new <see cref="ChangeTypeTarget"/> that wraps the
        /// <paramref name="target"/>, changing its <see cref="ITarget.DeclaredType"/> to the
        /// <paramref name="targetType"/> passed.
        /// </summary>
        /// <param name="target">Required. The target.</param>
        /// <param name="targetType">Required. The new type for the <paramref name="target"/>.</param>
        public static ITarget As(this ITarget target, Type targetType)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            return new ChangeTypeTarget(target, targetType);
        }

        /// <summary>
        /// Extension method which creates a new <see cref="ChangeTypeTarget"/> that wraps the
        /// <paramref name="target"/>, changing its <see cref="ITarget.DeclaredType"/> to
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <param name="target">Required. The target.</param>
        public static ITarget As<T>(this ITarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new ChangeTypeTarget(target, typeof(T));
        }

        /// <summary>
        /// Creates a new <see cref="ObjectTarget"/> for the passed <paramref name="object"/> whose
        /// <see cref="ITarget.DeclaredType"/> will either be set to <typeparamref name="T"/> or
        /// <paramref name="declaredType"/>, if it is passed non-null.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="object">The @object to be wrapped by the new <see cref="ObjectTarget"/></param>
        /// <param name="declaredType">Optional - will be used to set the <see cref="ITarget.DeclaredType"/>
        /// of the target that is created.</param>
        /// <param name="scopeBehaviour">The scope behaviour of the target if resolved inside an
        /// <see cref="IContainerScope"/>.</param>
        public static ITarget ForObject<T>(T @object, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.None)
        {
            return new ObjectTarget(@object, declaredType ?? typeof(T), scopeBehaviour);
        }

        /// <summary>
        /// Creates a new <see cref="ObjectTarget"/> for the passed <paramref name="object"/> whose
        /// <see cref="ITarget.DeclaredType"/> will either be set to object's type (obtained by calling
        /// <see cref="object.GetType"/> or <paramref name="declaredType"/>, if it is passed non-null.
        /// </summary>
        /// <param name="object">The @object to be wrapped by the new <see cref="ObjectTarget"/></param>
        /// <param name="declaredType">Optional - will be used to set the <see cref="ITarget.DeclaredType"/>
        /// of the target that is created.</param>
        /// <param name="scopeBehaviour">The scope behaviour of the target if resolved inside an
        /// <see cref="IContainerScope"/>.</param>
        public static ITarget ForObject(object @object, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.None)
        {
            return new ObjectTarget(@object, declaredType ?? @object?.GetType(), scopeBehaviour);
        }

        /// <summary>
        /// Creates a <see cref="ResolvedTarget"/> for the given type, providing a way to call back into the
        /// container during the execution of another target.
        /// </summary>
        /// <param name="type">Required. The type to be resolved from the container.</param>
        /// <param name="fallbackTarget">Optional. If provided and the container is unable to resolve the
        /// <paramref name="type"/>, then this target is used instead.</param>
        /// <returns>An <see cref="ITarget"/>.</returns>
        public static ITarget Resolved(Type type, ITarget fallbackTarget = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return new ResolvedTarget(type, fallbackTarget);
        }

        /// <summary>
        /// Generic version of <see cref="Resolved(Type, ITarget)"/> - creates a <see cref="ResolvedTarget"/>
        /// for the given type, providing a way to callback into the container during the execution of another
        /// target.
        /// </summary>
        /// <typeparam name="T">The type to be resolved from the container.</typeparam>
        /// <param name="fallbackTarget">Optional. If provided and the container is unable to resolve the
        /// <typeparamref name="T"/>, then this target is used instead</param>
        /// <returns>An <see cref="ITarget"/> which represents the act of resolving a particular type from the container.</returns>
        public static ITarget Resolved<T>(ITarget fallbackTarget = null)
        {
            return new ResolvedTarget(typeof(T), fallbackTarget);
        }

        /// <summary>
        /// A simple wrapper for the <see cref="DelegateTarget.DelegateTarget(Delegate, Type)"/> constructor.
        /// </summary>
        /// <param name="factory">Required, the factory delegate that is to be used to produce an object.</param>
        /// <param name="declaredType">Optional.  If not null, then it will be used as the target's <see cref="ITarget.DeclaredType"/>
        /// which, in turn, is used as the target's default registration type if not overriden when added to
        /// an <see cref="ITargetContainer"/>.  If null, then the return type of the factory will be used.</param>
        /// <returns>An <see cref="ITarget"/> which represents the passed factory.</returns>
        public static ITarget ForDelegate(Delegate factory, Type declaredType = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new DelegateTarget(factory, declaredType);
        }

        /// <summary>
        /// A simple wrapper for the <see cref="ExpressionTarget.ExpressionTarget(Expression, Type)"/> constructor.
        /// </summary>
        /// <param name="expression">Required, the expression representing the code that is to be executed
        /// in order to produce an object.</param>
        /// <param name="declaredType">Optional.  If not null, then it will be used as the target's <see cref="ITarget.DeclaredType"/>
        /// which, in turn, is used as the target's default registration type if not overriden when added to an
        /// <see cref="ITargetContainer"/>.  If null, then the <see cref="Expression.Type"/> will be used, unless
        /// <paramref name="expression"/> is a <see cref="LambdaExpression"/>, in which case the <see cref="Expression.Type"/>
        /// of its <see cref="LambdaExpression.Body"/> will be used.</param>
        /// <returns>An <see cref="ITarget"/> which represents the given expression; which must be compiled or otherwise
        /// translated into a runtime operation which creates/obtains an object.</returns>
        public static ITarget ForExpression(Expression expression, Type declaredType = null)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return new ExpressionTarget(expression, declaredType);
        }

        private static readonly IDictionary<string, ITarget> _emptyArgsDictionary = new Dictionary<string, ITarget>();

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>
        /// for the type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type whose constructor is to be bound by the target.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        /// <returns>A new target for the type <paramref name="type"/></returns>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.</remarks>
        public static ITarget ForType(
            Type type,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForType(type, namedArgs: null, memberBinding: memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>
        /// for the type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type whose constructor is to be bound by the target.</param>
        /// <param name="namedArgs">Optional.  A dictionary of targets that are to be bound to the type's
        /// constructor by name and type.  If <paramref name="type"/> is a generic type definition, then
        /// this parameter must be null, or an <see cref="ArgumentException"/> will be thrown.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        /// <returns>A new target for the type <paramref name="type"/></returns>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.</remarks>
        public static ITarget ForType(
            Type type,
            IDictionary<string, ITarget> namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (TypeHelpers.IsGenericTypeDefinition(type))
            {
                // can't pass named arguments if the type is generic, because there's no reliable
                // way to guarantee that the arguments can actually be bound at the moment.
                // once we have conditional targets, then perhaps.
                if (namedArgs?.Count > 0)
                {
                    throw new ArgumentException("Cannot use namedArguments with a generic type", nameof(namedArgs));
                }

                return new GenericConstructorTarget(type, memberBinding);
            }

            return new ConstructorTarget(type, namedArgs, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound by the target.</typeparam>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        /// <returns>A new target for the type <typeparamref name="T"/></returns>
        public static ITarget ForType<T>(IMemberBindingBehaviour memberBinding = null)
        {
            return ForType<T>(namedArgs: null, memberBinding: memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the type <typeparamref name="T"/> with
        /// an <see cref="IMemberBindingBehaviour"/> built from an <see cref="IMemberBindingBehaviourBuilder{TInstance}"/>
        /// that's configured by the <paramref name="configureMemberBinding"/> callback.
        /// </summary>
        /// <typeparam name="T">The type of object to be created by the target</typeparam>
        /// <param name="configureMemberBinding">Will be called with a new instance of
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> for you to configure any
        /// member bindings you wish to add to the target.</param>
        /// <returns>A new target for the type <typeparamref name="T"/></returns>
        public static ITarget ForType<T>(Action<IMemberBindingBehaviourBuilder<T>> configureMemberBinding)
        {
            var builder = MemberBindingBehaviour.For<T>();
            configureMemberBinding?.Invoke(builder);
            return ForType<T>(builder.BuildBehaviour());
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget" /> or <see cref="GenericConstructorTarget" />
        /// for the type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound by the target.</typeparam>
        /// <param name="namedArgs">Can be null. A dictionary of targets that are to be bound to the type's
        /// constructor by name and <see cref="ITarget.DeclaredType"/>.
        ///
        /// If <typeparamref name="T"/> is a generic type definition, then
        /// this parameter must be null, or an <see cref="ArgumentException"/> will be thrown.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        /// <returns>A new target for the type <typeparamref name="T"/></returns>
        public static ITarget ForType<T>(
            IDictionary<string, ITarget> namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForType(typeof(T), namedArgs, memberBinding);
        }

        /// <summary>
        /// Same as <see cref="ForType{T}(IDictionary{string, ITarget}, IMemberBindingBehaviour)"/> except this allows you to
        /// build a custom member binding behaviour using the fluent API exposed by the <see cref="IMemberBindingBehaviourBuilder{TInstance}"/>
        /// interface
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound by the target.</typeparam>
        /// <param name="namedArgs">Can be null. A dictionary of targets that are to be bound to the type's
        /// constructor by name and <see cref="ITarget.DeclaredType"/>.
        ///
        /// If <typeparamref name="T"/> is a generic type definition, then
        /// this parameter must be null, or an <see cref="ArgumentException"/> will be thrown.</param>
        /// <param name="configureMemberBinding">Will be called with a new instance of
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> for you to configure any
        /// member bindings you wish to add to the target.</param>
        /// <returns>A new target for the type <typeparamref name="T"/></returns>
        public static ITarget ForType<T>(
            IDictionary<string, ITarget> namedArgs,
            Action<IMemberBindingBehaviourBuilder<T>> configureMemberBinding)
        {
            var builder = MemberBindingBehaviour.For<T>();
            configureMemberBinding?.Invoke(builder);
            return ForType<T>(namedArgs, builder.BuildBehaviour());
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>
        /// for the type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type whose constructor is to be bound by the target.</param>
        /// <param name="namedArgs">Optional.  An object whose publicly readable members which are of the
        /// type <see cref="ITarget"/> (or a type which implements it) are to be bound to the type's constructor
        /// by name and <see cref="ITarget.DeclaredType"/>.
        ///
        /// If <paramref name="type"/> is a generic type definition, then
        /// this parameter must be null, or an <see cref="ArgumentException"/> will be thrown.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.
        /// </remarks>
        /// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
        /// ConstructorTarget for the type 'MyType':
        /// <code>Target.ForType(typeof(MyType), namedArguments: new { param1 = new ObjectTarget(&quot; Hello World&quot;) });</code>
        /// </example>
        public static ITarget ForType(
            Type type,
            object namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForType(type, namedArgs.ToMemberValueDictionary<ITarget>(), memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>
        /// for the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound by the created target.</typeparam>
        /// <param name="namedArgs">Optional.  An object whose publicly readable members which are of the
        /// type <see cref="ITarget"/> (or a type which implements it) are to be bound to the type's constructor
        /// by name and <see cref="ITarget.DeclaredType"/>.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        /// <returns>A new target for the type <typeparamref name="T"/></returns>
        /// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
        /// ConstructorTarget for the type 'MyType':
        /// <code>Target.ForType&lt;MyType&gt;(namedArguments: new { param1 = new ObjectTarget(&quot;Hello World&quot;) });</code>
        /// </example>
        public static ITarget ForType<T>(
            object namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForType(typeof(T), namedArgs, memberBinding);
        }

        /// <summary>
        /// Same as <see cref="ForType{T}(object, IMemberBindingBehaviour)"/> except this lets you build a custom <see cref="IMemberBindingBehaviour"/>
        /// using the fluent API offered by the <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> interface.
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound by the created target.</typeparam>
        /// <param name="namedArgs">Optional.  An objeect whose publicly readable members which are of the type
        /// <see cref="ITarget"/> (or a type which implements it) are to be bound to the type's constructor by name
        /// and <see cref="ITarget.DeclaredType"/>.</param>
        /// <param name="configureMemberBinding">Will be called with a new instance of
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> for you to configure any
        /// member bindings you wish to add to the target.</param>
        /// <returns>A new target for the type <typeparamref name="T"/></returns>
        public static ITarget ForType<T>(
            object namedArgs,
            Action<IMemberBindingBehaviourBuilder<T>> configureMemberBinding)
        {
            var builder = MemberBindingBehaviour.For<T>();
            configureMemberBinding?.Invoke(builder);
            return ForType(typeof(T), namedArgs, builder.BuildBehaviour());
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor, or a <see cref="GenericConstructorTarget"/> if the
        /// constructor belongs to a generic type definition ('open generic type').
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForConstructor(constructor, (ParameterBinding[])null, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor, or a <see cref="GenericConstructorTarget"/> if the
        /// constructor belongs to a generic type definition ('open generic type').
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="parameterBindings">Can be null/empty.  An array of <see cref="ParameterBinding"/>
        /// objects containing targets to be bound to somme or all of the constructor parameters.  **Must not be supplied if <paramref name="constructor"/>
        /// is a constructor belonging to an open generic type,**</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            ParameterBinding[] parameterBindings,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }

            if (TypeHelpers.IsGenericTypeDefinition(constructor.DeclaringType))
            {
                if (parameterBindings?.Length > 0)
                {
                    throw new ArgumentException("You cannot currently supply parameter bindings for generic constructors", nameof(parameterBindings));
                }

                return new GenericConstructorTarget(constructor, memberBinding);
            }

            return new ConstructorTarget(constructor, parameterBindings, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor, or a <see cref="GenericConstructorTarget"/> if the
        /// constructor belongs to a generic type definition ('open generic type').
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="namedArgs">Can be null.  A dictionary of targets that are to be bound to the
        /// constructor by name and <see cref="ITarget.DeclaredType"/>.  **Must not be supplied if <paramref name="constructor"/>
        /// is a constructor belonging to an open generic type,**</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            IDictionary<string, ITarget> namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }

            if (TypeHelpers.IsGenericTypeDefinition(constructor.DeclaringType))
            {
                if (namedArgs?.Count > 0)
                {
                    throw new ArgumentException("You cannot current supply named argument bindings for open generic constructors", nameof(namedArgs));
                }

                return new GenericConstructorTarget(constructor, memberBinding);
            }

            var bindings = ParameterBinding.BindMethod(constructor, namedArgs ?? _emptyArgsDictionary);
            return new ConstructorTarget(constructor, bindings, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor, or a <see cref="GenericConstructorTarget"/> if the
        /// constructor belongs to a generic type definition ('open generic type').
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="namedArgs">Optional.  An object whose publicly readable members which are of the
        /// type <see cref="ITarget"/> (or a type which implements it) are to be bound to the constructor
        /// by name and <see cref="ITarget.DeclaredType"/>.  **Must not be supplied if <paramref name="constructor"/>
        /// is a constructor belonging to an open generic type,**</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on any target container in which the target is subsequently registered.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            object namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }

            if (TypeHelpers.IsGenericTypeDefinition(constructor.DeclaringType))
            {
                if (namedArgs != null)
                {
                    throw new ArgumentException("You cannot current supply named argument bindings for open generic constructors", nameof(namedArgs));
                }

                return new GenericConstructorTarget(constructor, memberBinding);
            }

            var bindings = ParameterBinding.BindMethod(constructor, namedArgs.ToMemberValueDictionary<ITarget>());
            return new ConstructorTarget(constructor, bindings, memberBinding);
        }

        /// <summary>
        /// Creates a target which binds to the constructor of an open generic type, an exemplar of which is passed in <paramref name="newExpr"/>.
        /// 
        /// The created target will be <see cref="Targets.GenericConstructorTarget"/> whose <see cref="Targets.GenericConstructorTarget.GenericTypeConstructor"/>
        /// will be set to the constructor that is identified from the expression.
        /// </summary>
        /// <typeparam name="TExample">Must be a generic type.  It doesn't matter what the type arguments are, however,
        /// as the target that is created will be for the generic type definition of this type.</typeparam>
        /// <param name="newExpr">A lambda expression whose <see cref="LambdaExpression.Body"/> is a <see cref="NewExpression"/>
        /// which identifies the constructor that is to be used to create the instance of all concrete types derived from the
        /// the same generic type definition as <typeparamref name="TExample"/>.</param>
        /// <param name="memberBindingBehaviour">Optional. If you wish to bind members on the new instance, pass a member binding
        /// behaviour here.</param>
        /// <remarks>Note - a concrete generic is used as an *example* - the equivalent open generic constructor is located
        /// and registered against the open generic of the type you actually invoke this method for; e.g. if
        /// <typeparamref name="TExample"/> is `MyGeneric&lt;Foo, Bar&gt;`, then a target bound to the equivalent
        /// constructor on the open generic `MyGeneric&lt;,&gt;` will be what is actually created.</remarks>
        public static ITarget ForGenericConstructor<TExample>(
            Expression<Func<TExample>> newExpr,
            IMemberBindingBehaviour memberBindingBehaviour = null)
        {
            if (!TypeHelpers.IsGenericType(typeof(TExample)))
            {
                throw new ArgumentException($"{typeof(TExample)} is not a generic type.");
            }

            var ctor = Extract.GenericConstructor(newExpr ?? throw new ArgumentNullException(nameof(newExpr)));
            if (ctor == null)
            {
                throw new ArgumentException($"The expression ${newExpr} does not represent a NewExpression invoking a generic type's constructor.", nameof(newExpr));
            }

            return ForConstructor(ctor, memberBindingBehaviour);
        }
    }
}
