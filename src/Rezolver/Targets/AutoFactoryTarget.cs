using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
    public class AutoFactoryTarget : TargetBase
    {
        public override Type DeclaredType => DelegateType;

        public ITarget InnerTarget { get; }
        public Type DelegateType { get; }
        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        public AutoFactoryTarget(ITarget innerTarget, Type delegateType, Type returnType, Type[] parameterTypes)
            : base(innerTarget.Id) // note here - passing the ID in from the inner target to preserve the order.
        {
            InnerTarget = innerTarget ?? throw new ArgumentNullException(nameof(innerTarget));
            DelegateType = delegateType ?? throw new ArgumentNullException(nameof(delegateType));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ParameterTypes = parameterTypes ?? TypeHelpers.EmptyTypes;
            if (ParameterTypes.Distinct().Count() != ParameterTypes.Length)
                throw new ArgumentException($"Invalid auto factory delegate type: {delegateType} - all parameter types must be unique", nameof(parameterTypes));
        }
    }
}
