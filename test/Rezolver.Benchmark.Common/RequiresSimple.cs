using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark.Common
{
    public class RequiresSimple
    {
        readonly Simple _simple;
        public RequiresSimple(Simple simple)
        {
            _simple = simple;
        }
    }
}
