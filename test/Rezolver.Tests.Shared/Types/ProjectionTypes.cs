using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class From
    {

    }

    public class From1 : From { }

    public class From2 : From { }

    public interface ITo { }

    public interface ITo<out TFrom> : ITo { }

    public class To : ITo
    {

    }

    public class To<TFrom> : To, ITo<TFrom>
    {

    }
}
