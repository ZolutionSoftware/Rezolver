// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Runtime;

namespace Rezolver.Targets
{
    /// <summary>
    /// Target that represents the projection of an individual object of type <see cref="InputType"/>
    /// into an object of type <see cref="OutputType"/>.
    ///
    /// Created automatically by the <see cref="ProjectionTargetContainer"/>.
    /// </summary>
    public class ProjectionTarget : TargetBase
    {
        public override ScopeBehaviour ScopeBehaviour => ScopeBehaviour.None;
        /// <summary>
        /// Always returns <see cref="OutputType"/>
        /// </summary>
        public override Type DeclaredType => OutputType;
        /// <summary>
        /// The type of object that is to be created by the <see cref="OutputTarget"/>
        /// </summary>
        public Type ImplementationType { get; }
        /// <summary>
        /// The target whose result is being projected by the <see cref="OutputTarget"/>
        /// </summary>
        public ITarget InputTarget { get; }
        /// <summary>
        /// The element type of the enumerable from which the <see cref="InputTarget"/> was obtained.
        /// </summary>
        public Type InputType { get; }
        /// <summary>
        /// The target which produces the projected object from the result of the <see cref="InputTarget"/>.
        ///
        /// The expectation is that it has a dependency on an instance of <see cref="InputType"/> (e.g. via
        /// a parameter on a constructor or delegate etc).
        /// </summary>
        public ITarget OutputTarget { get; }
        /// <summary>
        /// The element type of the enumerable into which the result of the <see cref="OutputTarget"/>
        /// will go.
        /// </summary>
        public Type OutputType { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="ProjectionTarget"/> class.
        /// </summary>
        /// <param name="inputTarget">The target whose result is being projected by the <paramref name="outputTarget"/></param>
        /// <param name="outputTarget">The target which produces the projected object from the result of the <paramref name="inputTarget"/></param>
        /// <param name="inputType">The element type of the enumerable from which the <paramref name="inputTarget"/> was obtained</param>
        /// <param name="outputType">The element type of the enumerable into which the result of the <paramref name="outputTarget"/> will go.</param>
        /// <param name="implementationType">The type of object that is to be created by the <paramref name="outputTarget"/></param>
        public ProjectionTarget(ITarget inputTarget, ITarget outputTarget, Type inputType, Type outputType, Type implementationType)
        {
            InputTarget = inputTarget ?? throw new ArgumentNullException(nameof(inputTarget));
            OutputTarget = outputTarget ?? throw new ArgumentNullException(nameof(outputTarget));
            InputType = inputType ?? throw new ArgumentNullException(nameof(inputType));
            OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        internal ProjectionTarget(ITarget inputTarget, Type inputType, Type outputType, TargetProjection projection)
        {
            InputTarget = inputTarget;
            InputType = inputType;
            OutputType = outputType;
            OutputTarget = projection.Target;
            ImplementationType = projection.ImplementationType;
        }
    }
}
