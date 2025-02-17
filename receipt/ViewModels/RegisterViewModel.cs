﻿using receipt.Models;
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
        private string _customerCode;
        private string _customerName;
        private int _selectedAccountId;

        public DelegateCommand SaveReceiptCommand { get; }
        public DelegateCommand DeleteReceiptCommand { get; }
        public DelegateCommand BackToHomeCommand { get; }
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
        public ObservableCollection<katekero.Models.Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
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

        private void OnSalesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(TotalReceiptAmount));
        }

        public RegisterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SaveReceiptCommand = new DelegateCommand(SaveReceipt);
            DeleteReceiptCommand = new DelegateCommand(DeleteReceipt);
            BackToHomeCommand = new DelegateCommand(BackToHome);
            AccountDoubleClickCommand = new DelegateCommand(AccountDoubleClick);
            CustomerSearchCommand = new DelegateCommand(CustomerSearch);

            // Receiptsの初期化
            Receipts = new ObservableCollection<Receipt>();
            Receipts.CollectionChanged += OnReceiptsCollectionChanged;

            // 得意先マスタ
            using (var context = new AppDbContext())
            {
                Customers = new ObservableCollection<katekero.Models.Customer>(context.Customers.ToList());
            }
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
                        CustomerCode = this.CustomerCode,
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
                var dialogSettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = "OK",
                    AnimateShow = false,
                    AnimateHide = false
                }; if (metroWindow != null)
                {
                    await metroWindow.ShowMessageAsync("登録しました", "", MessageDialogStyle.Affirmative, dialogSettings);
                }
            }

        }
        private void DeleteReceipt()
        {

        }
        private void BackToHome()
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
            var receipts = navigationContext.Parameters.GetValue<ObservableCollection<Receipt>>(nameof(Receipts));
            if (receipts == null)
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

                this.ReceiptNo = 0;
                this.ReceiptDate = DateTime.Now;

                //this.CanHeaderEdit = true;  // 日付・得意先 変更可
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
