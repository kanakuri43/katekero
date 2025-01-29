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

        public ObservableCollection<Sale> Sales
        {
            get { return _sales; }
            set { SetProperty(ref _sales, value); }
        }

        public SalesSlipViewModel()
        {

        }
    }
}
