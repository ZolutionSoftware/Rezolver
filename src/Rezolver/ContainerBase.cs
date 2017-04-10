// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;
using Rezolver.Targets;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Rezolver
{
    /// <summary>
    /// Starting point for implementations of <see cref="IContainer"/> - only creatable through inheritance.
    /// </summary>
    /// <remarks>This class also implements <see cref="ITargetContainer"/> by proxying the <see cref="Targets"/> that are
    /// provided to it on construction (or created anew if not supplied).  All of those interface methods are implemented 
    /// explicitly except the <see cref="Register(ITarget, Type)"/> method,  which is available through the class' public API.
    /// 
    /// Note: <see cref="IContainer"/>s are generally not expected to implement <see cref="ITargetContainer"/>, and the framework
    /// will never assume they do.
    /// 
    /// The reason this class does is to make it easier to create a new container and to register targets into it without having to worry about
    /// managing a separate <see cref="ITargetContainer"/> instance in your application root - because all the registration extension methods defined
    /// in classes like <see cref="RegisterTypeTargetContainerExtensions"/>, <see cref="SingletonTargetContainerExtensions"/> plus many more
    /// will be available to developers in code which has a reference to this class, or one derived from it.
    /// 
    /// Note also that calling <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> on an instance of this type will always
    /// cause a <see cref="NotSupportedException"/> to be thrown.
    /// </remarks>
    public class ContainerBase : IContainer, ITargetContainer
    {
        #region NoChangeCompilerConfigurationProvider class
        private class NoChangeCompilerConfigurationProvider : IContainerConfiguration
        {
            ///<summary>Empty implementation of <see cref="IContainerConfiguration.Configure(IContainer, ITargetContainer)"/></summary>
            public void Configure(IContainer container, ITargetContainer targets)
            {

            }
        }
        #endregion

        #region MissingCompiledTarget

        /// <summary>
        /// Used as a sentinel type when a type cannot be resolved by a <see cref="ContainerBase"/> instance.  Instead of returning a null
        /// <see cref="ICompiledTarget"/> instance, the container will construct an instance of this type (typically through <see cref="GetMissingTarget(Type)"/>,
        /// which caches singleton instances of this class on a per-type basis) which can then be used just as if the lookup succeeded.
        /// </summary>
        /// <seealso cref="Rezolver.ICompiledTarget" />
        /// <remarks>The <see cref="GetObject(IResolveContext)"/> always throws an <see cref="InvalidOperationException"/> with the message
        /// 'Could resolve type [[type]]'</remarks>
        protected class MissingCompiledTarget : ICompiledTarget
        {
            private readonly Type _type;

            public ITarget SourceTarget => null;
            /// <summary>
            /// Constructs a new instance of the <see cref="MissingCompiledTarget"/> class.
            /// </summary>
            /// <param name="type"></param>
            public MissingCompiledTarget(Type type)
            {
                _type = type;
            }

            /// <summary>
            /// Implementation of <see cref="ICompiledTarget.GetObject(IResolveContext)"/>.  Always throws an <see cref="InvalidOperationException"/>.
            /// </summary>
            /// <param name="context">The current rezolve context.</param>
            /// <exception cref="InvalidOperationException">Always thrown.</exception>
            public object GetObject(IResolveContext context)
            {
                throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
            }
        }
        #endregion

        #region DirectResolveCompiledTarget

        /// <summary>
        /// Used when an <see cref="ITarget"/> is also of the same type as the one for which it is returned
        /// by the <see cref="ITargetContainer.Fetch(Type)"/> method of the <see cref="ContainerBase.Targets"/> container.
        /// 
        /// In this case, the target is not compiled, but instead is simply returned as the desired object.
        /// </summary>
        protected class DirectResolveCompiledTarget : ICompiledTarget
        {
            public ITarget SourceTarget { get; }
            /// <summary>
            /// Constructs a new instance of the <see cref="DirectResolveCompiledTarget"/>
            /// </summary>
            /// <param name="target"></param>
            public DirectResolveCompiledTarget(ITarget target)
            {
                SourceTarget = target;
            }

            /// <summary>
            /// Implementation of <see cref="ICompiledTarget.GetObject(IResolveContext)"/> - simply returns the
            /// target with which this instance was constructed.
            /// </summary>
            /// <param name="context">ignored</param>
            /// <returns></returns>
            public object GetObject(IResolveContext context)
            {
                return SourceTarget;
            }
        }
        #endregion

        /// <summary>
        /// Gets the compiler configuration provider to be passed when a derived container does not want the
        /// <see cref="CompilerConfiguration.DefaultProvider" /> provider to be used if one is not passed on construction.
        /// 
        /// This provider is guaranteed not to add/modify any registrations in the underlying target container
        /// which are connected with compilation.
        /// </summary>
        protected static IContainerConfiguration NoChangeCompilerConfiguration { get; } = new NoChangeCompilerConfigurationProvider();


        private static readonly
          ConcurrentDictionary<Type, Lazy<ICompiledTarget>> MissingTargets = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();

        /// <summary>
        /// Gets an <see cref="ICompiledTarget"/> for the given type which will always throw an <see cref="InvalidOperationException"/> whenever its
        /// <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method is called.  Use this when you can't resolve a target for a type.
        /// </summary>
        /// <param name="type">The type for which you wish to create a missing target.</param>
        protected static ICompiledTarget GetMissingTarget(Type type)
        {
            return MissingTargets.GetOrAdd(type, t => new Lazy<ICompiledTarget>(() => new MissingCompiledTarget(t))).Value;
        }

        /// <summary>
        /// Determines whether the given <paramref name="target"/> is an instance of <see cref="MissingCompiledTarget"/>.
        /// </summary>
        /// <param name="target">The target.</param>
        protected static bool IsMissingTarget(ICompiledTarget target)
        {
            return target is MissingCompiledTarget;
        }

        private int _compileConfigured = 0;
        private IContainerConfiguration _compilerConfigurationProvider;
        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBase"/>, optionally initialising it with the given <paramref name="targets"/> and <paramref name="compilerConfig"/>
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when <see cref="Resolve(IResolveContext)"/> (and other operations)
        /// is called.  If not provided, a new <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available to inherited types, after construction, through the 
        /// <see cref="Targets"/> property.</param>
        /// <param name="compilerConfig">Optional.  An object which will be used to configure this container and its targets to use a specific compilation
        /// strategy.  If <c>null</c>, then the <see cref="CompilerConfiguration.DefaultProvider"/> provider will be used.</param>
        protected ContainerBase(ITargetContainer targets = null, IContainerConfiguration compilerConfig = null)
        {
            Targets = targets ?? new TargetContainer();
            _compilerConfigurationProvider = compilerConfig ?? CompilerConfiguration.DefaultProvider;
        }

        /// <summary>
        /// Provides the <see cref="ITarget"/> instances that will be compiled into <see cref="ICompiledTarget"/>
        /// instances.
        /// </summary>
        /// <remarks>Notes to implementers: This property must NEVER be null.
        /// 
        /// This class implements the <see cref="ITargetContainer"/> interface by wrapping around this instance so that an application can create 
        /// an instance of <see cref="ContainerBase"/> and directly register targets into it; rather than having to create and setup the target container
        /// first.
        /// 
        /// You can add registrations to this target container at any point in the lifetime of any <see cref="ContainerBase"/> instances which are attached
        /// to it.  In reality, however, if any <see cref="Resolve(IResolveContext)"/> operations have been performed prior to adding more registrations,
        /// then there's no guarantee that new dependencies will be picked up - especially if the <see cref="CachingContainerBase"/> is being used as your
        /// application's container (which it nearly always will be).</remarks>
        protected ITargetContainer Targets { get; }

        /// <summary>
        /// Implementation of the <see cref="IContainer.Resolve(IResolveContext)"/> method.
        /// 
        /// Obtains an <see cref="ICompiledTarget"/> by calling the <see cref="GetCompiledRezolveTarget(IResolveContext)"/> method, and
        /// then immediately calls its <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method, returning the result.
        /// </summary>
        /// <param name="context">The context containing the type that's requested, any active scope and so on.</param>
        /// <returns></returns>
        public virtual object Resolve(IResolveContext context)
        {
            return GetCompiledRezolveTarget(context).GetObject(context);
        }

        /// <summary>
        /// Implementation of the <see cref="IContainer.TryResolve(IResolveContext, out object)"/> method.
        /// 
        /// Attempts to resolve the requested type (given on the <paramref name="context"/>, returning a boolean
        /// indicating whether the operation was successful.  If successful, then <paramref name="result"/> receives
        /// a reference to the resolved object.
        /// </summary>
        /// <param name="context">The context containing the type that's requested, any active scope and so on.</param>
        /// <param name="result">Receives a reference to the object that was resolved, if successful, or <c>null</c> if not.</param>
        /// <returns>A boolean indicating whether the operation completed successfully.</returns>
        public virtual bool TryResolve(IResolveContext context, out object result)
        {
            var target = GetCompiledRezolveTarget(context);
            if (!IsMissingTarget(target))
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
        /// Base implementation of <see cref="IContainer.FetchCompiled(IResolveContext)"/>.  Note that any container
        /// already defined in the <see cref="IResolveContext.Container"/> is ignored in favour of this container.
        /// </summary>
        /// <param name="context">The context containing the requested type and any scope which is currently in force.</param>
        /// <returns>Always returns a reference to a compiled target - but note that if <see cref="CanResolve(IResolveContext)"/>
        /// returns false for the same context, then the target's <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method 
        /// will likely throw an exception - in line with the behaviour of the <see cref="MissingCompiledTarget"/> class' behaviour.</returns>
        public virtual ICompiledTarget FetchCompiled(IResolveContext context)
        {
            //note that this container is fixed as the container in the context - regardless of the
            //one passed in.  This is important.  Scope and requestedType are left unchanged
            return GetCompiledRezolveTarget(context.New(newContainer: this));
        }

        /// <summary>
        /// Implementation of the <see cref="IContainer.CanResolve(IResolveContext)"/> method.  Returns true if, and only if,
        /// the <see cref="Targets"/> <see cref="ITargetContainer"/> returns a non-null <see cref="ITarget"/> when the 
        /// <see cref="IResolveContext.RequestedType"/> is passed to its <see cref="ITargetContainer.Fetch(Type)"/> method.
        /// </summary>
        /// <param name="context">The resolve context containing the requested type.</param>
        public virtual bool CanResolve(IResolveContext context)
        {
            return Targets.Fetch(context.RequestedType) != null;
        }

        /// <summary>
        /// The main workhorse of the resolve process - obtains an <see cref="ICompiledTarget"/> for the given <paramref name="context"/>
        /// by looking up an <see cref="ITarget"/> from the <see cref="Targets"/> target container, then compiling it.
        /// </summary>
        /// <param name="context">The current resolve context</param>
        /// <remarks>The specifics of how this process works are not important if you simply want to use the container, 
        /// but if you are looking to extend it, then it's essential you understand the different steps that the process
        /// goes through.
        /// 
        /// If the <see cref="ITargetContainer.Fetch(Type)"/> method of the <see cref="Targets"/> target container
        /// returns a <c>null</c> <see cref="ITarget"/>, or one which has its <see cref="ITarget.UseFallback"/> set to 
        /// <c>true</c>, then the method gets an alternative compiled target by calling the
        /// <see cref="GetFallbackCompiledRezolveTarget(IResolveContext)"/> method.  This fallback compiled target will be
        /// used instead of compiling the target unless the target was not null and its <see cref="ITarget.UseFallback"/> is true
        /// *AND* the compiled target returned by the fallback method is a <see cref="MissingCompiledTarget"/> - in which case
        /// the fallback target will be compiled as normal.
        /// 
        /// Before proceeding with compilation, the container checks whether the target can resolve the required object directly.
        /// 
        /// This means that the target either implements the <see cref="ICompiledTarget"/> interface (in which case it is immediately
        /// returned) or the <see cref="IResolveContext.RequestedType"/> is *not* <see cref="Object"/> and the target's type 
        /// is compatible with it (in which case the target is simply embedded in a new <see cref="DirectResolveCompiledTarget"/>, which
        /// will later just return the target when its <see cref="DirectResolveCompiledTarget.GetObject(IResolveContext)"/> is called).
        /// 
        /// <em>The <see cref="ObjectTarget"/> supports the <see cref="ICompiledTarget"/> interface, therefore any objects which are
        /// directly registered through this target will always use that class' implementation of <see cref="ICompiledTarget"/> if
        /// requested through the <see cref="Resolve(IResolveContext)"/> method.</em>
        /// 
        /// Once the decision has been taken to compile the target, the container first needs a compiler (<see cref="Compilation.ITargetCompiler"/>)
        /// and a compile context provider (<see cref="Compilation.ICompileContextProvider"/>).
        /// 
        /// <em>Note that classes which implement the <see cref="ITargetCompiler"/> interface also frequently implement the 
        /// <see cref="ICompileContextProvider"/> interface so that any additional state they require is correctly attached 
        /// to the <see cref="ICompileContext"/> which will be fed to their <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> 
        /// implementation.</em>
        /// 
        /// These are both obtained by resolving them directly from the <see cref="IResolveContext.Container"/> of the 
        /// <paramref name="context"/> (since a container can be delegated to from another container which originally
        /// received the <see cref="Resolve(IResolveContext)"/> call).  Attentive readers will realise at this point 
        /// that this could lead to an infinite recursion - i.e. since compiling a target means resolving a compiler, which
        /// in turn must mean compiling that target.
        /// 
        /// The class sidesteps this potential pitfall by requiring that the targets registered for these types support direct 
        /// resolving, as per the description a couple of paragraphs back.  Therefore, compilers and context providers are typically
        /// registered as objects via the <see cref="ObjectTarget"/> target.
        /// 
        /// Finally, a new <see cref="ICompileContext"/> is created via the 
        /// <see cref="ICompileContextProvider.CreateContext(IResolveContext, ITargetContainer, IContainer)"/> method of the resolved
        /// context provider, and then passed to the <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> method
        /// of the resolved compiler.  The result of that operation is then returned to the caller.
        /// </remarks> 
        protected virtual ICompiledTarget GetCompiledRezolveTarget(IResolveContext context)
        {
            if (Interlocked.CompareExchange(ref _compileConfigured, 1, 0) == 0)
                _compilerConfigurationProvider.Configure(this, Targets);

            ITarget target = Targets.Fetch(context.RequestedType);

            if (target == null)
                return GetFallbackCompiledRezolveTarget(context);

            //if the entry advises us to fall back if possible, then we'll see what we get from the 
            //fallback operation.  If it's NOT the missing target, then we'll use that instead
            if (target.UseFallback)
            {
                var fallback = GetFallbackCompiledRezolveTarget(context);
                if (!IsMissingTarget(fallback))
                    return fallback;
            }

            //if the target also supports the ICompiledTarget interface then return it, bypassing the
            //need for any direct compilation.
            //also check whether the type of the target is compatible with the requested type - so long
            //as the requested type is not System.Object.  If so, return a DirectResolveCompiledTarget
            //which will simply return the target when GetObject is called.
            if (target is ICompiledTarget)
                return (ICompiledTarget)target;
            else if (context.RequestedType != typeof(object) && TypeHelpers.IsAssignableFrom(context.RequestedType, target.GetType()))
                return new DirectResolveCompiledTarget(target);

            //if ITargetCompiler or ICompileContextProvider have been requested then we can't continue, 
            //because in order to compile this target we'd need a compiler we can use which, by definition, we don't have.
            //also, the same error occurs if the container in question is unable to resolve the type.
            //note that this could trigger this same line of code to blow in the context container.
            ITargetCompiler compiler;
            ICompileContextProvider compileContextProvider;

            if (context.RequestedType == typeof(ITargetCompiler) || !context.Container.TryResolve(out compiler))
                throw new InvalidOperationException("The compiler has not been correctly configured for this container.  It must be registered in the container's targets and must be a target that supports the ICompiledTarget interface.");
            if (context.RequestedType == typeof(ICompileContextProvider) || !context.Container.TryResolve(out compileContextProvider))
                throw new InvalidOperationException("Could not obtain an ICompileContextProvider for the operation");

            //
            return compiler.CompileTarget(
                target,
                compileContextProvider.CreateContext(
                    context,
                    //the targets we pass here are wrapped in a new ChildBuilder by the context
                    Targets)
                );
        }

        /// <summary>
        /// Called by <see cref="GetCompiledRezolveTarget(IResolveContext)"/> if no valid <see cref="ITarget"/> can be
        /// found for the <paramref name="context"/> or if the one found has its <see cref="ITarget.UseFallback"/> property
        /// set to <c>true</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>An <see cref="ICompiledTarget"/> to be used as the result of a <see cref="Resolve(IResolveContext)"/> operation
        /// where the search for a valid target either fails or is inconclusive (e.g. - empty enumerables).</returns>
        /// <remarks>The base implementation always returns an instance of the <see cref="MissingCompiledTarget"/> via
        /// the <see cref="GetMissingTarget(Type)"/> static method.</remarks>
        protected virtual ICompiledTarget GetFallbackCompiledRezolveTarget(IResolveContext context)
        {
            return GetMissingTarget(context.RequestedType);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return GetService(serviceType);
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
            //IServiceProvider should return null if not found - so we use TryResolve.
            object toReturn = null;
            this.TryResolve(serviceType, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/> - simply proxies the
        /// call to the target container referenced by the <see cref="Targets"/> property.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serviceType"></param>
        /// <remarks>Remember: registering new targets into an <see cref="ITargetContainer"/> after an <see cref="IContainer"/>
        /// has started compiling targets within it can yield unpredictable results.
        /// 
        /// If you create a new container and perform all your registrations before you use it, however, then everything will 
        /// work as expected.
        /// 
        /// Note also the other ITargetContainer interface methods are implemented explicitly so as to hide them from the 
        /// list of class members.
        /// </remarks>
        public void Register(ITarget target, Type serviceType = null)
        {
            Targets.Register(target, serviceType);
        }

        #region ITargetContainer explicit implementation
        ITarget ITargetContainer.Fetch(Type type)
        {
            return Targets.Fetch(type);
        }

        IEnumerable<ITarget> ITargetContainer.FetchAll(Type type)
        {
            return Targets.FetchAll(type);
        }

        ITargetContainer ITargetContainer.CombineWith(ITargetContainer existing, Type type)
        {
            throw new NotSupportedException();
        }

        ITargetContainer ITargetContainer.FetchContainer(Type type)
        {
            return Targets.FetchContainer(type);
        }

        void ITargetContainer.RegisterContainer(Type type, ITargetContainer container)
        {
            Targets.RegisterContainer(type, container);
        }
        #endregion
    }
}
