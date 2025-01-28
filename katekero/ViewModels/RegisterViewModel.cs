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
        private int _saleId;
        private int _saleNo;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Product> _products;
        private ObservableCollection<ProductCategory> _productCategories;
        private ObservableCollection<Sale> _sales;
        private int _selectedProductCategoryId;
        private int _selectedProductId;
        private int _customerId;
        private string _customerName;
        private int _totalAmount;
        private int _includedTaxPrice;

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }
        public DelegateCommand<Product> AddSaleDetailCommand { get; }

        public int SaleId
        {
            get { return _saleId; }
            set { SetProperty(ref _saleId, value); }
        }
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
        public int TotalAmount
        {
            get { return _totalAmount; }
            set { SetProperty(ref _totalAmount, value); }
        }
        public int IncludedTaxPrice
        {
            get { return _includedTaxPrice; }
            set { SetProperty(ref _includedTaxPrice, value); }
        }


        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            CustomerSearchCommand = new DelegateCommand(CustomerSearch);
            AddSaleDetailCommand = new DelegateCommand<Product>(AddSaleDetail);

            // Salesの初期化
            Sales = new ObservableCollection<Sale>();

            // 商品マスタ
            using (var context = new AppDbContext())
            {
                Products = new ObservableCollection<Product>(context.Products.ToList());
            }            
            // 得意先マスタ
            using (var context = new AppDbContext())
            {
                Customers = new ObservableCollection<Customer>(context.Customers.ToList());
            }

            //商品分類マスタ
            using (var context = new AppDbContext())
            {
                ProductCategories = new ObservableCollection<ProductCategory>(context.ProductCategories.ToList());
            }

            TotalAmount = 1234;
            IncludedTaxPrice = 123;

        }

        private void AddSaleDetail(Product product)
        {
            if (product != null)
            {
                var sale = new Sale
                {
                    State = 0,
                    SaleNo = this.SaleNo,
                    LineNo = 1,
                    CustomerId = this.CustomerId,
                    CustomerName = this.CustomerName,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1, // 初期数量を1とする
                    Price = product.Price,
                    Amount = product.Price // 初期数量が1なので価格と同じ
                };
                Sales.Add(sale);
            }
        }

        private void Save()
        {
            using (var context = new AppDbContext())
            {
                var dt = DateTime.Now;
                if (this.SaleNo == 0)
                {
                    foreach (var sale in _sales)
                    {
                        // 新規データとして追加
                        sale.CreatedAt = dt;
                        sale.UpdatedAt = dt;
                        context.Sales.Add(sale);
                    }
                }
                else
                {
                    foreach (var sale in _sales)
                    {
                        // 既存データを更新
                        var existingSale = context.Sales.FirstOrDefault(s => s.SaleNo == sale.SaleNo);
                        if (existingSale != null)
                        {
                            existingSale.CustomerId = sale.CustomerId;
                            existingSale.CustomerName = sale.CustomerName;
                            existingSale.ProductId = sale.ProductId;
                            existingSale.ProductName = sale.ProductName;
                            existingSale.Quantity = sale.Quantity;
                            existingSale.Price = sale.Price;
                            existingSale.Amount = sale.Amount;
                            existingSale.UpdatedAt = dt;
                        }
                    }
                }

                context.SaveChanges();
            }
        }

        private void Cancel()
        {
            // Home
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Dashboard), p);
        }

        private void CustomerSearch()
        {
            // 得意先検索
            var p = new NavigationParameters();
            p.Add(nameof(CustomerSearchViewModel.SaleNo), this.SaleNo);
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.CustomerSearch), p);

        }

        private void ShowDetails(int SaleNo)
        {
            using var context = new AppDbContext();
            var s = new ObservableCollection<Sale>(context.Sales
                                                          .Where(j => j.SaleNo == SaleNo)
                                                          .ToList());
            this.Sales = s;

            if (SaleNo == 0)
            {
                if (this.CustomerId == 0)
                {
                    // 新規登録時

                }
                else
                {
                    // 新規登録時に得意先検索して、選んで戻ってきた
                    // CustomerIdでCustomersを検索
                    var customer = this.Customers.FirstOrDefault(c => c.Id == this.CustomerId);
                    this.CustomerName = customer.Name;
                }
            }
            else
            {
                if (this.CustomerId == 0)
                {
                    //既存売上呼び出しで、得意先を選ばないで帰ってきた
                    // SalesのCustomerNameを表示
                    this.CustomerName = this.Sales.Select(sale => sale.CustomerName).FirstOrDefault();
                }
                else 
                {
                    // SalesのCustomerNameを表示
                    this.CustomerName = this.Sales.Select(sale => sale.CustomerName).FirstOrDefault();
                }
            }

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

            this.SaleNo = navigationContext.Parameters.GetValue<int>(nameof(SaleNo));
            this.CustomerId = navigationContext.Parameters.GetValue<int>(nameof(CustomerId));
            ShowDetails(this.SaleNo);
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
