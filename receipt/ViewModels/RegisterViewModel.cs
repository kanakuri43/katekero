using receipt.Models;
using receipt.Views;
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

namespace receipt.ViewModels
{
    public class RegisterViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private DateTime _receiptDate;
        private int _receiptId;
        private int _receiptNo;
        private ObservableCollection<Receipt> _receipts;
        private ObservableCollection<katekero.Models.Customer> _customers;
        private ObservableCollection<Account> _accounts;
        private int _totalReceiptAmount;
        private int _customerId;
        private string _customerName;
        private int _selectedAccountId;

        public DelegateCommand SaveReceiptCommand { get; }
        public DelegateCommand DeleteReceiptCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }
        public DelegateCommand AccountDoubleClickCommand { get; }

        public DateTime ReceiptDate
        {
            get { return _receiptDate; }
            set { SetProperty(ref _receiptDate, value); }
        }
        public int ReceiptId
        {
            get { return _receiptId; }
            set { SetProperty(ref _receiptId, value); }
        }
        public int ReceiptNo
        {
            get { return _receiptNo; }
            set { SetProperty(ref _receiptNo, value); }
        }
        public int TotalReceiptAmount
        {
            get { return _totalReceiptAmount; }
            set { SetProperty(ref _totalReceiptAmount, value); }
        }
        public ObservableCollection<Account> Accounts
        {
            get { return _accounts; }
            set { SetProperty(ref _accounts, value); }
        }
        public int SelectedAccountId
        {
            get { return _selectedAccountId; }
            set { SetProperty(ref _selectedAccountId, value); }
        }

        public ObservableCollection<Receipt> Receipts
        {
            get { return _receipts; }
            set
            {
                if (SetProperty(ref _receipts, value))
                {
                    _receipts.CollectionChanged += OnSalesCollectionChanged;
                    RaisePropertyChanged(nameof(TotalReceiptAmount));
                }
            }
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

        private void OnSalesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(TotalReceiptAmount));
        }

        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SaveReceiptCommand = new DelegateCommand(SaveReceipt);
            DeleteReceiptCommand = new DelegateCommand(DeleteReceipt);
            CancelCommand = new DelegateCommand(Home);
            AccountDoubleClickCommand = new DelegateCommand(AccountDoubleClick);
            CustomerSearchCommand = new DelegateCommand(CustomerSearch);

            // Receiptsの初期化
            Receipts = new ObservableCollection<Receipt>();
            Receipts.CollectionChanged += OnReceiptsCollectionChanged;

            // 入金方法
            using (var context = new AppDbContext())
            {
                Accounts = new ObservableCollection<Account>(context.Accounts.ToList());
            }

        }

        private void OnReceiptsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            RaisePropertyChanged(nameof(TotalReceiptAmount));
        }


        private void AddReceiptDetail()
        {
            using (var context = new AppDbContext())
            {
                var account = context.Accounts.FirstOrDefault(a => a.Id == _selectedAccountId);

                var existingReceipt = Receipts.FirstOrDefault(s => s.AccountId == account.Id);

                if (existingReceipt != null)
                {

                }
                else
                {
                    var r = new Receipt
                    {
                        State = 0,
                        ReceiptNo = this.ReceiptNo,
                        CustomerId = this.CustomerId,
                        CustomerName = this.CustomerName,
                        AccountId = account.Id,
                        AccountName = account.Name,
                        ReceiptAmount = 0
                    };
                    Receipts.Add(r);
                }
            }
        }


        private void AccountDoubleClick()
        {
            AddReceiptDetail();
        }
        private async void SaveReceipt()
        {
            using (var context = new AppDbContext())
            {
                var dt = DateTime.Now;
                if (this.ReceiptNo == 0)
                {
                    // 売上番号設定
                    int maxSaleNo = context.Receipts.Max(s => (int?)s.ReceiptNo) ?? 0;
                    int newSaleNo = maxSaleNo + 1;

                    var lineNo = 0;
                    foreach (var r in _receipts)
                    {
                        lineNo++;

                        r.ReceiptDate = this.ReceiptDate;
                        r.ReceiptNo = newSaleNo;
                        r.LineNo = lineNo;
                        r.CreatedAt = dt;
                        r.UpdatedAt = dt;
                        context.Receipts.Add(r);
                    }
                }
                else
                {

                }

                context.SaveChanges();

                // ダイアログを表示
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                if (metroWindow != null)
                {
                    await metroWindow.ShowMessageAsync("登録しました", "");
                }
            }

        }
        private void DeleteReceipt()
        {

        }
        private void Home()
        {
            if (this.Receipts != null)
            {
                this.Receipts.Clear();
            }

            var p = new NavigationParameters();
            _regionManager.RequestNavigate("ContentRegion", nameof(Dashboard), p);
        }

        private void CustomerSearch()
        {
            var p = new NavigationParameters();
            p.Add(nameof(CustomerSearchViewModel.Sales), new ObservableCollection<Receipt>(this.Receipts));
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.CustomerSearch), p);

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.CustomerId = navigationContext.Parameters.GetValue<int>(nameof(CustomerId));
            this.CustomerName = navigationContext.Parameters.GetValue<string>(nameof(CustomerName));

            this.ReceiptDate = DateTime.Now;

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
