using katekero.Models;
using katekero.Views;
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

namespace katekero.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private int _selectedSaleNo;
        private DateTime _selectedDate;
        private string _json;
        private DateTime _lastFetchecAt;

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
            p.Add(nameof(RegisterViewModel.SaleNo), 0);
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
            HttpClient client = new HttpClient();
            string apiToken = "ZQjrR6cwgLLBpt5NZQU4INEg8pAeWyE0AYdSWEHg"; // kintoneのAPIトークンを入力

            // ヘッダーにAPIトークンを設定
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Cybozu-API-Token", apiToken);

            // レコード取得のURL
            string url = $"https://vk5k755s9nir.cybozu.com/k/v1/records.json?app=140";

            // レコードを取得
            HttpResponseMessage response = await client.GetAsync(url);

            string responseBody = await response.Content.ReadAsStringAsync();

            // Jsonプロパティにパースした結果を設定
            Json = JObject.Parse(responseBody).ToString();
        }


        private void SaleDoubleClick()
        {
            var p = new NavigationParameters();
            p.Add(nameof(RegisterViewModel.SaleNo), _selectedSaleNo);
            p.Add(nameof(RegisterViewModel.CustomerId), 0);
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);
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
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
