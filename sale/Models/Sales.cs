using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sale.Models
{
    [Table("sales")]
    public class Sale : INotifyPropertyChanged
    {
        public int Id { get; set; }

        [Column("state")]
        public int State { get; set; }
        [Column("sale_date")]
        public DateTime SaleDate { get; set; }
        [Column("sale_no")]
        public int SaleNo { get; set; }
        [Column("line_no")]
        public int LineNo { get; set; }
        [Column("customer_id")]
        public int CustomerId { get; set; }
        [Column("customer_name")]
        public string CustomerName { get; set; }
        [Column("product_id")]
        public int ProductId { get; set; }
        [Column("product_name")]
        public string ProductName { get; set; }

        private int _quantity;
        [Column("quantity")]
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        [Column("price")]
        public int Price { get; set; }
        [Column("tax_rate")]
        public int TaxRate { get; set; }
        [Column("tax_price")]
        public int TaxPrice { get; set; }

        private int _amount;
        [Column("amount")]
        public int Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        [Column("is_invoice_closed")]
        public int IsInvoiceClosed { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
