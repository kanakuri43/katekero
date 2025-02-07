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

namespace sale.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private ObservableCollection<Order> _orders;
        private int _selectedSaleNo;
        private int _selectedOrderNo;
        private DateTime _selectedDate;
        private string _json;
        private DateTime _lastFetchedAt;
        private bool _isProgressRingActive;
        private readonly Timer _timer;

        public ObservableCollection<Order> Orders
        {
            get { return _orders; }
            set { SetProperty(ref _orders, value); }
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
            ForwardCommand = new DelegateCommand(Forward);
            BackwardCommand = new DelegateCommand(Backward);
            ManualReloadCommand = new DelegateCommand(ManualReload);

            using (var context = new AppDbContext())
            {
                Sales = new ObservableCollection<Sale>(context.Sales.ToList());
            }

            ManualReload();
            // Timerの設定
            _timer = new Timer(60000); // 1分ごとに実行
            _timer.Elapsed += (sender, e) => ManualReload();
            _timer.Start();

            IsProgressRingActive = false;
            SelectedDate = DateTime.Now;
            ShowSalesList();

        }
        public DelegateCommand RegisterCommand { get; }
        public DelegateCommand SaleDoubleClickCommand { get; }
        public DelegateCommand OrderDoubleClickCommand { get; }
        public DelegateCommand SelectedDateChangedCommand { get; }
        public DelegateCommand ForwardCommand { get; }
        public DelegateCommand BackwardCommand { get; }
        public DelegateCommand ManualReloadCommand { get; }

        private void Register()
        {
            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);

        }
        private void Forward()
        {
            this.SelectedDate = this.SelectedDate.AddDays(1);
            ShowSalesList();
        }
        private void Backward()
        {
            this.SelectedDate = this.SelectedDate.AddDays(-1);
            ShowSalesList();
        }
        private async void ManualReload()
        {
            await FetchKintone(new string[] { });

            LastFetchedAt = DateTime.Now;
        }

        private async Task FetchKintone(string[] args)
        {
            try
            {
                IsProgressRingActive = true; 

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
                    Price = int.Parse((string)record["price"]["value"])
                }).ToList();

                Orders = new ObservableCollection<Order>(records);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                IsProgressRingActive = false; 
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
            var selectedOrder = Orders.FirstOrDefault(o => o.OrderNo == SelectedOrderNo);
            if (selectedOrder != null)
            {
                var sale = new Sale
                {
                    SaleNo = 0, 
                    SaleDate = DateTime.Now,
                    CustomerId = 0, // 必要に応じて設定
                    CustomerName = selectedOrder.CustomerName,
                    ProductId = 0, // 必要に応じて設定
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
