using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class Page
    {
        public string Caption { get; set; }
    }
    public class HomePage : Page
    {

    }

    public class AboutPage : Page
    {

    }

    public interface IPageBuilder
    {
        Page Build();
    }

    public interface IPageBuilder<out TPage> : IPageBuilder
        where TPage : Page
    {
        new TPage Build();
    }

    public class DelegatedPageBuilder<TPage> : IPageBuilder<TPage>
        where TPage : Page
    {
        Lazy<TPage> _lazy;

        public DelegatedPageBuilder(TPage page, Action<TPage> activator)
        {
            _lazy = new Lazy<TPage>(() => {
                activator(page);
                return page;
            });
        }
        public TPage Build() => _lazy.Value;
        Page IPageBuilder.Build() => Build();
    }
    // </example>
}
