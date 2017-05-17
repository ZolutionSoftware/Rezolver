using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Targets
{
    /// <summary>
    /// A special delegate target which explicitly allows direct resolving (via <see cref="IDirectTarget"/>) without compilation.
    /// 
    /// Created by the factory and registration functions e.g. <see cref="Target.ForDelegate{TResult}(Func{IResolveContext, TResult}, Type)"/>
    /// or <see cref="DelegateTargetContainerExtensions.RegisterDelegate{TResult}(ITargetContainer, Func{IResolveContext, TResult}, Type, ScopeBehaviour)"/>.
    /// </summary>
    internal class NullaryDelegateTarget : DelegateTarget, IDirectTarget
    {
        public NullaryDelegateTarget(Delegate factory, Type declaredType = null) : base(factory, declaredType)
        {
            if (FactoryMethod.GetParameters()?.Length > 0) throw new ArgumentException("Only nullary delegates (i.e. which have no parameters) can be used for this target");
        }

        object IDirectTarget.GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
