using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
                throw new ArgumentNullException(nameof(target));
            
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
                throw new ArgumentNullException(nameof(target));

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
                throw new ArgumentNullException(nameof(target));

            return new SingletonTarget(target);
        }

        /// <summary>
        /// Extension method which creates a new <see cref="ChangeTypeTarget"/> that wraps the 
        /// <paramref name="target"/>, changing its <see cref="ITarget.DeclaredType"/> to the 
        /// <paramref name="targetType"/> passed.
        /// </summary>
        /// <param name="target">Required. The target.</param>
        /// <param name="targetType">Required. The new type for the <paramref name="target"/>.</param>
        public static ITarget ChangeType(this ITarget target, Type targetType)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return new ChangeTypeTarget(target, targetType);
        }

        /// <summary>
        /// Extension method which creates a new <see cref="ChangeTypeTarget"/> that wraps the 
        /// <paramref name="target"/>, changing its <see cref="ITarget.DeclaredType"/> to 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <param name="target">Required. The target.</param>
        public static ITarget ChangeType<T>(this ITarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

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
            return new ObjectTarget(@object, declaredType, scopeBehaviour);
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

        #region Types/Constructors
        private static readonly IDictionary<string, ITarget> _emptyArgsDictionary = new Dictionary<string, ITarget>();

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>
        /// for the type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type whose constructor is to be bound by the target.</param>
        /// <param name="memberBinding">The member binding behaviour.</param>
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
        /// <param name="memberBinding">The member binding behaviour.</param>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.</remarks>
        public static ITarget ForType(
            Type type,
            IDictionary<string, ITarget> namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (TypeHelpers.IsGenericTypeDefinition(type))
            {
                //can't pass named arguments if the type is generic, because there's no reliable
                //way to guarantee that the arguments can actually be bound at the moment.
                //once we have conditional targets, then perhaps.
                if (namedArgs?.Count > 0)
                    throw new ArgumentException("Cannot use namedArguments with a generic type", nameof(namedArgs));

                return new GenericConstructorTarget(type, memberBinding);
            }

            return new ConstructorTarget(type, namedArgs, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>
        /// for the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound by the target.</typeparam>
        /// <param name="memberBinding">The member binding behaviour.</param>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.</remarks>
        public static ITarget ForType<T>(IMemberBindingBehaviour memberBinding = null)
        {
            return ForType<T>(namedArgs: null, memberBinding: memberBinding);
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
        /// <param name="memberBinding">Optional.  The member binding behaviour to be used - controls
        /// whether and which properties and/or fields also receive values injected from the container when an
        /// instance is created.</param>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget" />
        /// is created; otherwise a <see cref="ConstructorTarget" /> is created.</remarks>
        public static ITarget ForType<T>(
            IDictionary<string, ITarget> namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForType(typeof(T), namedArgs, memberBinding);
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
        /// <param name="memberBinding">The member binding behaviour.</param>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.
        /// </remarks>
        /// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
        /// ConstructorTarget for the type 'MyType':
        ///<code>Target.ForType(typeof(MyType), namedArguments: new { param1 = new ObjectTarget(&quot; Hello World&quot;) });</code>
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
        /// <typeparam name="T">The type whose constructor is to be bound by the target.</typeparam>
        /// <param name="namedArgs">Optional.  An object whose publicly readable members which are of the 
        /// type <see cref="ITarget"/> (or a type which implements it) are to be bound to the type's constructor 
        /// by name and <see cref="ITarget.DeclaredType"/>.
        /// 
        /// If <typeparamref name="T"/> is a generic type definition, then
        /// this parameter must be null, or an <see cref="ArgumentException"/> will be thrown.</param>
        /// <param name="memberBinding">The member binding behaviour.</param>
        /// <remarks>If the type is a generic type definition, then a <see cref="GenericConstructorTarget"/>
        /// is created; otherwise a <see cref="ConstructorTarget"/> is created.
        /// </remarks>
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
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor.
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="memberBinding">The member binding behaviour.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            IMemberBindingBehaviour memberBinding = null)
        {
            return ForConstructor(constructor, (ParameterBinding[])null, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor.
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="parameterBindings">Can be null/empty.  An array of <see cref="ParameterBinding"/> 
        /// objects containing targets to be bound to somme or all of the constructor parameters.</param>
        /// <param name="memberBinding">The member binding behaviour.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            ParameterBinding[] parameterBindings,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            return new ConstructorTarget(constructor, parameterBindings, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor.
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="namedArgs">Can be null.  A dictionary of targets that are to be bound to the
        /// constructor by name and <see cref="ITarget.DeclaredType"/>.</param>
        /// <param name="memberBinding">The member binding behaviour.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            IDictionary<string, ITarget> namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));
            var bindings = ParameterBinding.BindMethod(constructor, namedArgs ?? _emptyArgsDictionary);
            return new ConstructorTarget(constructor, bindings, memberBinding);
        }

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> for the given constructor.
        /// </summary>
        /// <param name="constructor">Required.  The constructor to be bound by the target.</param>
        /// <param name="namedArgs">Optional.  An object whose publicly readable members which are of the 
        /// type <see cref="ITarget"/> (or a type which implements it) are to be bound to the constructor 
        /// by name and <see cref="ITarget.DeclaredType"/>.</param>
        /// <param name="memberBinding">The member binding behaviour.</param>
        public static ITarget ForConstructor(
            ConstructorInfo constructor,
            object namedArgs,
            IMemberBindingBehaviour memberBinding = null)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));
            var bindings = ParameterBinding.BindMethod(constructor, namedArgs.ToMemberValueDictionary<ITarget>());
            return new ConstructorTarget(constructor, bindings, memberBinding);
        }

        #endregion
    }
}
