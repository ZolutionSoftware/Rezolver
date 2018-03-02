using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Benchmark
{
    /// <summary>
    /// An operation which is benchmarked for different containers.
    /// 
    /// The individual container benchmark implementations are passed on construction
    /// </summary>
    internal class Benchmark
    {
        public string Name { get; internal set; }
        public IEnumerable<ContainerBenchmarkBase> ContainerBenchmarks { get; }

        public Benchmark(string name, IEnumerable<ContainerBenchmarkBase> containerBenchmarks)
        {
            Name = name;
            ContainerBenchmarks = containerBenchmarks.ToArray();
        }

        public IEnumerable<ContainerBenchmarkBase> IncludeContainerTypes(IEnumerable<string> types)
        {
            HashSet<string> filterItems = new HashSet<string>(types ?? Enumerable.Empty<string>());
            return ContainerBenchmarks.Where(b => filterItems.Contains(b.ContainerType));
        }

        public IEnumerable<ContainerBenchmarkBase> ExcludeContainerTypes(IEnumerable<string> types)
        {
            HashSet<string> filterItems = new HashSet<string>(types ?? Enumerable.Empty<string>());
            return ContainerBenchmarks.Where(b => !filterItems.Contains(b.ContainerType));
        }
    }
}
