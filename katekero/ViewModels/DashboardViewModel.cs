using katekero.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace katekero.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            EditCommand = new DelegateCommand(EditCommandExecute);

        }
        public DelegateCommand EditCommand { get; }
        private void EditCommandExecute()
        {
            // 登録画面表示
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Register), p);

        }
    }
}
