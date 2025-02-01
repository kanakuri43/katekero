using katekero.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace katekero.ViewModels
{
    public class SalesSlipViewModel : BindableBase
    {
        private ObservableCollection<Sale> _sales;
        private int _saleNo;
        private string _customerName;

        public ObservableCollection<Sale> Sales
        {
            get { return _sales; }
            set { SetProperty(ref _sales, value); }
        }
        public int SaleNo
        {
            get { return _saleNo; }
            set { SetProperty(ref _saleNo, value); }
        }
        public string CustomerName
        {
            get { return _customerName; }
            set { SetProperty(ref _customerName, value); }
        }


        public SalesSlipViewModel(ObservableCollection<Sale> sales)
        {
            Sales = sales ?? new ObservableCollection<Sale>();
            if (Sales != null && Sales.Count > 0)
            {
                SaleNo = Sales[0].SaleNo;
                CustomerName = Sales[0].CustomerName;
            }

        }
    }
}
