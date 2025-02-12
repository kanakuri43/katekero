﻿using Prism.Mvvm;
using Prism.Regions;
using aggregate.Views;

namespace aggregate.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "KATEKERO";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(Dashboard));

        }
    }
}
