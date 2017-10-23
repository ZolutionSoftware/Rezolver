using Rezolver.Tests.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests
{
    public partial class TargetTypeSelectorTests
    {
        public ITestOutputHelper Output { get; private set; }
        public TargetTypeSelectorTests(ITestOutputHelper output)
        {
            Output = output;
        }
        // non-generic types should just be returned as-is
        public static TheoryData<Type> NonGenericTypes = new TheoryData<Type>
        {
            { typeof(object) },
            { typeof(int) },
            { typeof(string) },
            { typeof(IEnumerable) },
            { typeof(BaseClass) },
            { typeof(BaseClassChild) },
            { typeof(BaseClassGrandchild) },
            { typeof(Action) }
        };

        [Theory]
        [MemberData(nameof(NonGenericTypes))]
        public void Basic_ShouldReturnOneResult(Type type)
        {
            // Act & Assert
            Assert.Equal(new[] { type }, new TargetTypeSelector(type));
        }

        // these are 'simple' because they don't have bases and are neither contravariant nor covariant
        public static TheoryData<Type, Type[]> SimpleGenericTypes = new TheoryData<Type, Type[]>
        {
            { typeof(SimpleGenericInterface<TypeArgs.T1>),
                new[] {typeof(SimpleGenericInterface<TypeArgs.T1>), typeof(SimpleGenericInterface<>) } },
            { typeof(SimpleGenericInterface<TypeArgs.T1, TypeArgs.T2>),
                new[] {typeof(SimpleGenericInterface<TypeArgs.T1, TypeArgs.T2>), typeof(SimpleGenericInterface<,>) } },
            { typeof(SimpleGenericInterface<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>),
                new[] {typeof(SimpleGenericInterface<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>), typeof(SimpleGenericInterface<,,>) } },
            { typeof(SimpleGenericClass<TypeArgs.T1>),
                new[] {typeof(SimpleGenericClass<TypeArgs.T1>), typeof(SimpleGenericClass<>) } },
            { typeof(SimpleGenericClass<TypeArgs.T1, TypeArgs.T2>),
                new[] {typeof(SimpleGenericClass<TypeArgs.T1, TypeArgs.T2>), typeof(SimpleGenericClass<,>) } },
            { typeof(SimpleGenericClass<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>),
                new[] {typeof(SimpleGenericClass<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>), typeof(SimpleGenericClass<,,>) } },
            { typeof(SimpleGenericValueType<TypeArgs.T1>),
                new[] {typeof(SimpleGenericValueType<TypeArgs.T1>), typeof(SimpleGenericValueType<>) } },
            { typeof(SimpleGenericValueType<TypeArgs.T1, TypeArgs.T2>),
                new[] {typeof(SimpleGenericValueType<TypeArgs.T1, TypeArgs.T2>), typeof(SimpleGenericValueType<,>) } },
            { typeof(SimpleGenericValueType<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>),
                new[] {typeof(SimpleGenericValueType<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>), typeof(SimpleGenericValueType<,,>) } }
        };

        [Theory]
        [MemberData(nameof(SimpleGenericTypes))]
        public void Basic_ShouldReturnClosedAndOpenGeneric(Type type, Type[] expected)
        {
            // Act & Assert
            Assert.Equal(expected, new TargetTypeSelector(type));
        }

        public static TheoryData<Type, Type[]> NestedGenericTypes = new TheoryData<Type, Type[]>
        {
            {
                typeof(SimpleGenericInterface<SimpleGenericClass<TypeArgs.T1>>),
                new[] {
                    typeof(SimpleGenericInterface<SimpleGenericClass<TypeArgs.T1>>),
                    typeof(SimpleGenericInterface<>).MakeGenericType(typeof(SimpleGenericClass<>)),
                    typeof(SimpleGenericInterface<>)
                }
            },
            {
                typeof(SimpleGenericInterface<SimpleGenericClass<TypeArgs.T1>, SimpleGenericValueType<TypeArgs.T2>>),
                new[] {
                    typeof(SimpleGenericInterface<SimpleGenericClass<TypeArgs.T1>, SimpleGenericValueType<TypeArgs.T2>>),
                    typeof(SimpleGenericInterface<,>).MakeGenericType(typeof(SimpleGenericClass<TypeArgs.T1>), typeof(SimpleGenericValueType<>)),
                    typeof(SimpleGenericInterface<,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<TypeArgs.T2>)),
                    typeof(SimpleGenericInterface<,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>)),
                    typeof(SimpleGenericInterface<,>)
                }
            },
            {
                typeof(SimpleGenericInterface<SimpleGenericClass<TypeArgs.T1>, SimpleGenericValueType<TypeArgs.T2>, SimpleGenericInterface<TypeArgs.T3>>),
                new[] {
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<TypeArgs.T1>), typeof(SimpleGenericValueType<TypeArgs.T2>), typeof(SimpleGenericInterface<TypeArgs.T3>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<TypeArgs.T1>), typeof(SimpleGenericValueType<TypeArgs.T2>), typeof(SimpleGenericInterface<>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<TypeArgs.T1>), typeof(SimpleGenericValueType<>), typeof(SimpleGenericInterface<TypeArgs.T3>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<TypeArgs.T1>), typeof(SimpleGenericValueType<>), typeof(SimpleGenericInterface<>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<TypeArgs.T2>), typeof(SimpleGenericInterface<TypeArgs.T3>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<TypeArgs.T2>), typeof(SimpleGenericInterface<>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>), typeof(SimpleGenericInterface<TypeArgs.T3>)),
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>), typeof(SimpleGenericInterface<>)),
                    typeof(SimpleGenericInterface<,,>)
                }
            },
            // this time, starting with an open generic as a type argument to the outer generic - which drastically shortens the potential list
            // of types
            {
                typeof(SimpleGenericInterface<>).MakeGenericType(typeof(SimpleGenericClass<>)),
                new[] {
                    typeof(SimpleGenericInterface<>).MakeGenericType(typeof(SimpleGenericClass<>)),
                    typeof(SimpleGenericInterface<>)
                }
            },
            {
                typeof(SimpleGenericInterface<,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>)),
                new[] {
                    typeof(SimpleGenericInterface<,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>)),
                    typeof(SimpleGenericInterface<,>)
                }
            },
            {
                typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>), typeof(SimpleGenericInterface<>)),
                new[] {
                    typeof(SimpleGenericInterface<,,>).MakeGenericType(typeof(SimpleGenericClass<>), typeof(SimpleGenericValueType<>), typeof(SimpleGenericInterface<>)),
                    typeof(SimpleGenericInterface<,,>)
                }
            },
        };

        [Theory]
        [MemberData(nameof(NestedGenericTypes))]
        public void Basic_ShouldHandleNestedGenerics(Type type, Type[] expected)
        {
            // Arrange & Act
            var result = new TargetTypeSelector(type).ToArray();
            LogExpectedOrder(expected);
            LogActual(result);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
