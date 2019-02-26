using System;
using System.Diagnostics;
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

        private static int _numInstances = 0;
        private static Container _container = new Container();
        private static Container SetupContainer()
        {
            var container = new Container();
            container.RegisterType<NoCtor>();

            return container;
        }

        static void Main(string[] args)
        {
            const int runTimeSecs = 30;

            CancellationTokenSource cancel = new CancellationTokenSource(runTimeSecs * 1000);

            Run_NoCtor(SetupContainer(), cancel.Token, true);
            Console.WriteLine($"Num instances created in {runTimeSecs}: {_numInstances}. Rate: {(_numInstances / runTimeSecs):0.00}/sec");
        }

        private static void Run_NoCtor(Container container, CancellationToken cancel, bool warmup)
        {
            NoCtor instance;
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

    }
}
