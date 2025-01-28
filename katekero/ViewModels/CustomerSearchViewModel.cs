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
	public class CustomerSearchViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Customer> _customers;
        private int _saleNo;
        private int _customerId;


        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }
        public int SaleNo
        {
            get { return _saleNo; }
            set { SetProperty(ref _saleNo, value); }
        }
        public int CustomerId
        {
            get { return _customerId; }
            set { SetProperty(ref _customerId, value); }
        }

        public CustomerSearchViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            CancelCommand = new DelegateCommand(CancelCommandExecute);
            CustomerDoubleClickCommand = new DelegateCommand(CustomerDoubleClick);

            // 得意先マスタ
            using (var context = new AppDbContext())
            {
                Customers = new ObservableCollection<Customer>(context.Customers.ToList());
            }
        }
        public DelegateCommand CancelCommand { get; }

        public DelegateCommand CustomerDoubleClickCommand { get; }

        private void CancelCommandExecute()
        {
            var p = new NavigationParameters();
            p.Add(nameof(RegisterViewModel.SaleNo), this.SaleNo);
            p.Add(nameof(RegisterViewModel.CustomerId), 0);
            _regionManager.RequestNavigate("ContentRegion", nameof(Register), p);

        }
        private void CustomerDoubleClick()
        {
            var p = new NavigationParameters();
            p.Add(nameof(RegisterViewModel.SaleNo), this.SaleNo);
            p.Add(nameof(RegisterViewModel.CustomerId), CustomerId);
            _regionManager.RequestNavigate("ContentRegion", nameof(Register), p);

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.SaleNo = navigationContext.Parameters.GetValue<int>(nameof(SaleNo));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
