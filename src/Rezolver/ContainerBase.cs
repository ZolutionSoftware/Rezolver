// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Rezolver.Compilation;
using Rezolver.Events;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Starting point for implementations of <see cref="IContainer"/> - only creatable through inheritance.
    /// </summary>
    /// <remarks>This class also implements <see cref="ITargetContainer"/> by proxying the <see cref="Targets"/> that are
    /// provided to it on construction (or created anew if not supplied).  All of those interface methods are implemented
    /// explicitly except the <see cref="Register(ITarget, Type)"/> method,  which is available through the class' public
    /// API.
    ///
    /// Note: <see cref="IContainer"/>s are generally not expected to implement <see cref="ITargetContainer"/>, and the
    /// framework will never assume they do.
    ///
    /// The reason this class does is to make it easier to create a new container and to register targets into it without
    /// having to worry about managing a separate <see cref="ITargetContainer"/> instance in your application root -
    /// because all the registration extension methods defined in classes like
    /// <see cref="RegisterTypeTargetContainerExtensions"/>, <see cref="SingletonTargetContainerExtensions"/> (plus many
    /// more) will be available to developers in code which has a reference to this class, or one derived from it.
    ///
    /// Note also that calling <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> on an instance of this
    /// type will always cause a <see cref="NotSupportedException"/> to be thrown, thus preventing containers from being
    /// registered as sub target containers within an <see cref="ITargetContainer"/> via its
    /// <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> method.
    /// </remarks>
    public class ContainerBase : IContainer, IRootTargetContainer
    {
        /// <summary>
        /// Provides the <see cref="ITarget"/> instances that will be compiled into <see cref="ICompiledTarget"/>
        /// instances.
        /// </summary>
        /// <remarks>This class implements the <see cref="ITargetContainer"/> interface by wrapping around this instance so that
        /// an application can create an instance of <see cref="ContainerBase"/> and directly register targets into it;
        /// rather than having to create and setup the target container first.
        ///
        /// You can add registrations to this target container at any point in the lifetime of any
        /// <see cref="ContainerBase"/> instances which are attached to it.
        ///
        /// In reality, however, if any <see cref="Resolve(IResolveContext)"/> operations have been performed prior to
        /// adding more registrations, then there's no guarantee that new dependencies will be picked up - especially
        /// if the <see cref="CachingContainerBase"/> is being used as your application's container (which it nearly
        /// always will be).</remarks>
        protected IRootTargetContainer Targets { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBase"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup
        /// when <see cref="Resolve(IResolveContext)"/> (and other operations) is called.  If not provided, a new
        /// <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available to inherited types,
        /// after construction, through the <see cref="Targets"/> property.</param>
        protected ContainerBase(IRootTargetContainer targets = null)
        {
            this.Targets = targets ?? new TargetContainer();
        }

        event EventHandler<TargetRegisteredEventArgs> IRootTargetContainer.TargetRegistered
        {
            add
            {
                this.Targets.TargetRegistered += value;
            }

            remove
            {
                this.Targets.TargetRegistered -= value;
            }
        }

        event EventHandler<TargetContainerRegisteredEventArgs> IRootTargetContainer.TargetContainerRegistered
        {
            add
            {
                this.Targets.TargetContainerRegistered += value;
            }

            remove
            {
                this.Targets.TargetContainerRegistered -= value;
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IContainer.Resolve(IResolveContext)"/> method.
        ///
        /// Obtains an <see cref="ICompiledTarget"/> by calling the
        /// <see cref="GetCompiledTarget(IResolveContext)"/> method, and then immediately calls its
        /// <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method, returning the result.
        /// </summary>
        /// <param name="context">The context containing the type that's requested, any active scope and so on.</param>
        /// <returns></returns>
        public virtual object Resolve(IResolveContext context)
        {
            return this.GetCompiledTarget(context).GetObject(context);
        }

        /// <summary>
        /// Implementation of the <see cref="IContainer.TryResolve(IResolveContext, out object)"/> method.
        ///
        /// Attempts to resolve the requested type (given on the <paramref name="context"/>, returning a boolean
        /// indicating whether the operation was successful.  If successful, then <paramref name="result"/> receives
        /// a reference to the resolved object.
        /// </summary>
        /// <param name="context">The context containing the type that's requested, any active scope and so on.</param>
        /// <param name="result">Receives a reference to the object that was resolved, if successful, or <c>null</c>
        /// if not.</param>
        /// <returns>A boolean indicating whether the operation completed successfully.</returns>
        public virtual bool TryResolve(IResolveContext context, out object result)
        {
            var target = this.GetCompiledTarget(context);
            if (!target.IsUnresolved())
            {
                result = target.GetObject(context);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IScopeFactory.CreateScope"/> method.
        ///
        /// The base definition creates a <see cref="ContainerScope"/> with this container passed as the scope's container.
        ///
        /// Thus, the new scope is a 'root' scope.
        /// </summary>
        public virtual IContainerScope CreateScope()
        {
            return new ContainerScope(this);
        }

        /// <summary>
        /// Base implementation of <see cref="IContainer.GetCompiledTarget(IResolveContext)"/>.  Note that any container
        /// already defined in the <see cref="IResolveContext.Container"/> is ignored in favour of this container.
        /// </summary>
        /// <param name="context">The context containing the requested type and any scope which is currently in force.</param>
        /// <returns>Always returns a reference to a compiled target - but note that if
        /// <see cref="CanResolve(IResolveContext)"/> returns false for the same context, then the target's
        /// <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method will likely throw an exception - in line with
        /// the behaviour of the <see cref="UnresolvedTypeCompiledTarget"/> class.</returns>
        public ICompiledTarget GetCompiledTarget(IResolveContext context)
        {
            // note that this container is fixed as the container in the context - regardless of the
            // one passed in.  This is important.  Scope and RequestedType are left unchanged
            return this.GetCompiledTargetVirtual(context.New(newContainer: this));
        }

        /// <summary>
        /// Implementation of the <see cref="IContainer.CanResolve(IResolveContext)"/> method.  Returns true if, and only
        /// if, the <see cref="Targets"/> <see cref="ITargetContainer"/> returns a non-null <see cref="ITarget"/> when the
        /// <see cref="IResolveContext.RequestedType"/> is passed to its <see cref="ITargetContainer.Fetch(Type)"/> method.
        /// </summary>
        /// <param name="context">The resolve context containing the requested type.</param>
        public virtual bool CanResolve(IResolveContext context)
        {
            return this.Targets.Fetch(context.RequestedType) != null;
        }

        /// <summary>
        /// The main workhorse of the resolve process - obtains an <see cref="ICompiledTarget"/> for the given
        /// <paramref name="context"/> by looking up an <see cref="ITarget"/> from the <see cref="Targets"/> target
        /// container, then compiling it.
        /// </summary>
        /// <param name="context">The current resolve context</param>
        /// <remarks>The specifics of how this process works are not important if you simply want to use the container,
        /// but if you are looking to extend it, then it's essential you understand the different steps that the process
        /// goes through:
        ///
        /// ### Locating the target
        ///
        /// If the <see cref="ITargetContainer.Fetch(Type)"/> method of the <see cref="Targets"/> target container
        /// returns a <c>null</c> <see cref="ITarget"/>, or one which has its <see cref="ITarget.UseFallback"/> set to
        /// <c>true</c>, then the method gets an alternative compiled target by calling the
        /// <see cref="GetFallbackCompiledTarget(IResolveContext)"/> method.
        ///
        /// This fallback will be used unless the target was not null and its <see cref="ITarget.UseFallback"/> is
        /// true *AND* the compiled target returned by the fallback method is an <see cref="UnresolvedTypeCompiledTarget"/>.
        ///
        /// ### To compile, or not to compile
        ///
        /// Before proceeding with compilation, the code checks whether the target can resolve the required object
        /// directly.
        ///
        /// This means that the target either implements the <see cref="ICompiledTarget"/> interface (in which case it is
        /// immediately returned) or the <see cref="IResolveContext.RequestedType"/> is *not* <see cref="Object"/> and the
        /// target's type is compatible with it (in which case the target is simply embedded in a new
        /// <see cref="ConstantCompiledTarget"/>, which will later just return the target when its
        /// <see cref="ConstantCompiledTarget.GetObject(IResolveContext)"/> is called).
        ///
        /// <em>The <see cref="ObjectTarget"/> supports the <see cref="ICompiledTarget"/> interface, therefore any objects
        /// which are directly registered through this target will always use that class' implementation of
        /// <see cref="ICompiledTarget"/> if requested through the <see cref="Resolve(IResolveContext)"/> method.</em>
        ///
        /// * * *
        ///
        /// Once the decision has been taken to compile the target, the container first needs a compiler
        /// (<see cref="Compilation.ITargetCompiler"/>).
        ///
        /// This is obtained by resolving it directly from the <see cref="IResolveContext.Container"/> of the
        /// <paramref name="context"/> (since a container can be delegated to from another container which originally
        /// received the <see cref="Resolve(IResolveContext)"/> call).  Attentive readers will realise at this point
        /// that this could lead to an infinite recursion - i.e. since compiling a target means resolving a compiler, which
        /// in turn must mean compiling that target.
        ///
        /// The class sidesteps this potential pitfall by requiring that a target registered for <see cref="ITargetCompiler"/>
        /// supports direct resolving, as per the description a couple of paragraphs back.  Therefore, compilers and context
        /// providers are typically registered as constant services via the <see cref="ObjectTarget"/> target.
        ///
        /// Finally, a new <see cref="ICompileContext"/> is created via the
        /// <see cref="ITargetCompiler.CreateContext(IResolveContext, ITargetContainer)"/> method of the resolved
        /// context provider, and then passed to the <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/>
        /// method of the resolved compiler.  The result of that operation is then returned to the caller.
        /// </remarks>
        protected virtual ICompiledTarget GetCompiledTargetVirtual(IResolveContext context)
        {
            ITarget target = this.Targets.Fetch(context.RequestedType);

            if (target == null)
            {
                return this.GetFallbackCompiledTarget(context);
            }

            // if the entry advises us to fall back if possible, then we'll see what we get from the
            // fallback operation.  If it's NOT the unresolved target, then we'll use that instead
            if (target.UseFallback)
            {
                var fallback = this.GetFallbackCompiledTarget(context);
                if (!fallback.IsUnresolved())
                {
                    return fallback;
                }
            }

            // if the target also supports the ICompiledTarget interface then return it, bypassing the
            // need for any direct compilation.
            // Then check whether the type of the target is compatible with the requested type - so long
            // as the requested type is not System.Object.  If so, return a ConstantCompiledTarget
            // which will simply return the target when GetObject is called.
            // note that we don't check for IDirectTarget - because that can't honour scoping rules
            if (target is ICompiledTarget compiledTarget)
            {
                return compiledTarget;
            }
            else if (context.RequestedType != typeof(object) && TypeHelpers.IsAssignableFrom(context.RequestedType, target.GetType()))
            {
                return new ConstantCompiledTarget(target, target);
            }

            var compiler = this.Targets.GetOption<ITargetCompiler>(target.GetType());
            if (compiler == null)
            {
                throw new InvalidOperationException($"No compiler has been configured in the Targets target container for a target of type {target.GetType()} - please use the SetOption API to set an ITargetCompiler for all target types, or for specific target types.");
            }

            return compiler.CompileTarget(target, context.New(newContainer: this), this.Targets);
        }

        /// <summary>
        /// Called by <see cref="GetCompiledTarget(IResolveContext)"/> if no valid <see cref="ITarget"/> can be
        /// found for the <paramref name="context"/> or if the one found has its <see cref="ITarget.UseFallback"/> property
        /// set to <c>true</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>An <see cref="ICompiledTarget"/> to be used as the result of a <see cref="Resolve(IResolveContext)"/>
        /// operation where the search for a valid target either fails or is inconclusive (e.g. - empty enumerables).
        /// </returns>
        /// <remarks>The base implementation always returns an instance of the <see cref="UnresolvedTypeCompiledTarget"/>.</remarks>
        protected virtual ICompiledTarget GetFallbackCompiledTarget(IResolveContext context)
        {
            return new UnresolvedTypeCompiledTarget(context.RequestedType);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(serviceType);
        }

        /// <summary>
        /// Protected virtual implementation of <see cref="IServiceProvider.GetService(Type)"/>.
        ///
        /// Uses the <see cref="TryResolve(IResolveContext, out object)"/> method to resolve the service, returning
        /// <c>null</c> if the operation fails.
        /// </summary>
        /// <param name="serviceType">Type of service to be resolved.</param>
        /// <returns></returns>
        protected virtual object GetService(Type serviceType)
        {
            // IServiceProvider should return null if not found - so we use TryResolve.
            this.TryResolve(serviceType, out object toReturn);
            return toReturn;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/> - simply proxies the
        /// call to the target container referenced by the <see cref="Targets"/> property.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serviceType"></param>
        /// <remarks>Remember: registering new targets into an <see cref="ITargetContainer"/> after an
        /// <see cref="IContainer"/> has started compiling targets within it can yield unpredictable results.
        ///
        /// If you create a new container and perform all your registrations before you use it, however, then everything
        /// will work as expected.
        ///
        /// Note also the other ITargetContainer interface methods are implemented explicitly so as to hide them from the
        /// list of class members.
        /// </remarks>
        public void Register(ITarget target, Type serviceType = null)
        {
            this.Targets.Register(target, serviceType);
        }

        #region ITargetContainer explicit implementation
        ITarget ITargetContainer.Fetch(Type type) => this.Targets.Fetch(type);

        IEnumerable<ITarget> ITargetContainer.FetchAll(Type type) => this.Targets.FetchAll(type);

        ITargetContainer ITargetContainer.CombineWith(ITargetContainer existing, Type type) => throw new NotSupportedException();

        ITargetContainer ITargetContainer.FetchContainer(Type type) => this.Targets.FetchContainer(type);

        void ITargetContainer.RegisterContainer(Type type, ITargetContainer container) => this.Targets.RegisterContainer(type, container);

        void ICovariantTypeIndex.AddKnownType(Type serviceType) => this.Targets.AddKnownType(serviceType);

        IEnumerable<Type> ICovariantTypeIndex.GetKnownCovariantTypes(Type serviceType) => Targets.GetKnownCovariantTypes(serviceType);

        IEnumerable<Type> ICovariantTypeIndex.GetKnownCompatibleTypes(Type serviceType) => Targets.GetKnownCompatibleTypes(serviceType);

        TargetTypeSelector ICovariantTypeIndex.SelectTypes(Type type) => Targets.SelectTypes(type);

        #endregion
    }
}
