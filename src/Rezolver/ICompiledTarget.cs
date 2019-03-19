// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Interface for an object (usually an <see cref="ITarget"/> which can provide its own factory. 
    /// </summary>
    public interface IResolvable
    {
        /// <summary>
        /// Gets a factory delegate for the object to be used by a <see cref="Container"/>
        /// </summary>
        /// <returns>A delegate which, when invoked with a <see cref="ResolveContext"/>, produces the object</returns>
        Func<ResolveContext, object> Factor { get; }
    }

    /// <summary>
    /// Interface for an object (usually an <see cref="ITarget"/> which can provide its own strongly-typed
    /// factory
    /// </summary>
    /// <typeparam name="T">The type of instance that will be produced by the <see cref=""/></typeparam>
    public interface IResolvable<T>
    {
        /// <summary>
        /// Gets a strongly-typed delegate for the object to be used by a <see cref="Container"/>
        /// </summary>
        /// <returns>A delegate which, when invoked with a <see cref="ResolveContext"/>, produces the object</returns>
        Func<ResolveContext, T> Factory { get; }
    }
}
