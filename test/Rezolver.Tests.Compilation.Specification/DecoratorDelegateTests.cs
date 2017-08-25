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

        [Fact]
        public void DecoratorDelegate_ShouldDecorateArrayAndIEnumerable()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(2);
            targets.RegisterDecorator<IEnumerable<int>>(ii => new[] { 1 }.Concat(ii));
            targets.RegisterDecorator<int[]>(iii =>
            {
                var toReturn = new int[iii.Length + 1];
                Array.Copy(iii, toReturn, iii.Length);
                toReturn[2] = 2;
                return toReturn;
            });

            var container = CreateContainer(targets);

            var result = container.Resolve<int[]>();
        }
    }
}
