using katekero.Models;
using katekero.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace katekero.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private int _selectedSaleNo;

        public ObservableCollection<Sale> Sales
        {
            get { return _sales; }
            set { SetProperty(ref _sales, value); }
        }
        public int SelectedSaleNo
        {
            get { return _selectedSaleNo; }
            set { SetProperty(ref _selectedSaleNo, value); }
        }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SaleDoubleClickCommand = new DelegateCommand(SaleDoubleClick);
            RegisterCommand = new DelegateCommand(Register);

            using (var context = new AppDbContext())
            {
                Sales = new ObservableCollection<Sale>(context.Sales.ToList());
            }


        }
        public DelegateCommand RegisterCommand { get; }
        public DelegateCommand SaleDoubleClickCommand { get; }

        private void Register()
        {
            var p = new NavigationParameters();
            p.Add(nameof(RegisterViewModel.SaleNo), 0);
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);

        }
        private void SaleDoubleClick()
        {
            var p = new NavigationParameters();
            p.Add(nameof(RegisterViewModel.SaleNo), _selectedSaleNo);
            p.Add(nameof(RegisterViewModel.CustomerId), 0);
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);
        }
    }
}
