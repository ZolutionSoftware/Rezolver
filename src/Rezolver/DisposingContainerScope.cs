// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Fully functional container scope.  Supports tracking of implicitly and explicitly scoped objects to enable disposal.
    /// </summary>
    public sealed class DisposingContainerScope : NonTrackingContainerScope
    {
        // TODO: change this to use a different 'key' which includes the factory
        // so that we don't need a closure to create the lazy.

        /// <summary>
        /// Explicitly scoped objects can be a mixture of disposable and non-disposable objects
        /// </summary>
        private ConcurrentDictionary<TypeAndTargetId, Lazy<ScopedObject>> _explicitlyScopedObjects
            = new ConcurrentDictionary<TypeAndTargetId, Lazy<ScopedObject>>();

        /// <summary>
        /// implicitly scoped objects will always be IDisposable
        /// </summary>
        private ConcurrentBag<ScopedObject> _implicitlyScopedObjects
            = new ConcurrentBag<ScopedObject>();

        private readonly struct ScopedObject
        {
            private static int _order = 0;

            public int Id { get; }

            public object Object { get; }

            public ScopedObject(object obj)
            {
                Id = _order++;
                Object = obj;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DisposingContainerScope"/> class.
        /// </summary>
        /// <param name="container">The container that this scope will use to resolve instances.</param>
        public DisposingContainerScope(Container container)
            : base(container)
        {
            _canActivate = true;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DisposingContainerScope"/> class that
        /// is optionally treated as a root scope.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isRoot"></param>
        public DisposingContainerScope(ContainerScope parent, bool isRoot)
            : base(parent, isRoot)
        {
            _canActivate = true;
        }

        internal override T ActivateImplicit<T>(T instance)
        {
            if (instance is IDisposable)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(DisposingContainerScope));

                this._implicitlyScopedObjects.Add(new ScopedObject(instance));
            }

            return instance;
        }

        internal override T ActivateExplicit<T>(ResolveContext context, int targetId, Func<ResolveContext, T> instanceFactory)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(DisposingContainerScope));

            var key = new TypeAndTargetId(typeof(T), targetId);
            // TODO: RequestedType is IEnumerable<Blah> when a scoped object is requested as part of an enumerable - hence why these two MSDI Tests fail.
            if (this._explicitlyScopedObjects.TryGetValue(key, out Lazy<ScopedObject> lazy))
            {
                return (T)lazy.Value.Object;
            }

            // use a lazily evaluated object which is bound to this resolve context to ensure only one instance is created
            return (T)this._explicitlyScopedObjects.GetOrAdd(key, CreateLazy).Value.Object;

            Lazy<ScopedObject> CreateLazy(TypeAndTargetId k) => new Lazy<ScopedObject>(CreateScoped);

            ScopedObject CreateScoped() => new ScopedObject(instanceFactory(context));
        }

        private protected override void OnDispose()
        {
            try
            {
                base.OnDispose();
            }
            finally
            {
                // note that explicitly scoped objects might not actually be IDisposable :)
                var allExplicitObjects = this._explicitlyScopedObjects.Skip(0).ToArray()
                    .Where(l => l.Value.Value.Object is IDisposable && !(l.Value.Value.Object is ContainerScope))
                    .Select(l => l.Value.Value);
                var allImplicitObjects = this._implicitlyScopedObjects.Skip(0).ToArray()
                    .Where(l => !(l.Object is ContainerScope));

                // deref all used collections
                this._explicitlyScopedObjects = null;
                this._implicitlyScopedObjects = null;

                foreach (var obj in allExplicitObjects.Concat(allImplicitObjects)
                    .OrderByDescending(so => so.Id))
                {
                    ((IDisposable)obj.Object).Dispose();
                }
            }
        }
    }
}
