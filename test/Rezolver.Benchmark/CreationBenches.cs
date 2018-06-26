using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using Rezolver.Benchmark.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark
{
    [ClrJob]
    //[CoreJob]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [MemoryDiagnoser]
    public class CreationBenches
    {
        private Lazy<Singleton> _singleton;
        private IContainer _containerUnprepped;
        private IContainer _containerPrepped;
        private List<object> _warmed;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _singleton = new Lazy<Singleton>(() => new Singleton());

            _containerUnprepped = PrepContainer();
            _containerPrepped = PrepContainer();

            _warmed = Warmup(_containerPrepped);
        }

        private List<object> Warmup(IContainer container)
        {
            // literally just calls each of the warm methods once to force compilation
            return new List<object>
            {
                Rezolver_New_Warm(),
                Rezolver_NewDependency_Warm(),
                Rezolver_Complex_Warm()
            };
        }

        private IContainer PrepContainer()
        {
            var container = new Container();

            container.RegisterType<SimpleType>();
            container.RegisterType<SimpleType2>();
            container.RegisterType<SimpleType3>();
            container.RegisterType<RequiresSimpleType>();
            container.RegisterType<RequiresSimpleType2>();
            container.RegisterType<RequiresSimpleType3>();
            container.RegisterType<RequiresLots>();
            container.RegisterSingleton<Singleton>();
            container.RegisterType<Types.RequiresLotsAndSingleton>();

            return container;
        }

        #region no container benchmarks

        [Benchmark(Baseline =true)]
        [BenchmarkCategory("New")]
        public SimpleType NoContainer_New() => new SimpleType();
       
        [Benchmark(Baseline = true)]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType NoContainer_NewDependency() => new RequiresSimpleType(new SimpleType());

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton NoContainer_Complex()
            => new Types.RequiresLotsAndSingleton(new Types.RequiresLots(new RequiresSimpleType(new SimpleType()),
                    new RequiresSimpleType2(new SimpleType2()), new RequiresSimpleType3(new SimpleType3()), new SimpleType(), new SimpleType2(), new SimpleType3()), _singleton.Value);

        #endregion

        #region rezolver benchmarks (prepared)

        [Benchmark]
        [BenchmarkCategory("New")]
        public SimpleType Rezolver_New_Warm() => _containerPrepped.Resolve<SimpleType>();

        [Benchmark]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType Rezolver_NewDependency_Warm() => _containerPrepped.Resolve<RequiresSimpleType>();

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton Rezolver_Complex_Warm() => _containerPrepped.Resolve<RequiresLotsAndSingleton>();


        #endregion

        #region rezolver benchmarks (unprepared)

        [Benchmark]
        [BenchmarkCategory("New")]
        public SimpleType Rezolver_New_Cold() => _containerUnprepped.Resolve<SimpleType>();

        [Benchmark]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType Rezolver_NewDependency_Cold() => _containerUnprepped.Resolve<RequiresSimpleType>();

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton Rezolver_Complex_Cold() => _containerUnprepped.Resolve<RequiresLotsAndSingleton>();


        #endregion
    }
}
