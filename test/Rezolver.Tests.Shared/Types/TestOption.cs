using Rezolver.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class TestOption : ContainerOption<string>
    {
        //simple example of how to implement your own custom option

        public static implicit operator TestOption(string value)
        {
            return new TestOption() { Value = value };
        }
    }
}
