using Rezolver;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XForms.Views;
using System.Collections;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace XForms
{
    // allows indexed gets on non-existent keys without throwing an exception
    public class PropertyDictionary : IDictionary<string, object>
    {
        private Dictionary<string, object> _inner;

        public PropertyDictionary(IEqualityComparer<string> comparer = null)
        {
            _inner = new Dictionary<string, object>(comparer ?? EqualityComparer<string>.Default);
        }

        public object this[string key] { get => _inner.TryGetValue(key, out var result) ? result : default(object); set => ((IDictionary<string, object>)_inner)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, object>)_inner).Keys;

        public ICollection<object> Values => ((IDictionary<string, object>)_inner).Values;

        public int Count => ((IDictionary<string, object>)_inner).Count;

        public bool IsReadOnly => ((IDictionary<string, object>)_inner).IsReadOnly;

        public void Add(string key, object value) => ((IDictionary<string, object>)_inner).Add(key, value);

        public void Add(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_inner).Add(item);

        public void Clear() => ((IDictionary<string, object>)_inner).Clear();

        public bool Contains(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_inner).Contains(item);

        public bool ContainsKey(string key) => ((IDictionary<string, object>)_inner).ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((IDictionary<string, object>)_inner).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => ((IDictionary<string, object>)_inner).GetEnumerator();

        public bool Remove(string key) => ((IDictionary<string, object>)_inner).Remove(key);

        public bool Remove(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_inner).Remove(item);

        public bool TryGetValue(string key, out object value) => ((IDictionary<string, object>)_inner).TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, object>)_inner).GetEnumerator();
    }
    public class PlatformSpecificProperties
    {
        private Dictionary<string, PropertyDictionary> _dictionary = new Dictionary<string, PropertyDictionary>();
        public PropertyDictionary this[string index]
        {
            get
            {
                if (!_dictionary.TryGetValue(index, out var toReturn))
                    _dictionary[index] = toReturn = new PropertyDictionary();

                return toReturn;
            }
        }
    }

    public class PageProperties<TPage>
    {
        public PropertyDictionary Properties { get; } = new PropertyDictionary();

        public PlatformSpecificProperties PerPlatformProperties { get; } = new PlatformSpecificProperties();
    }

    public class PageBuilder<TPage> where TPage : Page
    {
        public PageBuilder(TPage page, PageProperties<TPage> props)
        {
            Page.Title = props.Properties[nameof(Page.Title)];
            Page = page;
        }

        public Page Page { get; }
    }

    public partial class App : Application
    {
        public static CombinedTargetContainerConfig TargetContainerConfig { get; } =
            TargetContainer.DefaultConfig.Clone();

        public static CombinedContainerConfig ContainerConfig { get; } =
            Rezolver.Container.DefaultConfig.Clone();

        public static ITargetContainer Targets { get; } = new TargetContainer(TargetContainerConfig);

        public static IContainer Container { get; private set; }

        public App()
        {
            InitializeComponent();
            if (Container == null)
            {
                Targets.RegisterType<ItemsPage, Page>();
                Targets.RegisterType<AboutPage, Page>();

                Targets.RegisterDecorator((ItemsPage p, PlatformSpecificProperties<ItemsPage> props) =>
                {
                    p.Title = "Browse";
                    // p.Icon = 
                    return p;
                });

                Container = new Container(Targets, ContainerConfig);
            }
            SetMainPage();
        }

        public static void SetMainPage()
        {
            Current.MainPage = new TabbedPage
            {
                Children =
                {
                    new NavigationPage(new ItemsPage())
                    {
                        Title = "Browse",
                        Icon = Device.OnPlatform("tab_feed.png",null,null)
                    },
                    new NavigationPage(new AboutPage())
                    {
                        Title = "About",
                        Icon = Device.OnPlatform("tab_about.png",null,null)
                    },
                }
            };
        }
    }
}
