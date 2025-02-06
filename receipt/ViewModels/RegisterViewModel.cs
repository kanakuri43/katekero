using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using receipt.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace receipt.ViewModels
{
    public class RegisterViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Receipt> _receipts;
        private int _totalReceiptAmount;

        public int TotalReceiptAmount
        {
            get { return _totalReceiptAmount; }
            set { SetProperty(ref _totalReceiptAmount, value); }
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
        private void OnSalesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(TotalReceiptAmount));
        }

        public RegisterViewModel()
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }
    }
}
