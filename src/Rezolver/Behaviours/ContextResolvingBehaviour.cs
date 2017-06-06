using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Behaviours
{
    /// <summary>
    /// An <see cref="ITargetContainerBehaviour"/> that enables automatic resolving of the 
    /// <see cref="IResolveContext"/> created for a <see cref="IContainer.Resolve(IResolveContext)"/> operation.
    /// </summary>
    /// <remarks>The implementation registers a special internal target type which implements <see cref="ICompiledTarget"/> 
    /// simply by returning the context passed to its <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method</remarks>
    public sealed class ContextResolvingBehaviour : ITargetContainerBehaviour
    {
        private class ResolveContextTarget : ITarget, ICompiledTarget
        {
            public bool UseFallback => false;

            public Type DeclaredType => typeof(IResolveContext);

            public ScopeBehaviour ScopeBehaviour => ScopeBehaviour.None;

            public ScopePreference ScopePreference => ScopePreference.Current;

            public ITarget SourceTarget => this;

            public object GetObject(IResolveContext context)
            {
                return context;
            }

            public bool SupportsType(Type type)
            {
                return type.Equals(typeof(IResolveContext));
            }
        }

        private readonly ResolveContextTarget _target = new ResolveContextTarget();

        private ContextResolvingBehaviour() { }
        
        /// <summary>
        /// The one and only instance of <see cref="ContextResolvingBehaviour"/>
        /// </summary>
        public static ContextResolvingBehaviour Instance { get; } = new ContextResolvingBehaviour();
        /// <summary>
        /// Attaches this behaviour to the target container, adding a registration to the <paramref name="targets"/>
        /// for the type <see cref="IResolveContext"/>.
        /// 
        /// Note - if the <paramref name="targets"/> already has a registration for <see cref="IResolveContext"/>,
        /// then the behaviour DOES NOT overwrite it.
        /// </summary>
        /// <param name="targets"></param>
        public void Attach(ITargetContainer targets)
        {
            var existing = targets.Fetch(typeof(IResolveContext));
            if (existing == null || existing.UseFallback)
                targets.Register(_target);
        }
    }
}
