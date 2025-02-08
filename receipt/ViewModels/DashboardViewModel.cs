using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using receipt.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace receipt.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private bool _isProgressRingActive;
        private DateTime _selectedDate;
        private ObservableCollection<Receipt> _receipt;
        private int _selectedReceiptNo;

        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set { SetProperty(ref _isProgressRingActive, value); }
        }
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }
        public ObservableCollection<Receipt> Receipts
        {
            get { return _receipt; }
            set { SetProperty(ref _receipt, value); }
        }
        public int SelectedReceiptNo
        {
            get { return _selectedReceiptNo; }
            set { SetProperty(ref _selectedReceiptNo, value); }
        }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            ReceiptDoubleClickCommand = new DelegateCommand(ReceiptDoubleClick);
            SelectedDateChangedCommand = new DelegateCommand(SelectedDateChanged);
            RegisterCommand = new DelegateCommand(Register);
            ForwardCommand = new DelegateCommand(Forward);
            BackwardCommand = new DelegateCommand(Backward);

            using (var context = new AppDbContext())
            {
                Receipts = new ObservableCollection<Receipt>(context.Receipts.ToList());
            }

            SelectedDate = DateTime.Now;
            ShowReceiptsList();

        }
        public DelegateCommand RegisterCommand { get; }
        public DelegateCommand ReceiptDoubleClickCommand { get; }
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
            ShowReceiptsList();
        }
        private void Backward()
        {
            this.SelectedDate = this.SelectedDate.AddDays(-1);
            ShowReceiptsList();
        }
        private void SelectedDateChanged()
        {
            ShowReceiptsList();
        }

        private void ShowReceiptsList()
        {
            using var context = new AppDbContext();

            // 条件に基づいてデータをフィルタリング
            this.Receipts = new ObservableCollection<Receipt>(
                context.Receipts
                    .Where(s => s.ReceiptDate.Date == this.SelectedDate.Date)
                    .OrderBy(s => s.ReceiptNo)
                    .ThenBy(s => s.LineNo)
            );

        }
        private void ReceiptDoubleClick()
        {
            var selectedSales = Receipts.Where(s => s.ReceiptNo == SelectedReceiptNo).ToList();
            if (selectedSales.Any())
            {
                var p = new NavigationParameters();
                p.Add(nameof(RegisterViewModel.Receipts), new ObservableCollection<Receipt>(selectedSales));
                _regionManager.RequestNavigate("ContentRegion", nameof(Views.Register), p);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SelectedDate = DateTime.Now;
            ShowReceiptsList();
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
