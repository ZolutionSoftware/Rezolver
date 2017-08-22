// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Implements service decoration in an <see cref="ITargetContainer"/>, producing instances of the
    /// <see cref="DecoratorTarget"/> when <see cref="Fetch(Type)"/> or <see cref="FetchAll(Type)"/> are called.
    /// 
    /// The best way to add a decorator to your target container is to use the extension method 
    /// <see cref="DecoratorTargetContainerExtensions.RegisterDecorator{TDecorator, TDecorated}(ITargetContainer)"/>
    /// or its non-generic equivalent.
    /// </summary>
    /// <remarks>This class does not implement <see cref="ITarget"/>, rather
    /// it's an <see cref="ITargetContainer"/> into which other targets can be added,
    /// and when <see cref="Fetch(Type)"/> or <see cref="FetchAll(Type)"/> are called, a temporary
    /// <see cref="DecoratorTarget"/> is created which wraps around the targets that have been registered within and
    /// which will ultimately create instances of <see cref="DecoratorType"/></remarks>
    public class DecoratingTargetContainer : ITargetContainer
    {
        ///// <summary>
        ///// Gets the type which will be used to decorate the instances produced by targets in this decorator target.
        ///// </summary>
        //public Type DecoratorType { get; }

        /// <summary>
        /// Gets the type that's being decorated - is also the type under which this decorating container is registered
        /// in the <see cref="Root"/>
        /// </summary>
        public Type DecoratedType { get; }

        private ITargetContainer Inner { get; set; }

        private ITargetContainer Root { get; }

        private DecoratingTargetFactory DecoratorFactory { get; }

        private DecoratingTargetContainer(ITargetContainer root, Type decoratedType)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            DecoratedType = decoratedType ?? throw new ArgumentNullException(nameof(decoratedType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratingTargetContainer"/> class to decorate instances
        /// of <paramref name="decoratedType"/> with new instances of <paramref name="decoratorType"/> which are
        /// created via constructor injection.
        /// </summary>
        /// <param name="root">Required.  The root <see cref="ITargetContainer"/> to which this decorating
        /// container will be registered.</param>
        /// <param name="decoratorType">Type of the decorator.</param>
        /// <param name="decoratedType">Type being decorated.</param>
        public DecoratingTargetContainer(ITargetContainer root, Type decoratorType, Type decoratedType)
            : this(root, decoratedType)
        {
            if(decoratorType == null) throw new ArgumentNullException(nameof(decoratorType));
            DecoratorFactory = (ta, ty) => Target.ForType(decoratorType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="decoratorTarget"></param>
        /// <param name="decoratedType"></param>
        public DecoratingTargetContainer(ITargetContainer root, ITarget decoratorTarget, Type decoratedType)
            : this(root, decoratedType)
        {
            if (decoratorTarget == null) throw new ArgumentNullException(nameof(decoratorTarget));
            DecoratorFactory = (ta, ty) => decoratorTarget;
        }

        private void EnsureInnerContainer()
        {
            if (Inner != null) return;
            //similar logic to the main TargetContainer class here - if the type we're decorating is a generic 
            //then we fire the GenericTargetContainer factory.  Otherwise, we'll use a TargetListContainer
            if (TypeHelpers.IsGenericType(DecoratedType))
            {
                Inner = new GenericTargetContainer(
                    Root,
                    TypeHelpers.IsGenericTypeDefinition(DecoratedType) ?
                    DecoratedType
                    : DecoratedType.GetGenericTypeDefinition());
            }
            else
                Inner = new TargetListContainer(Root, DecoratedType);
        }

        private DecoratorTarget CreateDecoratorTarget(ITarget decorated, Type type)
        {
            return new DecoratorTarget(DecoratorFactory, decorated, type);
        }

        /// <summary>
        /// Implements <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> by wrapping the 
        /// <paramref name="existing"/> container and returning itself.
        /// 
        /// This allows decorators to be applied on top of decorators; and decorators to be added after types
        /// have begun to be registered in another target container.
        /// </summary>
        /// <param name="existing">The existing <see cref="ITargetContainer" /> instance that this instance is to be combined with</param>
        /// <param name="type">The type that the combined container owner will be registered under.</param>
        /// <exception cref="InvalidOperationException">If this target container is already decorating another container</exception>
        public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (Inner != null)
                throw new InvalidOperationException("Already decorating another container");

            Inner = existing;
            return this;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Fetch(Type)"/> - wraps a special target around
        /// the target returned by the target container that's decorated by this one.
        /// </summary>
        /// <param name="type">Required.  The type for which an <see cref="ITarget" /> is to be retrieved.</param>
        /// <remarks>If the inner container returns null, then so does this one.</remarks>
        public ITarget Fetch(Type type)
        {
            if (Inner == null)
                return null;

            if (ShouldDecorate(type))
            {
                var result = Inner.Fetch(type);
                return result != null ? CreateDecoratorTarget(result, type) : null;
            }
            else
                return Inner.Fetch(type);
        }

        private bool ShouldDecorate(Type type)
        {
            return type == DecoratedType 
                || (TypeHelpers.IsGenericType(type)
                    && TypeHelpers.IsGenericTypeDefinition(DecoratedType)
                    && type.GetGenericTypeDefinition() == DecoratedType);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.FetchAll(Type)"/> - passes the call on to the inner 
        /// container that's decorated by this one, and then wraps each of those targets in a <see cref="DecoratorTarget"/> which
        /// represents the decoration logic for each instance.
        /// </summary>
        /// <param name="type">Required.  The type for which the <see cref="ITarget" /> instances are to be retrieved.</param>
        public IEnumerable<ITarget> FetchAll(Type type)
        {
            if (Inner == null)
                return Enumerable.Empty<ITarget>();
            if (ShouldDecorate(type))
                return Inner.FetchAll(type).Select(t => CreateDecoratorTarget(t, type));
            else
                return Inner.FetchAll(type);
        }

        /// <summary>
        /// Registers a target, either for the <paramref name="serviceType" /> specified or, if null, the <see cref="ITarget.DeclaredType" />
        /// of the <paramref name="target" />.  Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/>.
        /// </summary>
        /// <param name="target">Required.  The target to be registered</param>
        /// <param name="serviceType">Optional.  The type the target is to be registered against, if different
        /// from the <see cref="ITarget.DeclaredType" /> of the <paramref name="target" />.  If provided, then the <paramref name="target" />
        /// must be compatible with this type.</param>
        /// <remarks>The decorator target does not accept registrations directly; rather it passes the call on to its
        /// inner container which could be a <see cref="TargetListContainer"/>, or <see cref="GenericTargetContainer"/> in 
        /// the most basic cases; or it could be another <see cref="DecoratingTargetContainer"/> in situations where a type has had
        /// multiple decorators registered against it.</remarks>
        public void Register(ITarget target, Type serviceType = null)
        {
            EnsureInnerContainer();

            Inner.Register(target, serviceType);
        }

        /// <summary>
        /// Retrieves an existing container registered against the given <paramref name="type" />, or null if not found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>This is an implementation of <see cref="ITargetContainer.FetchContainer(Type)"/> which wraps
        /// around the inner target container and passes the call on to that.</remarks>
        public ITargetContainer FetchContainer(Type type)
        {
            EnsureInnerContainer();
            return Inner.FetchContainer(type);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> - the call is
        /// automatically forwarded on to the inner target container that's being decorated, since decorator targets don't support
        /// direct registration of targets or containers.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="container">The container.</param>
        public void RegisterContainer(Type type, ITargetContainer container)
        {
            EnsureInnerContainer();
            Inner.RegisterContainer(type, container);
        }
    }
}
