namespace XForms.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new XForms.App());
        }
    }
}