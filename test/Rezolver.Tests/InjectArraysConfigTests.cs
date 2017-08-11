using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class InjectArraysConfigTests
    {
        /// <summary>
        /// Inheriting from TargetDictionary allows us to support specific 
        /// </summary>
        private class ArrayTargetContainer : TargetDictionaryContainer
        {
            public ArrayTargetContainer(ITargetContainer root)
                : base(root)
            {

            }

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

                // Now, technically arrays are covariant and therefore
                // should contain any object whose type is equal to or derived from
                // the desired element type.  Need to figure that one out.
                Type elementType = TypeHelpers.GetElementType(type);
                return new ArrayTarget(Root.FetchAll(elementType), elementType);
            }
        }

        /// <summary>
        /// Very similar to the enumerable target
        /// </summary>
        public class ArrayTarget : TargetBase
        {
            public override Type DeclaredType { get; }

            public IEnumerable<ITarget> Targets { get; }

            public override bool UseFallback => !Targets.Any();

            public ArrayTarget(IEnumerable<ITarget> targets, Type elementType)
            {
                Targets = targets ?? throw new ArgumentNullException(nameof(targets));
                DeclaredType = TypeHelpers.MakeArrayType(elementType ?? throw new ArgumentNullException(nameof(elementType)));
            }
        }

        [Fact]
        public void ShouldFetchArrayTarget()
        {
            var targets = new TargetContainer();
            var arrayContainer = new ArrayTargetContainer(targets);
            targets.RegisterContainer(typeof(Array), arrayContainer);
            var a = new int[0];
            var result = targets.Fetch(typeof(int[]));
            Assert.NotNull(result);
        }
    }
}
