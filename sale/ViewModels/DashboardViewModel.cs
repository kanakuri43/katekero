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

namespace sale.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private ObservableCollection<Order> _orders;
        private int _selectedSaleNo;
        private DateTime _selectedDate;
        private string _json;
        private DateTime _lastFetchecAt;
        private bool _isProgressRingActive;

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
            get { return _lastFetchecAt; }
            set { SetProperty(ref _lastFetchecAt, value); }
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
            SelectedDateChangedCommand = new DelegateCommand(SelectedDateChanged);
            RegisterCommand = new DelegateCommand(Register);
            ForwardCommand = new DelegateCommand(Forward);
            BackwardCommand = new DelegateCommand(Backward);
            ReloadCommand = new DelegateCommand(Reload);

            using (var context = new AppDbContext())
            {
                Sales = new ObservableCollection<Sale>(context.Sales.ToList());
            }

            IsProgressRingActive = false; // ProgressRingを非表示
            SelectedDate = DateTime.Now;
            ShowSalesList();

        }
        public DelegateCommand RegisterCommand { get; }
        public DelegateCommand SaleDoubleClickCommand { get; }
        public DelegateCommand SelectedDateChangedCommand { get; }
        public DelegateCommand ForwardCommand { get; }
        public DelegateCommand BackwardCommand { get; }
        public DelegateCommand ReloadCommand { get; }

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
        private async void Reload()
        {
            await FetchKintone(new string[] { });

            LastFetchedAt = DateTime.Now;
        }

        private async Task FetchKintone(string[] args)
        {
            try
            {
                IsProgressRingActive = true; // ProgressRingを表示

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

                // Jsonプロパティにパースした結果を設定
                //Json = JObject.Parse(responseBody).ToString();

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
                IsProgressRingActive = false; // ProgressRingを非表示
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
