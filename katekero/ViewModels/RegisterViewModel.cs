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
	public class RegisterViewModel : BindableBase
	{
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Product> _products;
        private ObservableCollection<ProductCategory> _productCategories;
        private int _selectedProductCategoryId;
        private int _selectedProductId;

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }

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
    }
}
