using katekero.Models;
using katekero.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace katekero.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Sale> _sales;
        private int _selectedSaleNo;
        private DateTime _selectedDate;

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

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SaleDoubleClickCommand = new DelegateCommand(SaleDoubleClick);
            SelectedDateChangedCommand = new DelegateCommand(SelectedDateChanged);
            RegisterCommand = new DelegateCommand(Register);

            using (var context = new AppDbContext())
            {
                Sales = new ObservableCollection<Sale>(context.Sales.ToList());
            }

            SelectedDate = DateTime.Now;
            ShowSales();

        }
        public DelegateCommand RegisterCommand { get; }
        public DelegateCommand SaleDoubleClickCommand { get; }
        public DelegateCommand SelectedDateChangedCommand { get; }

        private void Register()
        {
            var p = new NavigationParameters();
            p.Add(nameof(RegisterViewModel.SaleNo), 0);
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);

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
            ShowSales();
        }

        private void ShowSales()
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
    }
}
