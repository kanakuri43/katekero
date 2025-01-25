using katekero.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace katekero.ViewModels
{
	public class CustomerSearchViewModel : BindableBase
	{
        private readonly IRegionManager _regionManager;
        public DelegateCommand CancelCommand { get; }
        public CustomerSearchViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            CancelCommand = new DelegateCommand(CancelCommandExecute);

        }
        private void CancelCommandExecute()
        {
            // Menu表示
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Register), p);

        }

    }
}
