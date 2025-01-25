using katekero.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace katekero.ViewModels
{
	public class RegisterViewModel : BindableBase
	{
        private readonly IRegionManager _regionManager;
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }
        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            CancelCommand = new DelegateCommand(CancelCommandExecute);
            CustomerSearchCommand = new DelegateCommand(CustomerSearchCommandExecute);

        }
        private void CancelCommandExecute()
        {
            // Menu表示
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Dashboard), p);

        }
        private void CustomerSearchCommandExecute()
        {
            // Menu表示
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(CustomerSearch), p);

        }
    }
}
