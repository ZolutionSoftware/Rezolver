using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.PerfAnalysis.NetCore
{
    class Program
    {
        public class NoCtor
        {
            public NoCtor()
            {
                //System.Diagnostics.Debugger.Break();
            }
        }

        public class EnumerableItem
        {
            private int counter = 0;
            public void DoSomething()
            {
                counter++;
            }
        }

        public class EnumerableItem1 : EnumerableItem
        {

        }

        public class EnumerableItem2 : EnumerableItem
        {

        }

        public class EnumerableItem3 : EnumerableItem
        {

        }

        private static int _numInstances = 0;

        static void Main(string[] args)
        {
            const int runTimeSecs = 30;

            CancellationTokenSource cancel = new CancellationTokenSource(runTimeSecs * 1000);

            //Run_NoCtor(new Container(), cancel.Token, true);
            //Run_NoCtor_NonGeneric(new Container(), cancel.Token, true);
            //Run_NoCtor_Singleton(new Container(), cancel.Token, true);
            Run_Enumerable(new Container(), cancel.Token, true);

            Console.WriteLine($"Num instances created in {runTimeSecs}: {_numInstances}. Rate: {(_numInstances / runTimeSecs):0.00}/sec");
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
                ++_numInstances;
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
                ++_numInstances;
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
                ++_numInstances;
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
                ++_numInstances;
            }
        }
    }
}

