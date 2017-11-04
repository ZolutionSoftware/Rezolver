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

        public ProjectionTarget(ITarget inputTarget, ITarget outputTarget, Type projectedType = null, Type declaredType = null)
        {
            InputTarget = inputTarget;
            ProjectedType = projectedType;
            OutputTarget = outputTarget;
            DeclaredType = declaredType ?? outputTarget.DeclaredType;
        }
    }
}
