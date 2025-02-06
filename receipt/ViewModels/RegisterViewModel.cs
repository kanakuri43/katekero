using receipt.Models;
using receipt.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using receipt.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;

namespace receipt.ViewModels
{
    public class RegisterViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Receipt> _receipts;
        private int _receiptNo;
        private int _totalReceiptAmount;
        private ObservableCollection<Account> _accounts;
        private int _customerId;
        private string _customerName;
        private int _selectedAccountId;

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CustomerSearchCommand { get; }
        public DelegateCommand AccountDoubleClickCommand { get; }

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

            SaveCommand = new DelegateCommand(Save);
            DeleteCommand = new DelegateCommand(Delete);
            CancelCommand = new DelegateCommand(Home);
            AccountDoubleClickCommand = new DelegateCommand(AccountDoubleClick);

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
        private void Save()
        {

        }
        private void Delete()
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


        public void OnNavigatedTo(NavigationContext navigationContext)
        {

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
