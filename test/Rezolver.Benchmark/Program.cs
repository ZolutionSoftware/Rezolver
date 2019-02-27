using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Rezolver.Benchmark
{
    class Program
    {
        static int Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CreationBenches>();
            return 0;
        }
    }
}
