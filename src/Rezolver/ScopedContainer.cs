// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Extends the <see cref="Container"/> to implement lifetime implicit scoping through the
    /// <see cref="Scope"/> that's created along with it.
    ///
    /// Implementation of the <see cref="IScopedContainer"/> interface.
    /// </summary>
    /// <remarks>
    /// If you want your root container to act as a lifetime scope, then you should use this
    /// class instead of using <see cref="Container"/>.
    ///
    /// Note that this class does NOT implement the <see cref="IContainerScope"/> interface because
    /// the two interfaces are not actually compatible with each other, thanks to identical sets of extension methods.
    /// </remarks>
    public sealed class ScopedContainer : Container, IDisposable
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="ScopedContainer"/> class.
        /// </summary>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the
        /// container, ultimately being passed to the <see cref="Targets"/> property.
        ///
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <param name="config">Can be null.  A configuration to apply to this container (and, potentially its
        /// <see cref="Targets"/>).  If not provided, then the <see cref="Container.DefaultConfig"/> will be used</param>
        public ScopedContainer(IRootTargetContainer targets = null, IContainerConfig config = null)
            : base(targets)
        {
            // annoying: double assignment here (base already initialises it...)
            _scope = new ConcurrentContainerScope(this);
            (config ?? DefaultConfig).Configure(this, Targets);
        }

        #region IDisposable Support
        private bool _isDisposed = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _scope.Dispose();
                }

                this._isDisposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
