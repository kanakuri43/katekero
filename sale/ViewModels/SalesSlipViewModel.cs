using sale.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace sale.ViewModels
{
    public class SalesSlipViewModel : BindableBase
    {
        private ObservableCollection<Sale> _sales;
        private int _saleNo;
        private DateTime _saleDate;
        private string _upperTitle;
        private string _middleTitle;
        private string _lowerTitle;
        private string _customerName;
        private string _customerAddress;
        private string _customerZipCode;
        private int _subTotal;
        private int _taxPrice;
        private int _totalAmount;

        public ObservableCollection<Sale> Sales
        {
            get { return _sales; }
            set { SetProperty(ref _sales, value); }
        }
        public string UpperTitle
        {
            get { return _upperTitle; }
            set { SetProperty(ref _upperTitle, value); }
        }
        public string MiddleTitle
        {
            get { return _middleTitle; }
            set { SetProperty(ref _middleTitle, value); }
        }
        public string LowerTitle
        {
            get { return _lowerTitle; }
            set { SetProperty(ref _lowerTitle, value); }
        }
        public int SaleNo
        {
            get { return _saleNo; }
            set { SetProperty(ref _saleNo, value); }
        }
        public DateTime SaleDate
        {
            get { return _saleDate; }
            set { SetProperty(ref _saleDate, value); }
        }
        public string CustomerName
        {
            get { return _customerName; }
            set { SetProperty(ref _customerName, value); }
        }
        public string CustomerAddress
        {
            get { return _customerAddress; }
            set { SetProperty(ref _customerAddress, value); }
        }
        public string CustomerZipCode
        {
            get { return _customerZipCode; }
            set { SetProperty(ref _customerZipCode, value); }
        }
        public int Subtotal
        {
            get { return _subTotal; }
            set { SetProperty(ref _subTotal, value); }
        }
        public int TaxPrice
        {
            get { return _taxPrice; }
            set { SetProperty(ref _taxPrice, value); }
        }
        public int TotalAmount
        {
            get { return _totalAmount; }
            set { SetProperty(ref _totalAmount, value); }
        }


        public SalesSlipViewModel(ObservableCollection<Sale> sales)
        {
            Sales = sales ?? new ObservableCollection<Sale>();
            if (Sales != null && Sales.Count > 0)
            {
                SaleNo = Sales[0].SaleNo;
                SaleDate = Sales[0].SaleDate;
                CustomerName = Sales[0].CustomerName;
                CustomerAddress = Sales[0].CustomerAddress;
                CustomerZipCode = Sales[0].CustomerZipCode;

                Subtotal = Sales.Sum(s => s.Amount);
                TaxPrice = (int)((int)Subtotal * 0.1);
                TotalAmount = Subtotal + TaxPrice;
            }

        }

        public void SetUpperTitle(string title)
        {
            UpperTitle = title;
        }
        public void SetMiddleTitle(string title)
        {
            MiddleTitle = title;
        }
        public void SetLowerTitle(string title)
        {
            LowerTitle = title;
        }
    }
}
