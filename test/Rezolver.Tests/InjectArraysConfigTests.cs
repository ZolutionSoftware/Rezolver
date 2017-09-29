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


        protected TargetContainer GetArrayEnabledContainer()
        {
            return new TargetContainer(new CombinedTargetContainerConfig()
            {
                Configuration.InjectEnumerables.Instance,
                Configuration.InjectArrays.Instance
            });
        }

        public static TheoryData<Type> FetchArrayTargetTypes => new TheoryData<Type>{
            {
                typeof(int)
            },
            {
                typeof(string)
            },
            {
                typeof(Types.BaseClass)
            },
            {
                typeof(Types.Generic<int>)
            }
        };

        [Theory]
        [MemberData(nameof(FetchArrayTargetTypes))]
        public void ShouldFetchArrayTarget(Type fetchType)
        {
            // targets should always be returned for array types even when no
            // targets are registered either for the array type, or for the element type
            var targets = GetArrayEnabledContainer();
            var result = targets.Fetch(fetchType.MakeArrayType());
            Assert.NotNull(result);
        }

        [Theory]
        [MemberData(nameof(FetchArrayTargetTypes))]
        public void ShouldFetchExplicitlyRegisteredArrayTarget(Type fetchType)
        {
            var targets = GetArrayEnabledContainer();
            var arrayType = fetchType.MakeArrayType();
            var myArray = Target.ForObject(Array.CreateInstance(fetchType, 1), arrayType);
            targets.Register(myArray);
            var result = targets.Fetch(arrayType);
            Assert.Same(myArray, result);
        }

        
    }
}
