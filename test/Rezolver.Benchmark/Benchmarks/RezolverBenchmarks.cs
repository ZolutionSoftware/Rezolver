using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Benchmark.Types;

namespace Rezolver.Benchmark.Benchmarks
{
    public class RezolverBenchmarks : ContainerBenchmarks<IContainer>
    {
        public override string ContainerType => "Rezolver";

        public sealed class Simple : PreparedBenchmark
        {
            protected override IContainer CreateAndPrepareContainer()
            {
                var container = new Container();
                container.RegisterType<SimpleType>();
                return container;
            }

            protected override void Run(IContainer container, Stopwatch sw)
            {
                sw.Start();
                var a = container.Resolve<SimpleType>();
                sw.Stop();
            }
        }

        public sealed class Simple_Precompiled : PreparedBenchmark
        {
            public override string BenchmarkName => "Simple";
            public override string ExtendedName => "Precompiled";
            protected override IContainer CreateAndPrepareContainer()
            {
                var container = new Container();
                container.RegisterType<SimpleType>();
                var a = container.Resolve<SimpleType>();
                return container;
            }

            protected override void Run(IContainer container, Stopwatch sw)
            {
                sw.Start();
                var a = container.Resolve<SimpleType>();
                sw.Stop();
            }
        }

        public sealed class RequiresSimple : PreparedBenchmark
        {
            protected override IContainer CreateAndPrepareContainer()
            {
                var container = new Container();
                container.RegisterType<RequiresSimpleType>();
                container.RegisterType<SimpleType>();
                return container;
            }

            protected override void Run(IContainer container, Stopwatch sw)
            {
                sw.Start();
                var a = container.Resolve<RequiresSimpleType>();
                sw.Stop();
            }
        }

        public sealed class RequiresSimple_Precompiled : PreparedBenchmark
        {
            public override string BenchmarkName => "RequiresSimple";
            public override string ExtendedName => "Precompiled";

            protected override IContainer CreateAndPrepareContainer()
            {
                var container = new Container();
                container.RegisterType<RequiresSimpleType>();
                container.RegisterType<SimpleType>();
                var a = container.Resolve<RequiresSimpleType>();
                return container;
            }

            protected override void Run(IContainer container, Stopwatch sw)
            {
                sw.Start();
                var a = container.Resolve<RequiresSimpleType>();
                sw.Stop();
            }
        }

        public sealed class RequiresLotsAndSingleton : PreparedBenchmark
        {
            protected override IContainer CreateAndPrepareContainer()
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

            protected override void Run(IContainer container, Stopwatch sw)
            {
                sw.Start();
                var a = container.Resolve<Types.RequiresLotsAndSingleton>();
                sw.Stop();
            }
        }

        public sealed class RequiresLotsAndSingleton_Precompiled : PreparedBenchmark
        {
            public override string BenchmarkName => "RequiresLotsAndSingleton";
            public override string ExtendedName => "Precompiled";
            protected override IContainer CreateAndPrepareContainer()
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
                var a = container.Resolve<Types.RequiresLotsAndSingleton>();
                return container;
            }

            protected override void Run(IContainer container, Stopwatch sw)
            {
                sw.Start();
                var a = container.Resolve<Types.RequiresLotsAndSingleton>();
                sw.Stop();
            }
        }

        public sealed class Create_Container_Register_And_Resolve_Simple : UnpreparedBenchmark
        {
            protected override void Run(Stopwatch sw)
            {
                sw.Start();
                var container = new Container();
                container.RegisterType<SimpleType>();
                var a = container.Resolve<SimpleType>();
                sw.Stop();
            }
        }

        public sealed class Register_And_Resolve_Simple : UnpreparedBenchmark
        {
            protected override void Run(Stopwatch sw)
            {
                var container = new Container();
                sw.Start();
                container.RegisterType<SimpleType>();
                var a = container.Resolve<SimpleType>();
                sw.Stop();
            }
        }
    }
}
