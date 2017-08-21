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

        public class ArrayTypeResolver : ITargetContainerTypeResolver
        {
            public Type GetContainerType(Type serviceType)
            {
                if (TypeHelpers.IsArray(serviceType))
                    return typeof(Array);
                return null;
            }
        }

        protected TargetContainer GetArrayEnabledContainer()
        {
            var targets = new TargetContainer();
            targets.SetOption<ITargetContainerTypeResolver, Array>(new ArrayTypeResolver());
            var arrayContainer = new ArrayTargetContainer(targets);
            targets.RegisterContainer(typeof(Array), arrayContainer);
            return targets;
        }

        [Fact]
        public void ShouldFetchArrayTarget()
        {
            var targets = GetArrayEnabledContainer();
            var result = targets.Fetch(typeof(int[]));
            Assert.NotNull(result);
        }

        [Fact]
        public void ShouldFetchExplicitlyRegisteredArray()
        {
            var targets = GetArrayEnabledContainer();
            var myArray = Target.ForObject(new[] { "hello world" });
            targets.Register(myArray);
            var result = targets.Fetch(typeof(string[]));
            Assert.Same(myArray, result);
        }
    }
}
