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

    public interface ITo {
        From From { get; }
    }

    public interface ITo<out TFrom> : ITo
        where TFrom : From
    {
        new TFrom From { get; }
    }

    public class To : ITo
    {
        public From From { get; }

        public To(From from)
        {
            From = from;
        }
    }

    public class To<TFrom> : To, ITo<TFrom>
        where TFrom : From
    {
        public new TFrom From { get => (TFrom)base.From; }
        public To(TFrom from)
            : base(from)
        {
        }
    }

    public class ToDecorator<TFrom> : ITo<TFrom>
        where TFrom : From
    {

        public ToDecorator(ITo<TFrom> inner)
        {
            Inner = inner;
        }

        public ITo<TFrom> Inner { get; }

        public TFrom From => Inner.From;

        From ITo.From => From;
    }
}
