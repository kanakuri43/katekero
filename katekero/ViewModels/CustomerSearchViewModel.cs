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
	public class CustomerSearchViewModel : BindableBase
	{
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Customer> _customers;

        public DelegateCommand CancelCommand { get; }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }

        public CustomerSearchViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            CancelCommand = new DelegateCommand(CancelCommandExecute);

            // 得意先マスタ
            using (var context = new AppDbContext())
            {
                Customers = new ObservableCollection<Customer>(context.Customers.ToList());
            }
        }
        private void CancelCommandExecute()
        {
            // Menu表示
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Register), p);

        }

    }
}
