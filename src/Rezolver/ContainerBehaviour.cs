using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    public static class ContainerBehaviour
    {
        /// <summary>
        /// This is the collection of behaviours which are used by most <see cref="ContainerBase"/> classes.
        /// 
        /// Does not, however, apply to <see cref="OverridingContainer"/>
        /// </summary>
        public static ContainerBehaviours Default { get; }

        /// <summary>
        /// This is the collection of behaviours used by the <see cref="OverridingContainer"/> container by default.
        /// 
        /// Note - this collection is empty by default as all behaviours are inherited from the container's root container.
        /// </summary>
        public static ContainerBehaviours DefaultOverridingBehaviour { get; }

        static ContainerBehaviour()
        {
            Default = new ContainerBehaviours(
                Compilation.Expressions.ExpressionCompilingBehaviour.Instance
            );

            DefaultOverridingBehaviour = new ContainerBehaviours();
        }
    }
}
