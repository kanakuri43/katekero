using katekero.Models;
using katekero.Views;
using Microsoft.Identity.Client;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;

namespace katekero.ViewModels
{
	public class RegisterViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private int _saleNo;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Product> _products;
        private ObservableCollection<ProductCategory> _productCategories;
        private ObservableCollection<Sale> _sales;
        private int _selectedProductCategoryId;
        private int _selectedProductId;
        private int _customerId;
        private string _customerName;

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }

        public int SaleNo
        {
            get { return _saleNo; }
            set { SetProperty(ref _saleNo, value); }
        }
        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set { SetProperty(ref _products, value); }
        }
        public ObservableCollection<ProductCategory> ProductCategories
        {
            get { return _productCategories; }
            set { SetProperty(ref _productCategories, value); }
        }
        public ObservableCollection<Sale> Sales
        {
            get { return _sales; }
            set { SetProperty(ref _sales, value); }
        }
        public int SelectedProductCategoryId
        {
            get { return _selectedProductCategoryId; }
            set { SetProperty(ref _selectedProductCategoryId, value); }
        }
        public int SelectedProductId
        {
            get { return _selectedProductId; }
            set { SetProperty(ref _selectedProductId, value); }
        }

        public int CustomerId
        {
            get { return _customerId; }
            set { SetProperty(ref _customerId, value); }
        }
        public string CustomerName
        {
            get { return _customerName; }
            set { SetProperty(ref _customerName, value); }
        }


        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            CancelCommand = new DelegateCommand(CancelCommandExecute);
            CustomerSearchCommand = new DelegateCommand(CustomerSearchCommandExecute);

            // 商品マスタ
            using (var context = new AppDbContext())
            {
                Products = new ObservableCollection<Product>(context.Products.ToList());
            }

            //商品分類マスタ
            using (var context = new AppDbContext())
            {
                ProductCategories = new ObservableCollection<ProductCategory>(context.ProductCategories.ToList());
            }

        }
        private void SaveCommandExecute()
        {
            using (var context = new AppDbContext())
            {
                if (this.SaleNo == 0)
                {
                    // Create
                    var sale = new Sale
                    {

                    };
                    context.Sales.Add(sale);


                    context.SaveChanges();
                }
            }
        }

        private void CancelCommandExecute()
        {
            // Home
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Dashboard), p);
        }

        private void CustomerSearchCommandExecute()
        {
            // 得意先検索
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(CustomerSearch), p);

        }

        private void ShowDetails(int SaleNo)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // TODO
            // Sale Noを受け取った時の処理
            this.SaleNo = navigationContext.Parameters.GetValue<int>(nameof(SaleNo));
            ShowDetails(SaleNo);
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
