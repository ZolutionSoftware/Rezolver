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
    public class TargetTypeSelectorTests
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
        public void ShouldReturnOneResult(Type type)
        {
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
        public void ShouldReturnClosedAndOpenGeneric(Type type, Type[] expected)
        {
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
        public void ShouldHandleNestedGenerics(Type type, Type[] expected)
        {
            var result = new TargetTypeSelector(type).ToArray();
            LogExpected(expected);
            LogActual(result);
            Assert.Equal(expected, result);
        }

        public static TheoryData<Type, Type[]> ContravariantTypes = new TheoryData<Type, Type[]>
        {
            {
                typeof(IContravariant<BaseClass>),
                new[] {
                    typeof(IContravariant<BaseClass>),
                    typeof(IContravariant<object>),
                    typeof(IContravariant<>)
                }
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
                }
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
                }
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
                }
            }
        };

        [Theory]
        [MemberData(nameof(ContravariantTypes))]
        public void ShouldReturnCorrectCombinationForContravariant(Type type, Type[] expected)
        {
            var result = new TargetTypeSelector(type).ToArray();
            LogExpected(expected);
            LogActual(result);
            // assert that instances of each closed generic search type can be assigned to the search type 
            // - this is double-checking our type compatibility assertions before checking that the results 
            // are the ones we expect.
            Assert.All(expected.Where(t => !t.IsGenericTypeDefinition && !t.ContainsGenericParameters),
                t => type.IsAssignableFrom(t));
            Assert.Equal(expected, result);

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
        public void ShouldReturnCorrectCombinationForNestedContravariant(Type type, Type[] expected)
        {
            var result = new TargetTypeSelector(type).ToArray();
            LogExpected(expected);
            LogActual(result);
            Assert.All(expected.Where(t => !t.IsGenericTypeDefinition && !t.ContainsGenericParameters),
                t => t.IsAssignableFrom(type));
            Assert.Equal(expected, result);

            //// standard contra
            //Action<BaseClassGrandchild> m = (Action<BaseClass>)null;
            //// double contra (effectively becomes co)
            //Action<Action<BaseClass>> m2 = (Action<Action<BaseClassGrandchild>>)null;
            //// covariance mixed with contra.  Note: contra search direction wins.
            //Action<Func<BaseClassGrandchild>> m3 = (Action<Func<BaseClass>>)null;
            //// double contra with co mixed in different positions - again, same as straight double contra.
            //Action<Action<Func<BaseClass>>> m4a = (Action<Action<Func<BaseClassGrandchild>>>)null;
            //Action<Func<Action<BaseClass>>> m4b = (Action<Func<Action<BaseClassGrandchild>>>)null;
            //Func<Action<Action<BaseClass>>> m4c = (Func<Action<Action<BaseClassGrandchild>>>)null;
            //// taking it to extremes
            //Action<Func<Func<Func<Action<BaseClass>>>>> m5 = (Action<Func<Func<Func<Action<BaseClassGrandchild>>>>>)null;
            //Action<Func<Func<Func<Action<Action<BaseClassGrandchild>>>>>> m6 = (Action<Func<Func<Func<Action<Action<BaseClass>>>>>>)null;

            //// 'odd' contravariance behaves normally
            //// 'even' contravariance is reversed, so IContra<> <--> Contra<> and BaseClass <--> BaseClassGrandchild
            //// alternately as we descend through the varying levels of contra-ness.  I'm yet to fully grasp exactly why,
            //// except when I do a proper worked example it always makes sense.
            //Action<Contravariant<BaseClass>> n1 = (Action<IContravariant<BaseClassGrandchild>>)null;
            //Action<Contravariant<Action<BaseClassGrandchild>>> n2 = (Action<IContravariant<Action<BaseClass>>>)null;
            //Action<Contravariant<IContravariant<BaseClassGrandchild>>> n3 = (Action<IContravariant<Contravariant<BaseClass>>>)null;
        }

        [Fact]
        public void ShouldDisableContravarianceGlobally()
        {
            // we need a target container to set options
            var targets = new TargetContainer();
            targets.SetOption<Options.EnableContravariance>(false);
            var result = new TargetTypeSelector(typeof(IContravariant<BaseClassGrandchild>), targets).ToArray();

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
        public void ShouldDisableContravarianceForOpenGeneric()
        {
            var targets = new TargetContainer();
            // test shows that contravariance still works for Action<> but is disabled for all IContravariant<>
            targets.SetOption<Options.EnableContravariance>(false, typeof(IContravariant<>));
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

        private void LogTypes(Type[] types, string header)
        {
            Output.WriteLine(header);
            Output.WriteLine("================");
            
            int counter = 0;
            StringBuilder sb = new StringBuilder();
            foreach(var type in types)
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

        private void LogExpected(Type[] expected)
        {
            LogTypes(expected, "Expected");
        }
    }
}
