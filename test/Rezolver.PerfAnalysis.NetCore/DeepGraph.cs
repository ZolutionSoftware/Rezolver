using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.PerfAnalysis.NetCore
{
    public class DeepGraph10
    {
        public DeepGraph10(DeepGraph9 inner)
        {
            Inner = inner;
        }

        public DeepGraph9 Inner { get; }
    }

    public class DeepGraph9
    {
        public DeepGraph9(DeepGraph8 inner)
        {
            Inner = inner;
        }

        public DeepGraph8 Inner { get; }
    }

    public class DeepGraph8
    {
        public DeepGraph8(DeepGraph7 inner)
        {
            Inner = inner;
        }

        public DeepGraph7 Inner { get; }
    }

    public class DeepGraph7
    {
        public DeepGraph7(DeepGraph6 inner)
        {
            Inner = inner;
        }

        public DeepGraph6 Inner { get; }
    }

    public class DeepGraph6
    {
        public DeepGraph6(DeepGraph5 inner)
        {
            Inner = inner;
        }

        public DeepGraph5 Inner { get; }
    }

    public class DeepGraph5
    {
        public DeepGraph5(DeepGraph4 inner)
        {
            Inner = inner;
        }

        public DeepGraph4 Inner { get; }
    }

    public class DeepGraph4
    {
        public DeepGraph4(DeepGraph3 inner)
        {
            Inner = inner;
        }

        public DeepGraph3 Inner { get; }
    }

    public class DeepGraph3
    {
        public DeepGraph3(DeepGraph2 inner)
        {
            Inner = inner;
        }

        public DeepGraph2 Inner { get; }
    }

    public class DeepGraph2
    {
        public DeepGraph2(DeepGraph1 inner)
        {
            Inner = inner;
        }

        public DeepGraph1 Inner { get; }
    }
    public class DeepGraph1
    {
        public DeepGraph1(DeepGraph inner)
        {
            Inner = inner;
        }

        public DeepGraph Inner { get; }
    }

    public class DeepGraph
    {
        public DeepGraph()
        {

        }
    }
}
