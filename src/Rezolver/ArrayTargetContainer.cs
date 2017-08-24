using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Inheriting from TargetDictionary allows us to support specific 
    /// </summary>
    public class ArrayTargetContainer : TargetDictionaryContainer
    {
        public ArrayTargetContainer(ITargetContainer root)
            : base(root)
        {

        }
        
        internal static readonly MethodInfo LinqToArrayMethod 
            = MethodCallExtractor.ExtractCalledMethod(() => Enumerable.ToArray<int>(null)).GetGenericMethodDefinition();

        public override ITarget Fetch(Type type)
        {
            if (!type.IsArray)
                throw new ArgumentException($"This target container only supports array types - { type } is not an array type");

            var baseResult = base.Fetch(type);
            if (baseResult != null && !baseResult.UseFallback)
                return baseResult;

            // check the array rank - if it's greater than 1, then we can't do anything
            if (type.GetArrayRank() != 1)
                throw new ArgumentException($"Arrays of rank 2 cannot be injected automatically - { type } has a rank of { type.GetArrayRank() }");

            // cheating: wrapping IEnumerable injection with a delegate bound directly to the Linq ToArray method.

            Type elementType = TypeHelpers.GetElementType(type);
            // Now, technically arrays are covariant and therefore
            // should contain any object whose type is equal to or derived from
            // the desired element type.  Need to figure that one out.

            var delegateType = typeof(Func<,>).MakeGenericType(
                typeof(IEnumerable<>).MakeGenericType(elementType),
                type
                );

            return new DelegateTarget(LinqToArrayMethod.MakeGenericMethod(elementType).CreateDelegate(delegateType), type);
        }
    }
}
