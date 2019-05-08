// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Interface for an object (usually an <see cref="ITarget"/> which can provide a factory which a <see cref="Container"/>
    /// can use to get an instance. 
    /// </summary>
    public interface IFactoryProvider
    {
        /// <summary>
        /// Gets a factory delegate for the object to be used by a <see cref="Container"/>
        /// </summary>
        /// <returns>A delegate which, when invoked with a <see cref="ResolveContext"/>, produces the object</returns>
        Func<ResolveContext, object> Factory { get; }
    }

    /// <summary>
    /// Interface for an object (usually an <see cref="ITarget"/> which can provide its own strongly-typed
    /// factory which a <see cref="Container"/> can use to get an instance.
    /// </summary>
    /// <typeparam name="TService">The type of instance that will be produced by the <see cref="Factory"/></typeparam>
    public interface IFactoryProvider<out TService>
    {
        /// <summary>
        /// Gets a strongly-typed delegate for the object to be used by a <see cref="Container"/>
        /// </summary>
        /// <returns>A delegate which, when invoked with a <see cref="ResolveContext"/>, produces the object</returns>
        Func<ResolveContext, TService> Factory { get; }
    }
}
