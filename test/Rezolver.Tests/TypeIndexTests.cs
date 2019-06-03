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

            var counter = 0;
            var sb = new StringBuilder();
            foreach (var type in types)
            {
                type.CSharpLikeTypeName(sb);
                Output.WriteLine("{0} => {1}", counter++, sb.ToString());
                sb.Clear();
            }
            Output.WriteLine("");
        }

        private void LogTypes((Type type, bool isVariant)[] tvs, string header)
        {
            Output.WriteLine(header);
            Output.WriteLine("================");

            var counter = 0;
            var sb = new StringBuilder();
            foreach (var (type, isVariant) in tvs)
            {
                type.CSharpLikeTypeName(sb);
                Output.WriteLine("{0} => {1}{2}", counter++, sb.ToString(), isVariant ? " (Variant)" : "");
                sb.Clear();
            }
            Output.WriteLine("");
        }

        private void LogActual(Type[] result)
        {
            LogTypes(result, "Actual");
        }

        private void LogActual((Type type, bool isVariant)[] result)
        {
            LogTypes(result, "Actual");
        }

        private void LogExpectedOrder(Type[] expected)
        {
            LogTypes(expected, "Expected (Specified Relative Order)");
        }

        private void LogExpectedOrder((Type type, bool isVariant)[] expected)
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


        [Fact]
        public void Basic_ShouldGetNonGenericRefAssignableTo()
        {
            // Arrange


            // Act

            // Assert
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
            Assert.Equal(new[] { type }, typeIndex.For(type).GetCompatibleTypes().Select(tv => tv.type));
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
            Assert.Equal(expected, typeIndex.For(type).GetCompatibleTypes().Select(tv => tv.type));
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
            var result = typeIndex.For(type).GetCompatibleTypes().Select(tv => tv.type).ToArray();
            LogExpectedOrder(expected);
            LogActual(result);

            // Assert
            Assert.Equal(expected, result);

            //Func<string> fs = null;
            //Func<object> fo = null;
            //fo = fs;

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

            //Func<Action<Func<object>>> fafo = null;
            //Func<Action<Func<string>>> fafs = null;
            //fafs = fafo;
        }

        [Fact]
        public void Covariant_ShouldBeCorrectForFuncObjectDelegate()
        {
            // Arrange
            var typeIndex = new TypeIndex();
            typeIndex.Prepare<Func<string>>();
            var entry = typeIndex.For<Func<object>>();

            // Act
            var result = entry.GetCompatibleTypes().ToArray();

            LogActual(result);

            // Assert
            Assert.Equal(new[]
            {
                (typeof(Func<object>), false),
                (typeof(Func<string>), true),
                (typeof(Func<>), false)
            }, result);
        }

        [Fact]
        public void Covariant_ShouldBeCorrectForFuncIEnumerableCharDelegate()
        {
            // Arrange
            var typeIndex = new TypeIndex();
            typeIndex.Prepare<Func<string>>();
            var entry = typeIndex.For<Func<IEnumerable<char>>>();

            // Act
            //var result = entry.GetTargetSearchTypes().ToArray();
            var result = entry.GetCompatibleTypes().ToArray();

            LogActual(result);

            // Assert
            Assert.Equal(new[] {
                (typeof(Func<IEnumerable<char>>), false),
                (typeof(Func<>).MakeGenericType(typeof(IEnumerable<>)), false),
                (typeof(Func<string>), true),
                (typeof(Func<>), false)
            }, result);
        }

        [Fact]
        public void Covariant_ShouldIncludeAllDerivedRegistrations_MostRecentToLeast()
        {
            // Arrange
            var typeIndex = new TypeIndex();
            typeIndex.Prepare(typeof(ICovariant<BaseClass>),
                typeof(ICovariant<BaseClassChild>),
                typeof(ICovariant<BaseClassGrandchild>));

            var entry = typeIndex.For<ICovariant<BaseClass>>();

            // Act
            var result = entry.GetCompatibleTypes().ToArray();

            LogActual(result);

            // Assert
            // here, the order of the covariant types should be determined by the order they're added.
            // Most recent should be last.
            Assert.Equal(new[]
            {
                (typeof(ICovariant<BaseClass>), false),
                (typeof(ICovariant<BaseClassGrandchild>), true),
                (typeof(ICovariant<BaseClassChild>), true),
                (typeof(ICovariant<>), false)
            }, result);
        }
    }
}
