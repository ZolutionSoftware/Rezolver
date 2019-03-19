// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Targets
{
    /// <summary>
    /// Implements <see cref="ITarget"/> by wrapping a single instance that's already been constructed by application code.
    /// </summary>
    public class ObjectTarget : TargetBase, IResolvable
    {

        /// <summary>
        /// Gets the scope behaviour.
        /// </summary>
        /// <value>The scope behaviour.</value>
        public override ScopeBehaviour ScopeBehaviour { get; }

        /// <summary>
        /// Always returns <see cref="ScopePreference.Root"/>
        /// </summary>
        public override ScopePreference ScopePreference => ScopePreference.Root;

        /// <summary>
        /// Gets the declared type of object that is returned by this target.  Might be different from the type
        /// of <see cref="Value"/> if explicitly defined when this target was constructed.
        /// </summary>
        /// <value>The type of the declared.</value>
        public override Type DeclaredType { get; }

        /// <summary>
        /// Gets the value that will be exposed by expressions built by this instance.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; }

        private Func<ResolveContext, object> _factory;

        /// <summary>
        /// Creates a new instance of the <see cref="ObjectTarget"/> class.
        /// </summary>
        /// <param name="obj">The object to be returned by this target when resolved.</param>
        /// <param name="declaredType">Optional.  The declared type of this target, if different from the absolute type of the <paramref name="obj"/></param>
        /// <param name="scopeBehaviour">Optional.  If you want the object to be disposed by Rezolver when the root scope is disposed, then
        /// specify a behaviour other than the default.  Note - the only real behaviour that makes sense for this is <see cref="ScopeBehaviour.Explicit"/>,
        /// since the Implicit behaviour will typically fool a scope that multiple instances are being created and, therefore, the object
        /// will be tracked multiple times by that scope.</param>
        /// <remarks>Please note - if you enable scope tracking, but the object is never resolved, then the object will not be disposed and you will need
        /// to ensure you dispose of it.</remarks>
        public ObjectTarget(object obj, Type declaredType = null, ScopeBehaviour scopeBehaviour = Rezolver.ScopeBehaviour.None)
        {
            Value = obj;
            ScopeBehaviour = scopeBehaviour;
            // if the caller provides a declared type we check
            // also that, if the object is null, the target type
            // can accept nulls.  Otherwise we're simply checking
            // that the value that's supplied is compatible with the
            // type that is being declared.
            if (declaredType != null)
            {
                if (Value == null)
                {
                    if (!declaredType.CanBeNull())
                    {
                        throw new ArgumentException(string.Format(ExceptionResources.TargetIsNullButTypeIsNotNullable_Format, declaredType), "declaredType");
                    }
                }
                else if (!TypeHelpers.AreCompatible(Value.GetType(), declaredType))
                {
                    throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, Value.GetType()), "declaredType");
                }

                DeclaredType = declaredType;
            }
            else // an untyped null is typed as Object
            {
                DeclaredType = Value == null ? typeof(object) : Value.GetType();
            }

            switch (ScopeBehaviour)
            {
                case ScopeBehaviour.None:
                    _factory = (ResolveContext context) => Value;
                    break;
                case ScopeBehaviour.Implicit:
                    _factory = (ResolveContext context) => context.ActivateImplicit_RootScope(Value);
                    break;
                case ScopeBehaviour.Explicit:
                    Func<ResolveContext, object> activation = (ResolveContext cc) => Value;
                    _factory = (ResolveContext context) => context.ActivateExplicit_RootScope(this.Id, activation);
                    break;
            }
        }

        /// <summary>
        /// Implementation of <see cref="IResolvable.Factory"/>
        /// </summary>
        /// <returns>A factory delegate which returns the <see cref="Value"/></returns>
        public Func<ResolveContext, object> Factory() => _factory;
    }
}
