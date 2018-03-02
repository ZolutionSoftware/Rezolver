using Rezolver.Tests.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public partial class TargetTypeSelectorTests
    {
        public static TheoryData<Type, Type[], Type[]> ContravariantTypes = new TheoryData<Type, Type[], Type[]>
        {
            {
                typeof(IContravariant<BaseClass>),
                new[] {
                    typeof(IContravariant<BaseClass>),
                    typeof(IContravariant<object>),
                    typeof(IContravariant<>)
                },
                null
            },
            {
                typeof(IContravariant<BaseClassGrandchild>),
                new[]
                {
                    typeof(IContravariant<BaseClassGrandchild>),
                    typeof(IContravariant<BaseClassChild>),
                    typeof(IContravariant<BaseClass>),
                    typeof(IContravariant<object>),
                    typeof(IContravariant<>)
                },
                null
            },
            // Action delegate type
            {
                typeof(Action<BaseClassGrandchild>),
                new[]
                {
                    typeof(Action<BaseClassGrandchild>),
                    typeof(Action<BaseClassChild>),
                    typeof(Action<BaseClass>),
                    typeof(Action<object>),
                    typeof(Action<>)
                },
                null
            },
            // Two-parameter contravariance
            {
                typeof(Action<BaseClassGrandchild, BaseClassGrandchild>),
                new[]
                {
                    typeof(Action<BaseClassGrandchild, BaseClassGrandchild>),
                    typeof(Action<BaseClassGrandchild, BaseClassChild>),
                    typeof(Action<BaseClassGrandchild, BaseClass>),
                    typeof(Action<BaseClassGrandchild, object>),
                    typeof(Action<BaseClassChild, BaseClassGrandchild>),
                    typeof(Action<BaseClassChild, BaseClassChild>),
                    typeof(Action<BaseClassChild, BaseClass>),
                    typeof(Action<BaseClassChild, object>),
                    typeof(Action<BaseClass, BaseClassGrandchild>),
                    typeof(Action<BaseClass, BaseClassChild>),
                    typeof(Action<BaseClass, BaseClass>),
                    typeof(Action<BaseClass, object>),
                    typeof(Action<object, BaseClassGrandchild>),
                    typeof(Action<object, BaseClassChild>),
                    typeof(Action<object, BaseClass>),
                    typeof(Action<object, object>),
                    typeof(Action<,>)
                },
                null
            },
            // Arrays as contravariant types
            {
                typeof(Action<BaseClass[]>),
                new[]
                {
                    typeof(Action<BaseClass[]>),
                    typeof(Action<object[]>),
                    typeof(Action<IList<BaseClass>>),
                    typeof(Action<IList<object>>),
                    typeof(Action<IList>),
                    typeof(Action<Array>),
                    typeof(Action<object>),
                    typeof(Action<>)
                },
                TypeHelpers.GetInterfaces(typeof(BaseClass).MakeArrayType()).SelectMany(
                    t => TypeHelpers.IsGenericType(t) ? new[] { t, t.GetGenericTypeDefinition() } : new[] { t })
                .Concat(
                    TypeHelpers.GetInterfaces(typeof(Object).MakeArrayType()).SelectMany(
                        t => TypeHelpers.IsGenericType(t) ? new[] { t, t.GetGenericTypeDefinition() } : new[] { t })
                ).Select(t => typeof(Action<>).MakeGenericType(t)).ToArray()
            }
        };

        [Theory]
        [MemberData(nameof(ContravariantTypes))]
        public void Contravariant_ShouldReturnCorrectCombination(Type type, Type[] expectedOrder, Type[] expectedInAnyOrder)
        {
            // Arrange and Act
            var result = new TargetTypeSelector(type).ToArray();

            LogExpectedOrder(expectedOrder);
            LogOthers(expectedInAnyOrder);
            LogActual(result);

            // Assert

            // assert that instances of each closed generic search type can be assigned to the search type 
            // - this is double-checking our type compatibility assertions before checking that the results 
            // are the ones we expect.
            Assert.All(result.Where(t => !TypeHelpers.IsGenericTypeDefinition(t) && !TypeHelpers.ContainsGenericParameters(t)),
                t => TypeHelpers.IsAssignableFrom(type, t));

            // check that the the types whose order was specified are actually in the specified order
            Assert.Equal(expectedOrder, result.Where(rt => expectedOrder.Contains(rt)));

            HashSet<Type> expectedSet = new HashSet<Type>(expectedOrder.Concat(expectedInAnyOrder ?? Enumerable.Empty<Type>()));
            HashSet<Type> resultSet = new HashSet<Type>(result);

            HashSet<Type> expectedMissing = new HashSet<Type>(expectedSet);
            HashSet<Type> resultsNotExpected = new HashSet<Type>(resultSet);

            expectedMissing.ExceptWith(resultSet);
            resultsNotExpected.ExceptWith(expectedSet);

            if (expectedMissing.Count != 0)
                LogTypes(expectedMissing.ToArray(), "Missing expected types");
            if (resultsNotExpected.Count != 0)
                LogTypes(resultsNotExpected.ToArray(), "Unexpected result types");

            // if this fails, the previous two logging calls should output the types which are missing/extra
            Assert.True(expectedSet.SetEquals(resultSet));
        }

        public static TheoryData<Type, Type[]> NestedContravariantTypes = new TheoryData<Type, Type[]>
        {
            {
                // from research - contravariance flips every time it nests - with odd numbers of nesting 
                // resulting in the expected base/interface search behaviour, and even numbers of nesting
                // resulting in covariant behaviour.
                typeof(IContravariant<IContravariant<BaseClass>>),
                new[] {
                    // inner most type argument would normally be descended because its contravariant,
                    // but contravariance inverts each time it's nested (effectively becoming covariance)
                    //typeof(IContravariant<IContravariant<BaseClassGrandchild>>),
                    //typeof(IContravariant<IContravariant<BaseClassChild>>),
                    typeof(IContravariant<IContravariant<BaseClass>>),
                    typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>)),
                    typeof(IContravariant<>)
                }
            },
            {
                typeof(IContravariant<IContravariant<IContravariant<BaseClass>>>),
                new[]
                {
                    typeof(IContravariant<IContravariant<IContravariant<BaseClass>>>),
                    typeof(IContravariant<IContravariant<IContravariant<object>>>),
                    typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>))),
                    typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>)),
                    typeof(IContravariant<>),
                }
            },
            {
                typeof(IContravariant<Contravariant<IContravariant<BaseClass>>>),
                new[]
                {
                    // the use of Contravariant<T> as the nested contravariant parameter prevents any further contravariance 
                    // within the any subsequent type arguments because T in that type is not contravariant.
                    // However, because it's passed to a contravariant parameter (of the outermost IContravariant), the search
                    // descends instead into IContravariant - which then does preserve contravariance - hence we see
                    // BaseClass, object variance in the 4th and the entries.
                    typeof(IContravariant<Contravariant<IContravariant<BaseClass>>>),
                    typeof(IContravariant<>).MakeGenericType(typeof(Contravariant<>).MakeGenericType(typeof(IContravariant<>))),
                    typeof(IContravariant<>).MakeGenericType(typeof(Contravariant<>)),
                    typeof(IContravariant<IContravariant<IContravariant<BaseClass>>>),
                    typeof(IContravariant<IContravariant<IContravariant<object>>>),
                    typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>))),
                    typeof(IContravariant<>).MakeGenericType(typeof(IContravariant<>)),
                    typeof(IContravariant<object>),
                    typeof(IContravariant<>),
                }
            }
        };


        [Theory]
        [MemberData(nameof(NestedContravariantTypes))]
        public void Contravariant_ShouldReturnCorrectCombinationForNested(Type type, Type[] expected)
        {
            // Arrange & Act
            var result = new TargetTypeSelector(type).ToArray();
            LogExpectedOrder(expected);
            LogActual(result);

            // Assert

            // verify that the expected types are compatible with the target type
            Assert.All(expected.Where(t => !TypeHelpers.IsGenericTypeDefinition(t) && !TypeHelpers.ContainsGenericParameters(t)),
                t => TypeHelpers.IsAssignableFrom(t, type));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Contravariant_ShouldBeDisabledGlobally()
        {
            // Arrange
            // we need a target container to set options
            var targets = new TargetContainer();
            targets.SetOption<Options.EnableContravariance>(false);

            // Act
            var result = new TargetTypeSelector(typeof(IContravariant<BaseClassGrandchild>), targets).ToArray();

            // Assert

            //should only calculate the exact type and the open generic
            Assert.Equal(
                new[]
                {
                    typeof(IContravariant<BaseClassGrandchild>),
                    typeof(IContravariant<>)
                },
                result);
        }

        [Fact]
        public void Contravariant_ShouldDisableForOpenGeneric()
        {
            // Arrange
            var targets = new TargetContainer();
            // test shows that contravariance still works for Action<> but is disabled for all IContravariant<>
            targets.SetOption<Options.EnableContravariance>(false, typeof(IContravariant<>));

            // Act & Assert
            Assert.False(targets.GetOption(typeof(IContravariant<>), Options.EnableContravariance.Default).Value);
            var result1 = new TargetTypeSelector(typeof(IContravariant<BaseClassGrandchild>), targets).ToArray();
            var result2 = new TargetTypeSelector(typeof(Action<BaseClassGrandchild>), targets).ToArray();

            Assert.Equal(
                new[]
                {
                    typeof(IContravariant<BaseClassGrandchild>),
                    typeof(IContravariant<>)
                },
                result1
                );

            Assert.Equal(
                new[]
                {
                    typeof(Action<BaseClassGrandchild>),
                    typeof(Action<BaseClassChild>),
                    typeof(Action<BaseClass>),
                    typeof(Action<object>),
                    typeof(Action<>)
                },
                result2);
        }

        [Fact]
        public void Contravariant_ShouldDisableForClosedGeneric()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.SetOption<Options.EnableContravariance, IContravariant<BaseClassGrandchild>>(false);

            // Act & Assert
            var result = new TargetTypeSelector(typeof(IContravariant<BaseClassGrandchild>), targets).ToArray();

            Assert.Equal(
                new[]
                {
                    typeof(IContravariant<BaseClassGrandchild>),
                    typeof(IContravariant<>)
                },
                result);
        }
    }
}
