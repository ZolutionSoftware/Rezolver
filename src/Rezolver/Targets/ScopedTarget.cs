// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver.Targets
{
    /// <summary>
    /// A target that produces or fetches a single instance of an object within a lifetime scope.
    /// </summary>
    /// <remarks>Scopes in Rezolver operate much the same as they do in any IOC framework, but use
    /// of them is always entirely optional.
    ///
    /// When a scope is active for a given <see cref="IContainer.Resolve(ResolveContext)"/> operation, most
    /// objects which are returned from those operations will implicitly be scoped to whichever scope is active
    /// when the objects are resolved.  Implicitly scoped objects are only disposed of when their containing
    /// scope is disposed, and you can have an unlimited number of instances of implicitly scoped objects
    /// per scope.
    ///
    /// This target is used to scope the object produced by a target *explicitly* to a scope, and to ensure
    /// that only *one* instance of that object is produced per scope.  Such objects are also not inherited between
    /// parent scopes and child scopes.</remarks>
    public class ScopedTarget : TargetBase
    {
        /// <summary>
        /// Gets the inner target whose result (when compiled) will be scoped to the active scope.
        /// </summary>
        /// <value>The inner target.</value>
        public ITarget InnerTarget { get; }

        /// <summary>
        /// Gets the declared type of object that is constructed by this target.
        /// </summary>
        /// <remarks>Always forwards the call on to <see cref="InnerTarget"/></remarks>
        public override Type DeclaredType
        {
            get { return InnerTarget.DeclaredType; }
        }

        /// <summary>
        /// Always returns <see cref="ScopeBehaviour.None"/>
        /// </summary>
        /// <value>The scope behaviour.</value>
        public override ScopeBehaviour ScopeBehaviour
        {
            get
            {
                return ScopeBehaviour.Explicit;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedTarget"/> class.
        /// </summary>
        /// <param name="innerTarget">Required.  The inner target.</param>
        public ScopedTarget(ITarget innerTarget)
        {
            if(innerTarget == null) throw new ArgumentNullException(nameof(innerTarget));
            InnerTarget = innerTarget;
        }

        /// <summary>
        /// Called to check whether a target can create an expression that builds an instance of the given <paramref name="type" />.
        /// </summary>
        /// <param name="type">Required</param>
        /// <remarks>Always forwards the call on the <see cref="InnerTarget"/></remarks>
        public override bool SupportsType(Type type)
        {
            return InnerTarget.SupportsType(type);
        }
    }
}
