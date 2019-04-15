// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Reflection;

namespace Rezolver.Targets
{
    /// <summary>
    /// An <see cref="ITarget" /> which resolve objects by executing a delegate with argument injection.
    /// </summary>
    /// <remarks>The delegate must be non-void and can have any number of parameters.
    ///
    /// A compiler must ensure that any parameters for the <see cref="Factory"/> are automatically
    /// resolved from the container, and that a parameter of the type <see cref="ResolveContext"/>
    /// will receive the context passed to the <see cref="IContainer.Resolve(ResolveContext)"/>
    /// method call for which this target is being compiled and/or executed.</remarks>
    public class DelegateTarget : TargetBase
    {
        /// <summary>
        /// Gets the factory delegate that will be invoked when this target is compiled and executed
        /// </summary>
        /// <value>The factory.</value>
        public Delegate Factory { get; }

        private readonly ScopeBehaviour _scopeBehaviour;
        
        /// <summary>
        /// Overrides <see cref="TargetBase.ScopeBehaviour"/> to return the value that's passed on construction
        /// through the <see cref="DelegateTarget.DelegateTarget(Delegate, Type, ScopeBehaviour, ScopePreference)"/>
        /// constructor.
        /// </summary>
        public override ScopeBehaviour ScopeBehaviour => _scopeBehaviour;

        private readonly ScopePreference _scopePreference;

        /// <summary>
        /// Overrides <see cref="TargetBase.ScopePreference"/> to return the value that's passed on construction
        /// through the <see cref="DelegateTarget.DelegateTarget(Delegate, Type, ScopeBehaviour, ScopePreference)"/>
        /// constructor.
        /// </summary>
        public override ScopePreference ScopePreference => _scopePreference;

        /// <summary>
        /// Gets the MethodInfo for the <see cref="Factory"/> delegate.
        /// </summary>
        /// <remarks>Whilst this can be easily obtained from the delegate yourself (by using the
        /// <see cref="RuntimeReflectionExtensions.GetMethodInfo(Delegate)"/> extension method) however, this
        /// class also uses it to determine the <see cref="DeclaredType"/> of the target or whether the delegate
        /// is actually compatible with the one supplied on construction, therefore if you need to introspect
        /// the delegate, you might as well use this.</remarks>
        public MethodInfo FactoryMethod { get; }

        private readonly Type _declaredType;

        /// <summary>
        /// Gets the declared type of object that is constructed by this target, either set on
        /// construction or derived from the return type of the <see cref="Factory"/>
        /// </summary>
        public override Type DeclaredType
        {
            get
            {
                return this._declaredType ?? FactoryMethod.ReturnType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTarget" /> class.
        /// </summary>
        /// <param name="factory">Required - the factory delegate.  Must have a return type and can take
        /// 0 or more parameters.</param>
        /// <param name="declaredType">Optional - type that will be set into the <see cref="DeclaredType" /> for the target;
        /// if not provided, then it will be derived from the <paramref name="factory" />'s return type</param>
        /// <param name="scopeBehaviour">Scope behaviour for this delegate.  The default is <see cref="ScopeBehaviour.Implicit"/>, which means
        /// that that any returned instance will be tracked implicitly by the active scope.  If the delegate produces a new instance, then 
        /// this or <see cref="ScopeBehaviour.Explicit"/> can be used safely - the choice being whether
        /// the expression should produce one instance per scope, or should act as a disposable transient object.</param>
        /// <param name="scopePreference">If <paramref name="scopeBehaviour"/> is not <see cref="ScopeBehaviour.None"/>, then this controls
        /// the preferred scope for the instance to be tracked.  Defaults to <see cref="ScopePreference.Current"/></param>
        /// <exception cref="ArgumentException">If the <paramref name="factory" /> represents a void delegate or if
        /// <paramref name="declaredType" /> is passed but the type is not compatible with the return type of
        /// <paramref name="factory" />.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="factory" /> is null</exception>
        public DelegateTarget(
            Delegate factory, 
            Type declaredType = null, 
            ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit,
            ScopePreference scopePreference = ScopePreference.Current)
        {
            if(factory == null) throw new ArgumentNullException(nameof(factory));

            FactoryMethod = factory.GetMethodInfo();

            if (FactoryMethod.ReturnType == typeof(void))
                throw new ArgumentException("Factory must have a return type", nameof(factory));
            if (FactoryMethod.GetParameters().Any(p => p.ParameterType.IsByRef))
                throw new ArgumentException("Delegates which have ref or out parameters are not permitted as the factory argument", nameof(factory));

            if (declaredType != null)
            {
                if (!TypeHelpers.AreCompatible(FactoryMethod.ReturnType, declaredType) && !TypeHelpers.AreCompatible(declaredType, FactoryMethod.ReturnType))
                {
                    throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, FactoryMethod.ReturnType), nameof(declaredType));
                }
            }

            this._declaredType = declaredType;
            this._scopeBehaviour = scopeBehaviour;
            Factory = factory;
        }
    }
}
