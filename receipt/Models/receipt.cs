using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace receipt.Models
{
    [Table("receipts")]
    public class Receipt : INotifyPropertyChanged
    {
        public int Id { get; set; }

        [Column("state")]
        public int State { get; set; }

        [Column("receipt_date")]
        public DateTime ReceiptDate { get; set; }

        [Column("receipt_no")]
        public int ReceiptNo { get; set; }

        [Column("line_no")]
        public int LineNo { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; }

        [Column("account_id")]
        public int AccountId { get; set; }

        [Column("account_name")]
        public string AccountName { get; set; }

        private int _receiptAmount;
        [Column("receipt_amount")]
        public int ReceiptAmount
        {
            get { return _receiptAmount; }
            set
            {
                if (_receiptAmount != value)
                {
                    _receiptAmount = value;
                    OnPropertyChanged(nameof(ReceiptAmount));
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
