// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Runtime;

namespace Rezolver
{
    /// <summary>
    /// Extensions for the <see cref="IRootTargetContainer"/> interface.
    /// </summary>
    public static partial class RootTargetContainerExtensions
    {
        /// <summary>
        /// Registers an enumerable projection that will create an enumerable of type <typeparamref name="TTo"/>
        /// from elements of an input enumerable of type <typeparamref name="TFrom"/> using constructor injection
        /// to create each instance of <typeparamref name="TTo"/>.
        ///
        /// The same as calling <see cref="RegisterProjection(IRootTargetContainer, Type, Type, Type)"/> with
        /// <typeparamref name="TTo"/> used as the argument to both `TTo` and `TImplementation` type parameters.
        /// </summary>
        /// <typeparam name="TFrom">The type of the enumerable that provides the source of the projection</typeparam>
        /// <typeparam name="TTo">The type of the enumerable that will be the output of the projection</typeparam>
        /// <param name="targets"></param>
        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets)
        {
            RegisterProjection<TFrom, TTo, TTo>(targets);
        }

        /// <summary>
        /// Registers an enumerable projection that will create an enumerable of type <typeparamref name="TTo"/>
        /// from elements of an input enumerable of type <typeparamref name="TFrom"/> using constructor injection
        /// to create each instance of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of the enumerable that provides the source of the projection</typeparam>
        /// <typeparam name="TTo">The type of the enumerable that will be the output of the projection</typeparam>
        /// <typeparam name="TImplementation">The type to be created for each element.</typeparam>
        /// <param name="targets"></param>
        /// <remarks>
        /// This is like hot-wiring the Linq <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
        /// directly into the container.
        ///
        /// Typically each instance of the implementation type <typeparamref name="TImplementation"/> will require an
        /// instance of type <typeparamref name="TFrom"/> to be passed into its constructor - the framework
        /// takes care of passing the individual elements in for each instance of <typeparamref name="TImplementation"/>
        /// that it creates.</remarks>
        public static void RegisterProjection<TFrom, TTo, TImplementation>(this IRootTargetContainer targets)
            where TImplementation : TTo
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), typeof(TImplementation));
        }

        /// <summary>
        /// Registers an enumerable projection that create an enumerable of type <typeparamref name="TTo"/>
        /// from elements of an input enumerable of type <typeparamref name="TFrom"/> with the type of each
        /// projected element is determined by the result of the <paramref name="implementationTypeSelector"/>
        /// callback for each input target.  The type returned by this callback must be able to be created via
        /// automatic constructor injection.
        /// </summary>
        /// <typeparam name="TFrom">The type of the enumerable that provides the source of the projection</typeparam>
        /// <typeparam name="TTo">The type of the enumerable that will be the output of the projection</typeparam>
        /// <param name="targets"></param>
        /// <param name="implementationTypeSelector">A callback to be invoked for each target in the input enumerable which
        /// provides the type that is to be created for the corresponding element in the output enumerable.</param>
        /// <remarks>This overload is useful for creating specialised generics which share the <typeparamref name="TTo"/> type
        /// as a base or interface.</remarks>
        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets, Func<IRootTargetContainer, ITarget, Type> implementationTypeSelector)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), implementationTypeSelector);
        }

        /// <summary>
        /// Non-generic version of <see cref="RegisterProjection{TFrom, TTo}(IRootTargetContainer)"/>
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="fromType">The type of the enumerable that provides the source of the projection</param>
        /// <param name="toType">The type of the enumerable that will be the output of the projection - this type
        /// must be able to be created via automatic constructor injection.</param>
        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType)
        {
            RegisterProjection(targets, fromType, toType, toType);
        }

        /// <summary>
        /// Non-generic version of <see cref="RegisterProjection{TFrom, TTo, TImplementation}(IRootTargetContainer)"/>
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="fromType">The type of the enumerable that provides the source of the projection.</param>
        /// <param name="toType">The type of the enumerable produced by the projection.</param>
        /// <param name="implementationType">The type to be created for each element in the output enumerable.  Must be able to
        /// be created via automatic constructor injection.</param>
        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Type implementationType)
        {
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            RegisterProjection(targets, fromType, toType, (r, t) => implementationType);
        }

        /// <summary>
        /// Non-generic version of <see cref="RegisterProjection{TFrom, TTo}(IRootTargetContainer, Func{IRootTargetContainer, ITarget, Type})"/>
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="fromType">The type of the enumerable that provides the source of the projection.</param>
        /// <param name="toType">The type of the enumerable produced by the projection.</param>
        /// <param name="implementationTypeSelector">A callback to be invoked for each target in the input enumerable which
        /// provides the type that is to be created for the corresponding element in the output enumerable.</param>
        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, Type> implementationTypeSelector)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (fromType == null)
            {
                throw new ArgumentNullException(nameof(fromType));
            }

            if (toType == null)
            {
                throw new ArgumentNullException(nameof(toType));
            }

            if (fromType == toType)
            {
                throw new ArgumentException($"The output enumerable type ({toType}) cannot be the same as the input enumerable type ({fromType})", nameof(toType));
            }

            if (implementationTypeSelector == null)
            {
                implementationTypeSelector = (r, t) => toType;
            }

            RegisterProjectionInternal(targets, fromType, toType, (r, t) =>
            {
                var implementationType = implementationTypeSelector(r, t);
                if (implementationType == null)
                {
                    throw new InvalidOperationException($"Implementation type returned for projection from {fromType} to {toType} for target {t} returned null");
                }

                // REVIEW: Cache the .ForType result on a per-type basis? It's container-agnostic.
                var target = r.Fetch(implementationType);
                return new TargetProjection(target != null && !target.UseFallback ? target : Target.ForType(implementationType), implementationType);
            });
        }

        /// <summary>
        /// Registers an enumerable projection that will create an enumerable of type <typeparamref name="TTo"/>
        /// from elements of an input enumerable of type <typeparamref name="TFrom"/> using targets produced by the
        /// <paramref name="implementationTargetFactory"/> for each input element.
        /// </summary>
        /// <typeparam name="TFrom">The type of the enumerable that provides the source of the projection</typeparam>
        /// <typeparam name="TTo">The type of the enumerable that will be the output of the projection</typeparam>
        /// <param name="targets"></param>
        /// <param name="implementationTargetFactory">A factory callback that will be called once for each target in the
        /// input enumerable - the target returned by the factory will then be used to produce the projected element
        /// in the output enumerable.</param>
        /// <remarks>This overload is useful when output elements require more complex injection strategies other than just
        /// constructor injection; or perhaps where more advanced constructor injection is required (e.g. perhaps with member
        /// injection).</remarks>
        public static void RegisterProjection<TFrom, TTo>(this IRootTargetContainer targets, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjection(targets, typeof(TFrom), typeof(TTo), implementationTargetFactory);
        }

        /// <summary>
        /// Non-generic version of <see cref="RegisterProjection{TFrom, TTo}(IRootTargetContainer, Func{IRootTargetContainer, ITarget, ITarget})"/>
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="fromType">The type of the enumerable that provides the source of the projection</param>
        /// <param name="toType">The type of the enumerable that will be the output of the projection</param>
        /// <param name="implementationTargetFactory">A factory callback that will be called once for each target in the
        /// input enumerable - the target returned by the factory will then be used to produce the projected element
        /// in the output enumerable.</param>
        public static void RegisterProjection(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjectionInternal(targets ?? throw new ArgumentNullException(nameof(targets)),
                fromType ?? throw new ArgumentNullException(nameof(fromType)),
                toType ?? throw new ArgumentNullException(nameof(toType)),
                implementationTargetFactory ?? throw new ArgumentNullException(nameof(implementationTargetFactory)));
        }

        private static void RegisterProjectionInternal(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, ITarget> implementationTargetFactory)
        {
            RegisterProjectionInternal(targets, fromType, toType, (r, t) =>
                {
                    var target = implementationTargetFactory(r, t);
                    if (target == null)
                    {
                        throw new InvalidOperationException("Implementation target factory returned a null target");
                    }

                    return new TargetProjection(target, target.DeclaredType);
                });
        }

        private static void RegisterProjectionInternal(this IRootTargetContainer targets, Type fromType, Type toType, Func<IRootTargetContainer, ITarget, TargetProjection> projectionFactory)
        {
            targets.RegisterContainer(typeof(IEnumerable<>).MakeGenericType(toType),
                new ProjectionTargetContainer(targets, fromType, toType, projectionFactory));
        }
    }
}
