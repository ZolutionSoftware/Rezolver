using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.PerfAnalysis.NetCore
{
    partial class Program
    {
        private static int _numIterations = 0;

        static void Main(string[] args)
        {
            const int runTimeSecs = 30;

            CancellationTokenSource cancel = new CancellationTokenSource(runTimeSecs * 1000);

            //Run_NoCtor(new Container(), cancel.Token, true);
            //Run_NoCtor_NonGeneric(new Container(), cancel.Token, true);
            //Run_NoCtor_Singleton(new Container(), cancel.Token, true);
            //Run_Enumerable(new Container(), cancel.Token, true);
            //Run_Compile_Transient(new Container(), cancel.Token);
            Run_Compile_Singleton(new Container(), cancel.Token);

            Console.WriteLine($"Num iterations in {runTimeSecs}: {_numIterations}. Rate: {(_numIterations / runTimeSecs):0.00}/sec");
        }

        private static void Run_NoCtor(Container container, CancellationToken cancel, bool warmup)
        {
            Console.WriteLine($"Creating instances of {nameof(NoCtor)}");
            NoCtor instance;
            container.RegisterType<NoCtor>();
            if (warmup)
            {
                instance = container.Resolve<NoCtor>();
            }

            while (!cancel.IsCancellationRequested)
            {
                instance = container.Resolve<NoCtor>();
                ++_numIterations;
            }
        }

        private static void Run_NoCtor_Singleton(Container container, CancellationToken cancel, bool warmup)
        {
            Console.WriteLine($"Fetching {nameof(NoCtor)} as a singleton");
            NoCtor instance;
            container.RegisterSingleton<NoCtor>();
            if (warmup)
            {
                instance = container.Resolve<NoCtor>();
            }

            while (!cancel.IsCancellationRequested)
            {
                instance = container.Resolve<NoCtor>();
                ++_numIterations;
            }
        }

        private static void Run_NoCtor_NonGeneric(Container container, CancellationToken cancel, bool warmup)
        {
            Console.WriteLine($"Creating instances of {nameof(NoCtor)} using non-generic");
            NoCtor instance;
            container.RegisterType<NoCtor>();
            if (warmup)
            {
                instance = (NoCtor)container.Resolve(typeof(NoCtor));
            }

            while (!cancel.IsCancellationRequested)
            {
                instance = (NoCtor)container.Resolve(typeof(NoCtor));
                ++_numIterations;
            }
        }

        private static void Run_Enumerable(Container container, CancellationToken cancel, bool warmup)
        {
            Console.WriteLine($"Creating and enumerating instances of enumerables of {nameof(EnumerableItem)}");
            IEnumerable<EnumerableItem> instance;
            container.RegisterType<EnumerableItem1>();
            container.RegisterType<EnumerableItem2>();
            container.RegisterType<EnumerableItem3>();
            if (warmup)
            {
                instance = container.Resolve<IEnumerable<EnumerableItem>>();
                foreach(var item in instance)
                {
                    item.DoSomething();
                }
            }

            while (!cancel.IsCancellationRequested)
            {
                instance = container.Resolve<IEnumerable<EnumerableItem>>();
                foreach (var item in instance)
                {
                    item.DoSomething();
                }
                ++_numIterations;
            }
        }

        private static void Run_Compile_Transient(Container container, CancellationToken cancel)
        {
            Console.WriteLine($"Compiling and executing delegates for deep nested dependency graph");

            container.RegisterType<DeepGraph10>();
            container.RegisterType<DeepGraph9>();
            container.RegisterType<DeepGraph8>();
            container.RegisterType<DeepGraph7>();
            container.RegisterType<DeepGraph6>();
            container.RegisterType<DeepGraph5>();
            container.RegisterType<DeepGraph4>();
            container.RegisterType<DeepGraph3>();
            container.RegisterType<DeepGraph2>();
            container.RegisterType<DeepGraph1>();
            container.RegisterType<DeepGraph>();

            var resolveContext = new ResolveContext(container, typeof(DeepGraph10));
            DeepGraph10 instance;

            while (!cancel.IsCancellationRequested)
            {
                var factory = container.GetWorker<DeepGraph10>(resolveContext);
                instance = factory(resolveContext);
                ++_numIterations;
            }
        }

        private static void Run_Compile_Singleton(Container container, CancellationToken cancel)
        {
            Console.WriteLine($"Compiling and executing delegates for deep nested dependency graph");

            container.RegisterSingleton<DeepGraph10>();
            container.RegisterSingleton<DeepGraph9>();
            container.RegisterSingleton<DeepGraph8>();
            container.RegisterSingleton<DeepGraph7>();
            container.RegisterSingleton<DeepGraph6>();
            container.RegisterSingleton<DeepGraph5>();
            container.RegisterSingleton<DeepGraph4>();
            container.RegisterSingleton<DeepGraph3>();
            container.RegisterSingleton<DeepGraph2>();
            container.RegisterSingleton<DeepGraph1>();
            container.RegisterSingleton<DeepGraph>();

            var resolveContext = new ResolveContext(container, typeof(DeepGraph10));
            DeepGraph10 instance;

            while (!cancel.IsCancellationRequested)
            {
                var factory = container.GetWorker<DeepGraph10>(resolveContext);
                instance = factory(resolveContext);
                ++_numIterations;
            }
        }
    }
}

