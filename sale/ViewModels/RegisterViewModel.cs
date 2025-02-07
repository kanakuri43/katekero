﻿using sale.Models;
using sale.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;

namespace sale.ViewModels
{
    public class RegisterViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private DateTime _saleDate;
        private int _saleId;
        private int _saleNo;
        private ObservableCollection<katekero.Models.Customer> _customers;
        private ObservableCollection<Product> _products;
        private ObservableCollection<ProductCategory> _productCategories;
        private ObservableCollection<Sale> _sales;
        private int _selectedProductCategoryId;
        private int _selectedProductId;
        private int _customerId;
        private string _customerName;
        private int _includedTaxPrice;
        private int _subTotal;
        private int _taxPrice;
        private int _totalAmount;
        private string _productSearchText;
        private ICollectionView _filteredProducts;
        private bool _canHeaderEdit;

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand PrintCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }
        //public DelegateCommand<Product> AddSaleDetailCommand { get; }
        public DelegateCommand ProductDoubleClickCommand { get; }
        public DelegateCommand<Sale> DeleteSaleCommand { get; }

        public int SaleId
        {
            get { return _saleId; }
            set { SetProperty(ref _saleId, value); }
        }
        public DateTime SaleDate
        {
            get { return _saleDate; }
            set { SetProperty(ref _saleDate, value); }
        }
        public int SaleNo
        {
            get { return _saleNo; }
            set { SetProperty(ref _saleNo, value); }
        }
        public ObservableCollection<katekero.Models.Customer> Customers
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
            set
            {
                if (SetProperty(ref _sales, value))
                {
                    _sales.CollectionChanged += OnSalesCollectionChanged;
                    RaisePropertyChanged(nameof(Subtotal));
                    RaisePropertyChanged(nameof(TaxPrice));
                    RaisePropertyChanged(nameof(TotalAmount));
                }
            }
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
        public bool CanHeaderEdit
        {
            get { return _canHeaderEdit; }
            set { SetProperty(ref _canHeaderEdit, value); }
        }
        public int Subtotal => Sales.Sum(s => s.Amount);

        public int TaxPrice => (int)(Subtotal * 0.1);

        public int TotalAmount => Subtotal + TaxPrice;

        public string ProductSearchText
        {
            get { return _productSearchText; }
            set
            {
                SetProperty(ref _productSearchText, value);
                FilterProducts();
            }
        }
        public ICollectionView FilteredProducts
        {
            get { return _filteredProducts; }
        }

        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SaveCommand = new DelegateCommand(Save);
            DeleteCommand = new DelegateCommand(Delete);
            PrintCommand = new DelegateCommand(Print);
            CancelCommand = new DelegateCommand(Home);
            CustomerSearchCommand = new DelegateCommand(CustomerSearch);
            ProductDoubleClickCommand = new DelegateCommand(ProductDoubleClick);
            DeleteSaleCommand = new DelegateCommand<Sale>(DeleteSale);

            // Salesの初期化
            Sales = new ObservableCollection<Sale>();
            Sales.CollectionChanged += OnSalesCollectionChanged;

            // 商品マスタ
            using (var context = new AppDbContext())
            {
                Products = new ObservableCollection<Product>(context.Products.ToList());
            }
            _filteredProducts = CollectionViewSource.GetDefaultView(Products);
            _filteredProducts.Filter = FilterProductByName;
            
            // 得意先マスタ
            using (var context = new AppDbContext())
            {
                Customers = new ObservableCollection<katekero.Models.Customer>(context.Customers.ToList());
            }

            //商品分類マスタ
            using (var context = new AppDbContext())
            {
                ProductCategories = new ObservableCollection<ProductCategory>(context.ProductCategories.ToList());
            }

        }

        private void OnSalesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Subtotal));
            RaisePropertyChanged(nameof(TaxPrice));
            RaisePropertyChanged(nameof(TotalAmount));
        }

        private void FilterProducts()
        {
            _filteredProducts.Refresh();
        }

        private bool FilterProductByName(object item)
        {
            if (item is Product product)
            {
                return string.IsNullOrEmpty(ProductSearchText) || product.Name.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private void AddSaleDetail()
        {
            using (var context = new AppDbContext())
            {
                var product = context.Products.FirstOrDefault(p => p.Id == _selectedProductId);

                var existingSale = Sales.FirstOrDefault(s => s.ProductId == product.Id);

                if (existingSale != null)
                {
                    existingSale.Quantity += 1;
                    existingSale.Amount = existingSale.Quantity * product.Price;
                    RaisePropertyChanged(nameof(Subtotal));
                    RaisePropertyChanged(nameof(TaxPrice));
                    RaisePropertyChanged(nameof(TotalAmount));
                }
                else
                {
                    var sale = new Sale
                    {
                        State = 0,
                        SaleNo = this.SaleNo,
                        CustomerId = this.CustomerId,
                        CustomerName = this.CustomerName,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Amount = product.Price
                    };
                    Sales.Add(sale);
                }
            }
        }

        private void Save()
        {
            using (var context = new AppDbContext())
            {
                var dt = DateTime.Now;
                if (this.SaleNo == 0)
                {
                    // 売上番号設定
                    int maxSaleNo = context.Sales.Max(s => (int?)s.SaleNo) ?? 0;
                    int newSaleNo = maxSaleNo + 1;

                    var lineNo = 0;
                    foreach (var sale in _sales)
                    {
                        lineNo++;

                        sale.SaleDate = this.SaleDate;
                        sale.SaleNo = newSaleNo; 
                        sale.LineNo = lineNo;
                        sale.CreatedAt = dt;
                        sale.UpdatedAt = dt;
                        context.Sales.Add(sale);
                    }
                }
                else
                {

                }

                context.SaveChanges();
            }
        }
        private void Delete()
        {
            using (var context = new AppDbContext())
            {
                // 選択されているSaleNoに基づいて削除
                var salesToDelete = context.Sales.Where(s => s.SaleNo == this.SaleNo).ToList();
                if (salesToDelete.Any())
                {
                    context.Sales.RemoveRange(salesToDelete);
                    context.SaveChanges();
                }
            }

        }

        private void Print()
        {
            var printDialog = new System.Windows.Controls.PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // 印刷する内容を作成
                var salesSlipView = new Views.SalesSlip();
                var salesSlipViewModel = new SalesSlipViewModel(new ObservableCollection<Sale>(this.Sales));

                // ここでTitleを設定
                salesSlipViewModel.SetTitle("納品書"); 

                salesSlipView.DataContext = salesSlipViewModel;

                // 印刷設定
                salesSlipView.Measure(new System.Windows.Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                salesSlipView.Arrange(new System.Windows.Rect(new System.Windows.Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight)));

                // 印刷実行
                printDialog.PrintVisual(salesSlipView, "Sales Slip");
            }
        }
        private void Home()
        {
            this.Sales.Clear();

            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Dashboard), p);
        }

        private void DeleteSale(Sale sale)
        {
            if (sale != null)
            {
                Sales.Remove(sale);
            }
        }
        private void CustomerSearch()
        {
            var p = new NavigationParameters();
            p.Add(nameof(CustomerSearchViewModel.Sales), new ObservableCollection<Sale>(this.Sales));
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.CustomerSearch), p);       

        }
        private void ProductDoubleClick()
        {
            AddSaleDetail();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.CustomerId = navigationContext.Parameters.GetValue<int>(nameof(CustomerId));
            this.CustomerName = navigationContext.Parameters.GetValue<string>(nameof(CustomerName));

            var sales = navigationContext.Parameters.GetValue<ObservableCollection<Sale>>(nameof(Sales));
            if (sales == null)
            {
                // 新期

                this.SaleDate = DateTime.Now;
                this.SaleNo = 0;

                this.CanHeaderEdit = true;  // 日付・得意先 変更可
            }
            else
            {
                // 編集

                this.Sales = sales;
                if (sales.Any())
                {
                    var sale = sales.First();
                    this.SaleNo = sale.SaleNo;
                    this.SaleDate = sale.SaleDate;

                    this.CustomerId = sale.CustomerId;
                    this.CustomerName = sale.CustomerName;

                }

                this.CanHeaderEdit = false; // 日付・得意先 変更不可
            }

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
