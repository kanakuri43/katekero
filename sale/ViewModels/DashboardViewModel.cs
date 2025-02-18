using sale.Models;
using sale.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Timers;
using katekero.Models;
using System.Windows.Documents;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Controls.Primitives;

namespace sale.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private ObservableCollection<Order> _fetchedOrders;
        private ObservableCollection<Customer> _fetchedCustomers;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Product> _fetchedProducts;
        private int _selectedSaleNo;
        private int _selectedOrderNo;
        private DateTime _selectedDate;
        private string _json;
        private DateTime _lastFetchedAt;
        private bool _isProgressRingActive;
        private readonly Timer _timer;

        public ObservableCollection<Product> FetchedProducts
        {
            get { return _fetchedProducts; }
            set { SetProperty(ref _fetchedProducts, value); }
        }
        public ObservableCollection<Customer> FetchedCustomers
        {
            get { return _fetchedCustomers; }
            set { SetProperty(ref _fetchedCustomers, value); }
        }
        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }
        public ObservableCollection<Order> FetchedOrders
        {
            get { return _fetchedOrders; }
            set { SetProperty(ref _fetchedOrders, value); }
        }
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
        public int SelectedOrderNo
        {
            get { return _selectedOrderNo; }
            set { SetProperty(ref _selectedOrderNo, value); }
        }
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }
        public string Json
        {
            get { return _json; }
            set { SetProperty(ref _json, value); }
        }
        public DateTime LastFetchedAt
        {
            get { return _lastFetchedAt; }
            set { SetProperty(ref _lastFetchedAt, value); }
        }
        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set { SetProperty(ref _isProgressRingActive, value); }
        }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SaleDoubleClickCommand = new DelegateCommand(SaleDoubleClick);
            OrderDoubleClickCommand = new DelegateCommand(OrderDoubleClick);
            SelectedDateChangedCommand = new DelegateCommand(SelectedDateChanged);
            RegisterCommand = new DelegateCommand(Register);
            ForwardCommand = new DelegateCommand(ForwardDate);
            BackwardCommand = new DelegateCommand(BackwardDate);
            ManualReloadCommand = new DelegateCommand(ReloadOrders);

            using (var context = new AppDbContext())
            {
                Sales = new ObservableCollection<Sale>(context.Sales.ToList());
                Customers = new ObservableCollection<Customer>(context.Customers.ToList());
            }

            IsProgressRingActive = true;

            ReloadOrders();
            // Timerの設定 (15秒ごと)
            _timer = new Timer(15000);
            _timer.Elapsed += (sender, e) => ReloadOrders();
            _timer.Start();

            SelectedDate = DateTime.Now;
            ShowSalesList();

            _ = InitializeAsync();

            IsProgressRingActive = false;

        }


        public DelegateCommand RegisterCommand { get; }
        public DelegateCommand SaleDoubleClickCommand { get; }
        public DelegateCommand OrderDoubleClickCommand { get; }
        public DelegateCommand SelectedDateChangedCommand { get; }
        public DelegateCommand ForwardCommand { get; }
        public DelegateCommand BackwardCommand { get; }
        public DelegateCommand ManualReloadCommand { get; }

        private async Task InitializeAsync()
        {
            // kintone から customers 取込
            await FetchKintoneCustomersAsync();
            UpdateCustomersByKintoneMaster();

            // kintone から products 取込
            await FetchKintoneProductsAsync();
            UpdateProductsByKintoneMaster();
        }
        private async Task FetchKintoneCustomersAsync()
        {
            try
            {
                HttpClient client = new HttpClient();

                // kintoneのAPIトークン
                string apiToken = "DACJDxy3G7iLkvL0bbEIgUUnS4LdNJo8HcSNiW5Q";

                // ヘッダーにAPIトークンを設定
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Cybozu-API-Token", apiToken);

                string url = $"https://vk5k755s9nir.cybozu.com/k/v1/records.json?app=204";
                HttpResponseMessage response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                // JSONデータをOrderオブジェクトに変換
                var jsonObject = JObject.Parse(responseBody);
                var records = jsonObject["records"].Select(record => new Customer
                {
                    Id = int.Parse((string)record["$id"]?["value"]),
                    State = 0,
                    Code = (string)record["code"]?["value"],
                    Name = (string)record["name"]?["value"],
                    ZipCode = (string)record["zip_code"]?["value"],
                    Address = (string)record["address"]?["value"],
                    InvoiceClosingDay = int.Parse((string)record["invoice_closing_day"]?["value"])
                }).ToList();

                FetchedCustomers = new ObservableCollection<Customer>(records);
            }
            catch (Exception ex)
            {
                // エラーハンドリング
            }
        }
        private async Task FetchKintoneProductsAsync()
        {
            try
            {
                HttpClient client = new HttpClient();

                // kintoneのAPIトークン
                string apiToken = "ovg4UNJ4DHalpTrDxSlSpuOpSOWnesdH6FUztYPd";

                // ヘッダーにAPIトークンを設定
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Cybozu-API-Token", apiToken);

                string url = $"https://vk5k755s9nir.cybozu.com/k/v1/records.json?app=205";
                HttpResponseMessage response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                // JSONデータをOrderオブジェクトに変換
                var jsonObject = JObject.Parse(responseBody);
                var records = jsonObject["records"].Select(record => new Product
                {
                    Id = int.Parse((string)record["$id"]?["value"]),
                    State = 0,
                    Code = (string)record["code"]?["value"],
                    Name = (string)record["name"]?["value"],
                    Price = int.Parse((string)record["price"]["value"]),
                    CategoryCode = (string)record["category_code"]?["value"],
                }).ToList();

                FetchedProducts = new ObservableCollection<Product>(records);
            }
            catch (Exception ex)
            {
                // エラーハンドリング
            }
        }

        private void Register()
        {
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);

        }
        private void ForwardDate()
        {
            this.SelectedDate = this.SelectedDate.AddDays(1);
            ShowSalesList();
        }
        private void BackwardDate()
        {
            this.SelectedDate = this.SelectedDate.AddDays(-1);
            ShowSalesList();
        }

        private void ReloadOrders()
        {
            _ = FetchKintoneOrders(new string[] { });
            LastFetchedAt = DateTime.Now;
        }

        private void UpdateCustomersByKintoneMaster()
        {
            if (FetchedCustomers == null || !FetchedCustomers.Any())
            {
                // FetchedCustomersがnullまたは空の場合、処理を中断
                return;
            }

            using (var context = new AppDbContext())
            {
                var dt = DateTime.Now;

                foreach (var fetchedCustomer in FetchedCustomers)
                {
                    var existingCustomer = context.Customers
                        .FirstOrDefault(c => c.Code == fetchedCustomer.Code);

                    if (existingCustomer == null)
                    {
                        // 新しい顧客を挿入
                        var newCustomer = new Customer
                        {
                            State = fetchedCustomer.State,
                            Code = fetchedCustomer.Code,
                            Name = fetchedCustomer.Name,
                            ZipCode = fetchedCustomer.ZipCode,
                            Address = fetchedCustomer.Address,
                            InvoiceClosingDay = fetchedCustomer.InvoiceClosingDay,
                            CreatedAt = dt,
                            UpdatedAt = dt
                        };
                        context.Customers.Add(newCustomer);
                    }
                    else
                    {
                        // 既存の顧客を更新
                        existingCustomer.State = fetchedCustomer.State;
                        existingCustomer.Name = fetchedCustomer.Name;
                        existingCustomer.ZipCode = fetchedCustomer.ZipCode;
                        existingCustomer.Address = fetchedCustomer.Address;
                        existingCustomer.InvoiceClosingDay = fetchedCustomer.InvoiceClosingDay;
                        existingCustomer.UpdatedAt = dt;
                    }

                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    // エラーハンドリング
                    Console.WriteLine($"Error saving changes: {ex.Message}");
                }
            }
        }
        private void UpdateProductsByKintoneMaster()
        {
            if (FetchedProducts == null || !FetchedProducts.Any())
            {
                // FetchedProductsがnullまたは空の場合、処理を中断
                return;
            }

            using (var context = new AppDbContext())
            {
                var dt = DateTime.Now;

                foreach (var fetchedProducts in FetchedProducts)
                {
                    var existingProduct = context.Products
                        .FirstOrDefault(c => c.Code == fetchedProducts.Code);

                    if (existingProduct == null)
                    {
                        // 新しい顧客を挿入
                        var newProduct = new Product
                        {
                            State = fetchedProducts.State,
                            Code = fetchedProducts.Code,
                            Name = fetchedProducts.Name,
                            Price = fetchedProducts.Price,
                            CategoryCode = fetchedProducts.CategoryCode,
                            CreatedAt = dt,
                            UpdatedAt = dt
                        };
                        context.Products.Add(newProduct);
                    }
                    else
                    {
                        // 既存の顧客を更新
                        existingProduct.State = fetchedProducts.State;
                        existingProduct.Name = fetchedProducts.Name;
                        existingProduct.Price = fetchedProducts.Price;
                        existingProduct.CategoryCode = fetchedProducts.CategoryCode;
                        existingProduct.UpdatedAt = dt;
                    }

                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    // エラーハンドリング
                    Console.WriteLine($"Error saving changes: {ex.Message}");
                }
            }
        }


        private async Task FetchKintoneOrders(string[] args)
        {
            try
            {
                // 取得前の注文リストをコピー
                var previousOrders = FetchedOrders?.ToList() ?? new List<Order>();

                HttpClient client = new HttpClient();

                string apiToken = "clYsh32YLyQryGcHH8t4F8hkyIhOuEL67YBuyuvU";

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Cybozu-API-Token", apiToken);

                string url = $"https://vk5k755s9nir.cybozu.com/k/v1/records.json?app=203";

                HttpResponseMessage response = await client.GetAsync(url);

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonObject = JObject.Parse(responseBody);
                var records = jsonObject["records"].Select(record => new Order
                {
                    Id = int.Parse((string)record["$id"]["value"]),
                    OrderNo = int.Parse((string)record["order_no"]["value"]),
                    OrderDate = DateTime.Parse((string)record["order_date"]["value"]),
                    CustomerCode = (string)record["customer_code"]["value"],
                    CustomerName = (string)record["customer_name"]["value"],
                    ProductCode = (string)record["product_code"]["value"],
                    ProductName = (string)record["product_name"]["value"],
                    Quantity = int.Parse((string)record["qty"]["value"]),
                    Price = int.Parse((string)record["price"]["value"]),
                    Sold = int.TryParse((string)record["sold"]?["value"], out int soldValue) ? soldValue : 0
                }).Where(order => order.Sold != 1).ToList();

                FetchedOrders = new ObservableCollection<Order>(records.OrderBy(o => o.OrderDate));

                // 新しい注文を検出
                var newOrders = FetchedOrders.Where(o => !previousOrders.Any(po => po.OrderNo == o.OrderNo)).ToList();

                if ((previousOrders.Count != 0) && (newOrders.Any()))
                {
                    var newOrderDetails = string.Join(", ", newOrders.Select(o => $"OrderNo: {o.OrderNo}, Customer: {o.CustomerName}"));

                    new ToastContentBuilder()
                        .AddText("カテケロ")
                        .AddText($"{newOrders.Count} 件の新着の受注があります")
                        .AddAttributionText(newOrderDetails)
                        .Show();
                }
            }
            catch (Exception ex)
            {
                // エラーハンドリング
            }
        }
        private void SaleDoubleClick()
        {
            var selectedSales = Sales.Where(s => s.SaleNo == SelectedSaleNo).ToList();
            if (selectedSales.Any())
            {
                var p = new NavigationParameters();
                p.Add(nameof(RegisterViewModel.Sales), new ObservableCollection<Sale>(selectedSales));
                _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);
            }
        }

        private void OrderDoubleClick()
        {
            var selectedOrder = FetchedOrders.FirstOrDefault(o => o.OrderNo == SelectedOrderNo);
            if (selectedOrder != null)
            {
                var customer = Customers.FirstOrDefault(c => c.Code == selectedOrder.CustomerCode);
                var sale = new Sale
                {
                    SaleNo = 0, 
                    SaleDate = DateTime.Now,
                    CustomerCode = selectedOrder.CustomerCode, 
                    CustomerName = selectedOrder.CustomerName,
                    CustomerZipCode = customer?.ZipCode ?? "",
                    CustomerAddress = customer?.Address ?? "",
                    ProductCode = selectedOrder.ProductCode, 
                    ProductName = selectedOrder.ProductName,
                    Quantity = selectedOrder.Quantity,
                    Price = selectedOrder.Price,
                    Amount = selectedOrder.Quantity * selectedOrder.Price
                };

                var p = new NavigationParameters();
                p.Add(nameof(RegisterViewModel.Sales), new ObservableCollection<Sale> { sale });
                _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);
            }
        }

        private void SelectedDateChanged()
        {
            ShowSalesList();
        }

        private void ShowSalesList()
        {
            using var context = new AppDbContext();

            // 条件に基づいてデータをフィルタリング
            this.Sales = new ObservableCollection<Sale>(
                context.Sales
                    .Where(s => s.SaleDate.Date == this.SelectedDate.Date)
                    .OrderBy(s => s.SaleNo)
                    .ThenBy(s => s.LineNo)
            );

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SelectedDate = DateTime.Now;
            ShowSalesList();
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
