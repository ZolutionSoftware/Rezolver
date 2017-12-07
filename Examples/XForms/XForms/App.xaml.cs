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
                Targets.RegisterType<ItemsPage>(mb => mb
                    .Bind(p => p.Title).ToObject("Browse")
                    .Bind(p => p.Icon).ToObject(Device.OnPlatform("tab_feed.png", null, null)));

                Targets.RegisterType<AboutPage>(mb => mb
                    .Bind(p => p.Title).ToObject("About")
                    .Bind(p => p.Icon).ToObject(Device.OnPlatform("tab_about.png", null, null)));

                Targets.RegisterType<TabbedPage>(mb => mb
                    .Bind(p => p.Children).AsCollection(typeof(ItemsPage), typeof(AboutPage)));
                
                Container = new Container(Targets, ContainerConfig);
            }
            SetMainPage();
        }

        public static void SetMainPage()
        {
            Current.MainPage = Container.Resolve<TabbedPage>();
        }
    }
}
