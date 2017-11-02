using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
    class Page
    {
        public string Caption { get; set; }
    }

    class TabbedPage : Page
    {
        public List<Page> Tabs { get; set; }
    }

    interface IPageActivator<out TPage> where TPage : Page
    {
        TPage Page { get; }
    }

    class PageActivator<TPage>  : IPageActivator<TPage>
        where TPage : Page
    {
        public TPage Page { get; }
        public PageActivator(TPage page, Action<TPage> activator)
        {
            activator(page);
            Page = page;
        }
    }

    // pages
    class HomePage : Page
    {

    }

    class AboutPage : Page
    {

    }



    public partial class TargetContainerTests
    {
        public void ShouldProjectTwoPageActivators()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.RegisterType<HomePage>();
            targets.RegisterType<AboutPage>();
            targets.RegisterObject<Action<HomePage>>(hp => hp.Caption = "Home");
            targets.RegisterObject<Action<AboutPage>>(ap => ap.Caption = "About");

            // Act

            // Assert
        }

    }
}
