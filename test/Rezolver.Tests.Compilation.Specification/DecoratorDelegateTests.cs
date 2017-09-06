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
                    t.RegisterDecorator(AppendToStringArray("world"));
                }
            },
            {
                "before",
                new[] { "hello", "world"},
                t =>
                {
                    t.RegisterDecorator(AppendToStringArray("world"));
                    t.RegisterObject("hello");
                }
            },
            {
                "append..toupper (after)",
                new[] { "HELLO", "WORLD" },
                t =>
                {
                    t.RegisterObject("hello");
                    t.RegisterDecorator(AppendToStringArray("world"));
                    t.RegisterDecorator((string[] ss) =>{
                        for(var f = 0; f<ss.Length; f++)
                        {
                            ss[f] = ss[f].ToUpperInvariant();
                        }
                        return ss;
                    });
                }
            },
            {
                "append..toupper (before)",
                new[] { "HELLO", "WORLD" },
                t =>
                {
                    t.RegisterDecorator(AppendToStringArray("world"));
                    t.RegisterDecorator((string[] ss) =>{
                        for(var f = 0; f<ss.Length; f++)
                        {
                            ss[f] = ss[f].ToUpperInvariant();
                        }
                        return ss;
                    });
                    t.RegisterObject("hello");
                }
            },
            {
                "append..toupper (split)",
                new[] { "HELLO", "WORLD" },
                t =>
                {
                    t.RegisterDecorator(AppendToStringArray("world"));
                    t.RegisterObject("hello");
                    t.RegisterDecorator((string[] ss) =>{
                        for(var f = 0; f<ss.Length; f++)
                        {
                            ss[f] = ss[f].ToUpperInvariant();
                        }
                        return ss;
                    });
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
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = container.Resolve<string[]>();
            Assert.Equal(expected, result);
        }
    }
}
