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

namespace sale.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private ObservableCollection<Order> _fetchedOrders;
        private ObservableCollection<Customer> _fetchedCustomers;
        private ObservableCollection<Customer> _customers;
        private int _selectedSaleNo;
        private int _selectedOrderNo;
        private DateTime _selectedDate;
        private string _json;
        private DateTime _lastFetchedAt;
        private bool _isProgressRingActive;
        private readonly Timer _timer;

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
            ManualReloadCommand = new DelegateCommand(Reload);

            using (var context = new AppDbContext())
            {
                Sales = new ObservableCollection<Sale>(context.Sales.ToList());
                Customers = new ObservableCollection<Customer>(context.Customers.ToList());
            }

            Reload();
            // Timerの設定 (1分ごと)
            _timer = new Timer(60000);
            _timer.Elapsed += (sender, e) => Reload();
            _timer.Start();

            //UpdateCustomersByKintoneMaster();

            SelectedDate = DateTime.Now;
            ShowSalesList();

            _ = InitializeAsync();

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
            await FetchKintoneCustomersAsync();
            UpdateCustomersByKintoneMaster();
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

                // レコード取得のURL
                string url = $"https://vk5k755s9nir.cybozu.com/k/v1/records.json?app=204";

                // レコードを取得
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
                    Address = (string)record["address"]?["value"]
                }).ToList();

                FetchedCustomers = new ObservableCollection<Customer>(records);
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

        private void Reload()
        {
            IsProgressRingActive = true;

            FetchKintoneOrders(new string[] { });

            LastFetchedAt = DateTime.Now;

            IsProgressRingActive = false;
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
                            Address = fetchedCustomer.Address,
                            CreatedAt = dt,
                            UpdatedAt = dt
                        };
                        context.Customers.Add(newCustomer);
                    }
                    else
                    {
                        // 既存の顧客を更新
                        existingCustomer.Name = fetchedCustomer.Name;
                        existingCustomer.Address = fetchedCustomer.Address;
                        existingCustomer.State = fetchedCustomer.State;
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


        private async Task FetchKintoneOrders(string[] args)
        {
            try
            {

                HttpClient client = new HttpClient();

                // kintoneのAPIトークン
                string apiToken = "clYsh32YLyQryGcHH8t4F8hkyIhOuEL67YBuyuvU";

                // ヘッダーにAPIトークンを設定
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Cybozu-API-Token", apiToken);

                // レコード取得のURL
                string url = $"https://vk5k755s9nir.cybozu.com/k/v1/records.json?app=203";

                // レコードを取得
                HttpResponseMessage response = await client.GetAsync(url);

                string responseBody = await response.Content.ReadAsStringAsync();

                // JSONデータをOrderオブジェクトに変換
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
            }
            catch (Exception ex)
            {

            }
            finally
            {

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
                var sale = new Sale
                {
                    SaleNo = 0, 
                    SaleDate = DateTime.Now,
                    CustomerCode = "0", // 必要に応じて設定
                    CustomerName = selectedOrder.CustomerName,
                    ProductCode = "0", // 必要に応じて設定
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
