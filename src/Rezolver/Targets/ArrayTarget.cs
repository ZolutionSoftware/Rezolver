using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Targets
{
    /// <summary>
    /// Very similar to the enumerable target
    /// </summary>
    public class ArrayTarget : TargetBase
    {
        /// <summary>
        /// Always returns the array type built from <see cref="ElementType"/>
        /// </summary>
        public override Type DeclaredType => TypeHelpers.MakeArrayType(ElementType);

        /// <summary>
        /// The element type of the array that should be built from this target
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// The targets whose results will be included in the array
        /// </summary>
        public IEnumerable<ITarget> Targets { get; }

        /// <summary>
        /// Returns <c>true</c> is the <see cref="Targets"/> are empty (indicating that the
        /// caller can safely use a 'better match' target from another source (e.g. target container)
        /// 
        /// </summary>
        public override bool UseFallback => !Targets.Any();

        public ArrayTarget(IEnumerable<ITarget> targets, Type elementType)
        {
            Targets = targets ?? throw new ArgumentNullException(nameof(targets));
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }
    }
}
