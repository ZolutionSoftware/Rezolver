using Rezolver.Tests.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests
{
    public class TypeIndexTests
    {
        private void LogTypes(Type[] types, string header)
        {
            Output.WriteLine(header);
            Output.WriteLine("================");

            int counter = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var type in types)
            {
                type.CSharpLikeTypeName(sb);
                Output.WriteLine("{0} => {1}", counter++, sb.ToString());
                sb.Clear();
            }
            Output.WriteLine("");
        }

        private void LogActual(Type[] result)
        {
            LogTypes(result, "Actual");
        }

        private void LogExpectedOrder(Type[] expected)
        {
            LogTypes(expected, "Expected (Specified Relative Order)");
        }

        private void LogOthers(Type[] anyExpected)
        {
            if (anyExpected != null)
                LogTypes(anyExpected, "Expected (Order Not Specified)");
        }

        public ITestOutputHelper Output { get; private set; }
        public TypeIndexTests(ITestOutputHelper output)
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
            // Arrange
            var typeIndex = new TypeIndex();

            // Act & Assert
            Assert.Equal(new[] { type }, typeIndex.For(type).GetTargetSearchTypes());
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
            // Arrange
            var typeIndex = new TypeIndex();

            // Act & Assert
            Assert.Equal(expected, typeIndex.For(type).GetTargetSearchTypes());
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
            // Arrange 
            var typeIndex = new TypeIndex();

            // Act
            var result = typeIndex.For(type).GetTargetSearchTypes().ToArray();
            LogExpectedOrder(expected);
            LogActual(result);

            // Assert
            Assert.Equal(expected, result);

            //Func<Action<object>> fao = null;
            //Func<Action<string>> fas = null;

            //fas = fao;
            //Action<string> @as = fao();

            //Action<Func<object>> afo = null;
            //Action<Func<string>> afs = null;

            //afs = afo;
            //Func<string> fs = () => "hello";
            //afo(fs);

            //Action<Func<Action<object>>> afao = null;
            //Action<Func<Action<string>>> afas = null;

            //afao = afas;
        }
    }
}
