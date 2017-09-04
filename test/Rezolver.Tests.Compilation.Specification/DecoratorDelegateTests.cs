using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using SetupTargets = System.Action<Rezolver.ITargetContainer>;

namespace Rezolver.Tests.Compilation.Specification
{
    using DecoratorTheoryDataWithResult = TheoryData<string, object, SetupTargets>;

    public partial class CompilerTestsBase
    {
        public static DecoratorTheoryDataWithResult IntDecorations =>
            new DecoratorTheoryDataWithResult()
            {
                {
                    "after",
                    2,
                    t =>
                    {
                        t.RegisterObject(1);
                        t.RegisterDecorator<int>(i => i * 2);
                    }
                },
                {
                    "before",
                    2,
                    t =>
                    {
                        t.RegisterDecorator<int>(i => i * 2);
                        t.RegisterObject(1);
                    }
                },
                {
                    "add..multiply (after)",
                    12,
                    t =>
                    {
                        t.RegisterObject(1);
                        t.RegisterDecorator((int i) => i + 5);
                        t.RegisterDecorator<int>(i => i * 2);
                    }
                },
                {
                    "add..multiply (before)",
                    12,
                    t =>
                    {
                        // just trying both syntaxes here
                        t.RegisterDecorator((int i) => i + 5);
                        t.RegisterDecorator<int>(i => i * 2);
                        t.RegisterObject(1);
                    }
                },
                {
                    "add..multiply (split)",
                    12,
                    t =>
                    {
                        // just trying both syntaxes here
                        t.RegisterDecorator((int i) => i + 5);
                        t.RegisterObject(1);
                        t.RegisterDecorator<int>(i => i * 2);
                    }
                },
            };

        public static DecoratorTheoryDataWithResult EnumerableIntDecorations => new DecoratorTheoryDataWithResult()
        {
            {
                "after",
                new[] { 1, -1 },
                t =>
                {
                    t.RegisterObject(1);
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Concat(new[] { -1 }));
                }
            },
            {
                "before",
                new[] { 1, -1 },
                t =>
                {
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Concat(new[] { -1 }));
                    t.RegisterObject(1);
                }
            },
            {
                "concat..reverse (after)",
                new[] { -1, 1 },
                t =>
                {
                    t.RegisterObject(1);
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Concat(new[] { -1 }));
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Reverse());
                }
            },
            {
                "concat..reverse (before)",
                new[] { -1, 1 },
                t =>
                {
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Concat(new[] { -1 }));
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Reverse());
                    t.RegisterObject(1);
                }
            },
            {
                "concat..reverse (split)",
                new[] { -1, 1 },
                t =>
                {
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Concat(new[] { -1 }));
                    t.RegisterObject(1);
                    t.RegisterDecorator<IEnumerable<int>>(ii => ii.Reverse());
                }
            },
        };

        private static Func<string[], string[]> AppendToStringArray(string value)
        {
            return (string[] input) =>
            {
                string[] newArray = new string[input.Length + 1];
                Array.Copy(input, newArray, input.Length);
                newArray[input.Length] = value;
                return newArray;
            };
        }

        public static DecoratorTheoryDataWithResult ArrayDecorations => new DecoratorTheoryDataWithResult()
        {
            {
                "after",
                new[] { "hello", "world"},
                t =>
                {
                    t.RegisterObject("hello");
                    t.RegisterDecorator<string[]>(AppendToStringArray("world"));
                }
            },
            {
                "before",
                new[] { "hello", "world"},
                t =>
                {
                    t.RegisterDecorator<string[]>(AppendToStringArray("world"));
                    t.RegisterObject("hello");
                }
            }
        };


        [Theory]
        [MemberData(nameof(IntDecorations))]
        public void DecoratorDelegate_ShouldDecorateInt(string name, int expected, SetupTargets setup)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);
            
            Assert.Equal(expected, container.Resolve<int>());
        }

        [Theory]
        [MemberData(nameof(EnumerableIntDecorations))]
        public void DecoratorDelegate_ShouldDecorateEnumerableOfInt(string name, IEnumerable<int> expected, SetupTargets setup)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = container.Resolve<IEnumerable<int>>();
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ArrayDecorations))]
        public void DecoratorDelegate_ShouldDecorateArrayOfStrings(string name, string[] expected, SetupTargets setup)
        {
            // The reason these tests fail is because the DecoratingTargetContainer is neither registered correctly for the
            // array type (see GetCorrectDecoratorTargetType in <root>\src\Rezolver\ITargetContainer.DecoratorExtensions.cs)
            // nor does it create its inner container correctly.
            // The first method does not honour the ITargetContainerTypeResolve option; and neither methods honour the
            // ITargetContainerFactory option.
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = container.Resolve<string[]>();
            Assert.Equal(expected, result);
        }
    }
}
