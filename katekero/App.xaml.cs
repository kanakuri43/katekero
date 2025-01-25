using System.Windows;
using katekero.Views;
using Prism.Ioc;

namespace katekero
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Dashboard>();
            containerRegistry.RegisterForNavigation<Register>();
            containerRegistry.RegisterForNavigation<CustomerSearch>();
        }
    }
}
