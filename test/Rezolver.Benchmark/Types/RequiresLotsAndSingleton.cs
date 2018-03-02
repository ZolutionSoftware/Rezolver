using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark.Types
{
    public class RequiresLotsAndSingleton
    {
        private readonly RequiresLots _rl;
        private readonly Singleton _sg;

        public RequiresLotsAndSingleton(RequiresLots rl, Singleton sg)
        {
            _rl = rl;
            _sg = sg;
        }
    }
}
