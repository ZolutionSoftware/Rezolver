using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Targets
{
    public class ProjectionTarget : TargetBase
    {
        public override Type DeclaredType { get; }

        public ITarget InputTarget { get; }
        public Type ProjectedType { get; }
        public ITarget OutputTarget { get; }

        public ProjectionTarget(ITarget inputTarget, ITarget outputTarget, Type projectedType, Type declaredType)
        {
            InputTarget = inputTarget ?? throw new ArgumentNullException(nameof(inputTarget));
            OutputTarget = outputTarget ?? throw new ArgumentNullException(nameof(outputTarget));
            ProjectedType = projectedType ?? throw new ArgumentNullException(nameof(projectedType));
            DeclaredType = declaredType ?? throw new ArgumentNullException(nameof(declaredType));
        }
    }
}
