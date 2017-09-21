using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ContravarianceExamples
    {
        [Fact]
        public void ShouldFetchOrderedIntEnumerable()
        {
            // This test doesn't actually demonstrate contravariance - it's used as a lead-in to the
            // wider example in the seconnd test.

            // <example1>
            var container = new Container();
            container.RegisterType(typeof(DefaultComparerWrapper<>), typeof(IComparer<>));
            container.RegisterType(typeof(OrderedEnumerableWrapper<>), typeof(IOrderedEnumerable<>));
            // explicit registration for IEnumerable<int>
            container.RegisterObject(Enumerable.Range(0, 10).Reverse());

            var unordered = container.ResolveMany<int>();
            var ordered = container.Resolve<IOrderedEnumerable<int>>();

            Assert.NotEqual(unordered, ordered);

            Assert.Equal(Enumerable.Range(0, 10), ordered);
            // </example1>
        }

        [Fact]
        public void ShouldUseOneComparerViaContravariance()
        {
            // See BUG #53
            // <example2>
            var container = new Container();

            // use the same two wrapper registrations as before.  We need the default
            // comparer to inject IComparer<double> into our ShapeAreaComparer
            container.RegisterType(typeof(DefaultComparerWrapper<>), typeof(IComparer<>));
            container.RegisterType(typeof(OrderedEnumerableWrapper<>), typeof(IOrderedEnumerable<>));
            // now register our shape comparer
            container.RegisterType<ShapeAreaComparer, IComparer<I2DShape>>();


            // ## TEST 1: Check contravariance is working for the comparer types:
            var rectangleComparer =
                Assert.IsType<ShapeAreaComparer>(container.Resolve<IComparer<Rectangle>>());
            var squareComparer =
                Assert.IsType<ShapeAreaComparer>(container.Resolve<IComparer<Square>>());
            var circleComparer =
                Assert.IsType<ShapeAreaComparer>(container.Resolve<IComparer<Circle>>());


            // ## TEST 2: Add some shapes and make sure they get sorted correctly:
            container.RegisterObject<IEnumerable<Rectangle>>(
                new Rectangle[] { new Rectangle(3, 1), new Rectangle(1, 2), new Rectangle(1, 1) });
            container.RegisterObject<IEnumerable<Square>>(
                new Square[] { new Square(3), new Square(2), new Square(1) });
            container.RegisterObject<IEnumerable<Circle>>(
                new Circle[] { new Circle(3), new Circle(2), new Circle(1) });

            var sortedRectangles = container.Resolve<IOrderedEnumerable<Rectangle>>();
            var sortedSquares = container.Resolve<IOrderedEnumerable<Square>>();
            var sortedCircles = container.Resolve<IOrderedEnumerable<Circle>>();

            Assert.Equal(
                Enumerable.Range(1, 3).Select(i => (double)i),
                sortedRectangles.Select(r => r.CalcArea()));

            Assert.Equal(
                Enumerable.Range(1, 3).Select(i=> (double)i * i),
                sortedSquares.Select(s => s.CalcArea()));

            Assert.Equal(
                Enumerable.Range(1, 3).Select(i => Math.PI * Math.Pow(i, 2)),
                sortedCircles.Select(c => c.CalcArea()));
            // </example2>
        }


        [Fact]
        public void SpecificRegistrationShouldOverrideContravariant()
        {
            // <example3>
            var container = new Container();
            container.RegisterType(typeof(DefaultComparerWrapper<>), typeof(IComparer<>));
            container.RegisterType(typeof(OrderedEnumerableWrapper<>), typeof(IOrderedEnumerable<>));
            // register the comparers
            container.RegisterType<RectangleComparer, IComparer<Rectangle>>();
            container.RegisterType<ShapeAreaComparer, IComparer<I2DShape>>();

            // Requesting either IComparer<Rectangle> *or* IComparer<Square> now
            // resolves to the RectangleComparer - because Rectangle is Square's base.
            Assert.IsType<RectangleComparer>(container.Resolve<IComparer<Rectangle>>());
            Assert.IsType<RectangleComparer>(container.Resolve<IComparer<Square>>());

            // This array is written in the exact opposite order to the one we desire
            // And a stable sort by area would leave objects 1 + 2 in the same order, 
            // and 3, 4 + 5 in the same order.
            IEnumerable<Rectangle> rectangles = new[]
            {
                new Rectangle(20, 1), //╗
                new Rectangle(1, 20), //╩ Area = 20
                new Rectangle(8, 2),  //╗
                new Square(4),        //╠ Area = 16
                new Rectangle(2, 8),  //╝
            };

            container.RegisterObject(rectangles);

            Assert.Equal(
                rectangles.Reverse(),
                container.Resolve<IOrderedEnumerable<Rectangle>>());
            // </example3>
        }

        [Fact]
        public void EmptyDecoratorShouldInjectContravariantBase()
        {
            // There is a bug with this - and so it's not currently included in the documentation.
            // See https://github.com/ZolutionSoftware/Rezolver/issues/54
            // When the bug is fixed - add the decorator approach as an alternative to the third example

            // <example4>
            var container = new Container();
            container.RegisterType(typeof(DefaultComparerWrapper<>), typeof(IComparer<>));
            container.RegisterType(typeof(OrderedEnumerableWrapper<>), typeof(IOrderedEnumerable<>));
            // register the comparers
            container.RegisterDecorator<RectangleComparerDecorator, IComparer<Rectangle>>();
            container.RegisterType<ShapeAreaComparer, IComparer<I2DShape>>();

            // Requesting either IComparer<Rectangle> *or* IComparer<Square> now
            // resolves to the RectangleComparer - because Rectangle is Square's base.
            Assert.IsType<RectangleComparerDecorator>(container.Resolve<IComparer<Rectangle>>());
            // BUG #54: this should also work:
            //Assert.IsType<RectangleComparerDecorator>(container.Resolve<IComparer<Square>>());

            // This array is written in the exact opposite order to the one we desire
            // And a stable sort by area would leave objects 1 + 2 in the same order, 
            // and 3, 4 + 5 in the same order.
            IEnumerable<Rectangle> rectangles = new[]
            {
                new Rectangle(20, 1), //╗
                new Rectangle(1, 20), //╩ Area = 20
                new Rectangle(8, 2),  //╗
                new Square(4),        //╠ Area = 16
                new Rectangle(2, 8),  //╝
            };

            container.RegisterObject(rectangles);

            Assert.Equal(
                rectangles.Reverse(),
                container.Resolve<IOrderedEnumerable<Rectangle>>());
            // </example4>
        }
    }
}
