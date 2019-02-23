using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark
{
    class BenchDotNetProgram
    {
        public static int Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CreationBenches>();
            return 0;
        }
    }
}
