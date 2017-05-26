using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Targets
{
    /// <summary>
    /// testing elements of the TargetBase class as it's used by so many of the standard targets
    /// </summary>
    public class TargetBaseTests
    {
        private class TestTarget : TargetBase
        {
            public override Type DeclaredType { get; }

            public TestTarget(Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (type.IsGenericTypeDefinition || type.ContainsGenericParameters)
                    throw new ArgumentException("TargetBase is not expected to be able to handle generic type definitions or open generic types", nameof(type));
                DeclaredType = type;
            }
        }

        public static TheoryData<Type, Type> SupportsTypeData = new TheoryData<Type, Type>()
        {
            { typeof(string), typeof(string) },
            { typeof(string), typeof(object) },
            { typeof(string), typeof(IEnumerable<char>) },
            { typeof(Types.BaseClassChild), typeof(Types.BaseClass) },
            { typeof(Types.BaseClassGrandchild), typeof(Types.BaseClass) },
            { typeof(Types.BaseClassGrandchild[]), typeof(Types.BaseClassChild[]) },
            //Covariance
            { typeof(List<Types.BaseClassChild>), typeof(IEnumerable<Types.BaseClass>) },
            //Contravariance
            { typeof(Action<Types.BaseClass>), typeof(Action<Types.BaseClassChild>) }
        };

        [Theory]
        [MemberData("SupportsTypeData")]
        public void ShouldSupportType(Type targetType, Type shouldBeSupported)
        {
            var target = new TestTarget(targetType);
            Assert.True(target.SupportsType(shouldBeSupported));
        }
    }
}
