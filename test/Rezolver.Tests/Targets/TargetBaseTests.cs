using Rezolver.Targets;
using Rezolver.Tests.Types;
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
            // Target Type                                      Supports Type
            { typeof(string),                                   typeof(string) },
            { typeof(string),                                   typeof(object) },
            { typeof(string),                                   typeof(IEnumerable<char>) },
            { typeof(BaseClassChild),                           typeof(BaseClass) },
            { typeof(BaseClassGrandchild),                      typeof(BaseClass) },
            { typeof(BaseClassGrandchild[]),                    typeof(BaseClassChild[]) },
        };

        [Theory]
        [MemberData(nameof(SupportsTypeData))]
        public void ShouldSupportType(Type targetType, Type shouldBeSupported)
        {
            var target = new TestTarget(targetType);
            Assert.True(target.SupportsType(shouldBeSupported));
        }

        public static TheoryData<Type, Type> SupportsVariantTypeData = new TheoryData<Type, Type>()
        {
            // Covariance
            { typeof(Func<BaseClassChild>),                     typeof(Func<BaseClass>) },
            { typeof(Covariant<string>),                        typeof(ICovariant<IEnumerable<char>>) },
            { typeof(Covariant<string>),                        typeof(ICovariant<object>) },
            // Contravariance
            { typeof(Action<BaseClass>),                        typeof(Action<BaseClassChild>) },
            { typeof(Contravariant<BaseClass>),                 typeof(IContravariant<BaseClassChild>) },
            // Variance combinations
            // ---------------------
            // When combining a contravariant type param as an argument to a covariant
            // type param - the normal contravariance rules apply
            { typeof(Action<Func<BaseClass>>),                  typeof(Action<Func<BaseClassChild>>) },
            { typeof(Func<Action<BaseClass>>),                  typeof(Func<Action<BaseClassChild>>) },
            // class<class<type>> -> iface<iface<type>> works because class->interface is 'smaller' assignment
            { typeof(Covariant<Covariant<string>>),             typeof(ICovariant<ICovariant<object>>) },
            { typeof(Covariant<Covariant<string>>),             typeof(ICovariant<object>) },
            // outer type -> interface works purely because of standard assignment equality
            // first generic arguments must be of the same generic type (i.e. IContravariant<> because
            // that type parameter is itself contravariant, which would only allow bases or interfaces
            { typeof(Contravariant<IContravariant<string>>),    typeof(IContravariant<IContravariant<object>>) },
            { typeof(Contravariant<object>),                    typeof(IContravariant<IContravariant<object>>) }
        };

        [Theory]
        [MemberData(nameof(SupportsVariantTypeData))]
        public void ShouldSupportVariantType(Type tTarget, Type tSupports)
        {
            var target = new TestTarget(tTarget);
            Assert.True(target.SupportsType(tSupports));
        }
    }
}
