using Rezolver;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XForms.Views;
using System.Collections;
using System.Linq;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace XForms
{
    public abstract class PageBuilder
    {
        public abstract Page Build();
    }
    public class PageBuilder<TPage> : PageBuilder
        where TPage : Page
    {
        private Lazy<Page> _lazyBuilder;
        public override Page Build() => _lazyBuilder.Value;
        public PageBuilder(TPage page, Action<TPage> configurePage = null)
        {
            _lazyBuilder = new Lazy<Page>(() =>
            {
                configurePage?.Invoke(page);
                return page;
            });
        }
    }

    public class NavigationPage<TRootPage> : NavigationPage
        where TRootPage : Page
    {
        public NavigationPage(PageBuilder<TRootPage> builder, Action<NavigationPage<TRootPage>> configurePage = null)
            : base(builder.Build())
        {
            configurePage(this);
        }
    }

    public partial class App : Application
    {
        public static CombinedTargetContainerConfig TargetContainerConfig { get; } =
            TargetContainer.DefaultConfig.Clone();

        public static CombinedContainerConfig ContainerConfig { get; } =
            Rezolver.Container.DefaultConfig.Clone();

        public static IRootTargetContainer Targets { get; } = new TargetContainer(TargetContainerConfig);

        public static IContainer Container { get; private set; }

        public App()
        {
            InitializeComponent();
            if (Container == null)
            {
                Targets.RegisterType<ItemsPage>();
                Targets.RegisterType<AboutPage>();

                Targets.RegisterObject<Action<NavigationPage<ItemsPage>>>(p =>
                {
                    p.Title = "Browse";
                    p.Icon = Device.OnPlatform("tab_feed.png", null, null);
                });

                Targets.RegisterObject<Action<NavigationPage<AboutPage>>>(p =>
                {
                    p.Title = "About";
                    p.Icon = Device.OnPlatform("tab_about.png", null, null);
                });
                Targets.RegisterType(typeof(PageBuilder<>));
                Targets.RegisterProjection<Page, NavigationPage>((r, t) => typeof(NavigationPage<>).MakeGenericType(t.DeclaredType));

                Container = new Container(Targets, ContainerConfig);
            }
            SetMainPage();
        }

        public static void SetMainPage()
        {
            TabbedPage tabbed = new TabbedPage();
            Current.MainPage = tabbed;
            tabbed.Children.Add(
                Container.ResolveMany<PageBuilder>()
                    .Select(pb => 
                    {
                        var p = pb.Build();
                        return new NavigationPage(p)
                        {
                            Title = p.Title,
                            Icon = p.Icon
                        };
                    })
                //{
                //    new NavigationPage(new ItemsPage())
                //    {
                //        Title = "Browse",
                //        Icon = Device.OnPlatform("tab_feed.png",null,null)
                //    },
                //    new NavigationPage(new AboutPage())
                //    {
                //        Title = "About",
                //        Icon = Device.OnPlatform("tab_about.png",null,null)
                //    },
                //}
            };
        }
    }
}
