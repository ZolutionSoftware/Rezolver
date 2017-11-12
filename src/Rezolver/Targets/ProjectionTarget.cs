using Rezolver.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Targets
{
    public class ProjectionTarget : TargetBase
    {
        public override Type DeclaredType { get => OutputType; }
        public Type ImplementationType { get; }
        public ITarget InputTarget { get; }
        public Type InputType { get; }
        public ITarget OutputTarget { get; }
        public Type OutputType { get; }

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
