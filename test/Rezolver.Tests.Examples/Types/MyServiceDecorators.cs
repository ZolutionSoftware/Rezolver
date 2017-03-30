using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class MyServiceDecorator1 : IMyService
    {
        public IMyService Inner { get; }
        public MyServiceDecorator1(IMyService inner)
        {
            Inner = inner;
        }
    }

    public class MyServiceDecorator2 : IMyService
    {
        public IMyService Inner { get; }
        public MyServiceDecorator2(IMyService inner)
        {
            Inner = inner;
        }
    }
    //</example>
}
