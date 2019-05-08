// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Default scope object used by the<see cref="Container"/> - performs no actual lifetime management
    /// of objects at all, but can create child scopes and track them for disposal when it is disposed.
    /// 
    /// In order to get object lifetime management from a container whose scope is set to an instance of this type,
    /// a new scope must be created from this one.
    /// </summary>
    public class NonTrackingContainerScope : ContainerScope
    {
        private LockedList<ContainerScope> _childScopes
            = new LockedList<ContainerScope>(64);

        internal NonTrackingContainerScope(Container container)
            : base(container)
        {

        }

        internal NonTrackingContainerScope(ContainerScope parent, bool isRoot)
            : base(parent, isRoot)
        {

        }

        private protected override void OnDispose()
        {
            using (var listLock = this._childScopes.Lock())
            {
                // dispose child scopes in reverse order of creation
                for (var f = this._childScopes.Count; f > 0; f--)
                {
                    this._childScopes[f - 1].Dispose();
                }
            }

            this._childScopes = null;
        }

        private protected override void ChildDisposed(ContainerScope child)
        {
            if (!_isDisposing && !_isDisposed)
            {
                this._childScopes.Remove(child);
            }
        }

        /// <summary>
        /// Creates a new child scope from this scope.  The new scope will use the same container as this one,
        /// but will have its own lifetime, and will track its own instances of any 'scoped' objects.
        /// 
        /// When this scope is disposed, the new scope will be disposed also - unless it has already been disposed.
        /// </summary>
        /// <returns></returns>
        public sealed override ContainerScope CreateScope()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(NonTrackingContainerScope));

            var scope = base.CreateScope();
            this._childScopes.Add(scope);
            return scope;
        }
    }
}
