using sale.Models;
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
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Windows;

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
        private string _selectedProductCategoryCode;
        private int _selectedProductId;
        private int _customerId;
        private string _customerCode;
        private string _customerName;
        private string _productSearchText;
        private ICollectionView _filteredProducts;
        private bool _canHeaderEdit;
        private int _primaryTaxRate;
        private int _secondaryTaxRate;

        public DelegateCommand SaveSalesCommand { get; }
        public DelegateCommand DeleteSalesCommand { get; }
        public DelegateCommand PrintSalesSlipCommand { get; }
        public DelegateCommand BackToHomeCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }
        public DelegateCommand ProductDoubleClickCommand { get; }
        public DelegateCommand<Sale> DeleteSaleCommand { get; }
        public DelegateCommand FilterProductsByCategoryCommand { get; }
        public int SaleId
        {
            get { return _saleId; }
            set { SetProperty(ref _saleId, value); }
        }
        public DateTime SaleDate
        {
            get { return _saleDate; }
            set
            {
                if (SetProperty(ref _saleDate, value))
                {
                    UpdateTaxRates();
                }
            }
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
                if (_sales != value)
                {
                    if (_sales != null)
                    {
                        _sales.CollectionChanged -= OnSalesCollectionChanged;
                    }
                    _sales = value;
                    if (_sales != null)
                    {
                        _sales.CollectionChanged += OnSalesCollectionChanged;
                    }
                    RaisePropertyChanged(nameof(Sales));
                    RaisePropertyChanged(nameof(Subtotal));
                    RaisePropertyChanged(nameof(TaxPrice));
                    RaisePropertyChanged(nameof(TotalAmount));
                }
            }
        }
        public string SelectedProductCategoryCode
        {
            get { return _selectedProductCategoryCode; }
            set
            {
                if (SetProperty(ref _selectedProductCategoryCode, value))
                {
                    FilterProductsByCategory();
                }
            }
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
        public string CustomerCode
        {
            get { return _customerCode; }
            set { SetProperty(ref _customerCode, value); }
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
        public int Subtotal => Sales.Where(s => s.SaleNo == this.SaleNo).Sum(s => s.Amount);

        public int TaxPrice => (int)(Subtotal * (PrimaryTaxRate * 0.01));

        public int TotalAmount => Subtotal + TaxPrice;

        public string ProductSearchText
        {
            get { return _productSearchText; }
            set
            {
                if (SetProperty(ref _productSearchText, value))
                {
                    FilterProductsByCategory();
                }
            }
        }
        public ICollectionView FilteredProducts
        {
            get { return _filteredProducts; }
        }

        public int PrimaryTaxRate
        {
            get { return _primaryTaxRate; }
            set { SetProperty(ref _primaryTaxRate, value); }
        }
        public int SecondaryTaxRate
        {
            get { return _secondaryTaxRate; }
            set { SetProperty(ref _secondaryTaxRate, value); }
        }

        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SaveSalesCommand = new DelegateCommand(SaveSales);
            DeleteSalesCommand = new DelegateCommand(DeleteSales);
            PrintSalesSlipCommand = new DelegateCommand(PrintSalesSlip);
            BackToHomeCommand = new DelegateCommand(BackToHome);
            CustomerSearchCommand = new DelegateCommand(CustomerSearch);
            ProductDoubleClickCommand = new DelegateCommand(ProductDoubleClick);
            DeleteSaleCommand = new DelegateCommand<Sale>(DeleteSale);
            FilterProductsByCategoryCommand = new DelegateCommand(FilterProductsByCategory);

            // Salesの初期化
            Sales = new ObservableCollection<Sale>();
            Sales.CollectionChanged += OnSalesCollectionChanged;

            this.SaleNo = 0;
            this.SaleDate = DateTime.Now;


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
                var sortedCategories = context.ProductCategories
                                              .OrderBy(pc => pc.Code) 
                                              .ToList();
                ProductCategories = new ObservableCollection<ProductCategory>(sortedCategories);
            }
            SelectedProductCategoryCode = "0";

            // 税率を取得
            UpdateTaxRates();
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
                        ProductCode = product.Code,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Amount = product.Price
                    };
                    Sales.Add(sale);
                }
            }
        }

        private async void SaveSales()
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
                        sale.CustomerCode = this.CustomerCode;
                        sale.CreatedAt = dt;
                        sale.UpdatedAt = dt;
                        context.Sales.Add(sale);
                    }
                }
                else
                {

                }

                context.SaveChanges();

                // カスタムダイアログを表示
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                if (metroWindow == null)
                {
                    return;
                }
                var dialogSettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = "はい",
                    NegativeButtonText = "いいえ",
                    AnimateShow = false,
                    AnimateHide = false
                };
                var result = await metroWindow.ShowMessageAsync("登録しました", "納品書を印刷しますか？", MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
                if (result == MessageDialogResult.Affirmative)
                {
                    PrintSalesSlip();
                }
            }
        }
        private void DeleteSales()
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

        private void PrintSalesSlip()
        {
            var printDialog = new System.Windows.Controls.PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // 印刷する内容を作成
                var salesSlipView = new Views.SalesSlip();
                var salesSlipViewModel = new SalesSlipViewModel(new ObservableCollection<Sale>(this.Sales));

                // Titleを設定
                salesSlipViewModel.SetUpperTitle("納品書 (控)"); 
                salesSlipViewModel.SetMiddleTitle("納品書"); 
                salesSlipViewModel.SetLowerTitle("受領書"); 

                salesSlipView.DataContext = salesSlipViewModel;

                // 印刷設定
                salesSlipView.Measure(new System.Windows.Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                salesSlipView.Arrange(new System.Windows.Rect(new System.Windows.Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight)));

                // 印刷実行
                printDialog.PrintVisual(salesSlipView, "Sales Slip");
            }
        }
        private void BackToHome()
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

        private void FilterProductsByCategory()
        {
            _filteredProducts.Filter = item =>
            {
                if (item is Product product)
                {
                    bool matchesCategory = SelectedProductCategoryCode == "0" || product.CategoryCode == SelectedProductCategoryCode;
                    bool matchesSearchText = string.IsNullOrEmpty(ProductSearchText) || product.Name.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase);
                    return matchesCategory && matchesSearchText;
                }
                return false;
            };
            _filteredProducts.Refresh();
        }

        private void UpdateTaxRates()
        {
            using (var context = new AppDbContext())
            {
                var taxRateHistory = context.TaxRateHistories
                    .Where(trh => trh.StartedDate <= SaleDate)
                    .OrderByDescending(trh => trh.StartedDate)
                    .FirstOrDefault();

                if (taxRateHistory != null)
                {
                    PrimaryTaxRate = taxRateHistory.PrimaryTaxRate;     // 消費税率
                    SecondaryTaxRate = taxRateHistory.SecondaryTaxRate; // 軽減税率
                }
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

            var sales = navigationContext.Parameters.GetValue<ObservableCollection<Sale>>(nameof(Sales));
            if (sales == null)
            {
                // 新期

                this.CustomerId = navigationContext.Parameters.GetValue<int>(nameof(CustomerId));

                // CustomersコレクションからCustomerIdに一致するCustomerを取得
                var customer = Customers.FirstOrDefault(c => c.Id == this.CustomerId);
                if (customer != null)
                {
                    this.CustomerCode = customer.Code;
                    this.CustomerName = customer.Name;
                }

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
                    this.CustomerCode = sale.CustomerCode;
                    this.CustomerName = sale.CustomerName;
                }

                // プロパティの変更を通知
                RaisePropertyChanged(nameof(Subtotal));
                RaisePropertyChanged(nameof(TaxPrice));
                RaisePropertyChanged(nameof(TotalAmount));
                
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
