using sale.Views;
using Prism.Ioc;
using System.Windows;

namespace sale
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
