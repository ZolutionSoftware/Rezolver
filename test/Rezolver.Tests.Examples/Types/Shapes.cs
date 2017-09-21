using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public interface I2DShape
    {
        double CalcArea();
    }

    public class Rectangle : I2DShape
    {
        public double Length { get; }
        public double Height { get; }

        public Rectangle(double length, double height)
        {
            Length = length;
            Height = height;
        }

        public double CalcArea()
        {
            return Length * Height;
        }
    }

    public class Square : Rectangle
    {
        public Square(double size)
            : base(size, size)
        {

        }
    }

    public class Circle : I2DShape
    {
        public double Radius { get; }

        public Circle(double radius)
        {
            Radius = radius;
        }

        public double CalcArea()
        {
            return Math.PI * Math.Pow(Radius, 2);
        }
    }
    // </example>
}
