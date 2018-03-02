using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class GenericTwoCtors<T>
    {
        // this type has two constructors, and is used
        // to test GenericConstructorTarget's ability to bind to a specific
        // constructor on an open generic type.  We set up a container which has
        // a registration for T, and both int and double which will cause an ambiguous
        // match under normal circumstances.

        public T Prop1 { get; }
        public int Prop2Int { get; }
        public double Prop2Double { get; }

        public GenericTwoCtors(T arg1, double arg2)
        {
            Prop1 = arg1;
            Prop2Double = arg2;
        }

        public GenericTwoCtors(T arg1, int arg2)
        {
            Prop1 = arg1;
            Prop2Int = arg2;
        }
    }
}
