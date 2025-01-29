using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace katekero.Models
{
    [Table("sales")]
    public class Sale
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
        [Column("quantity")]
        public int Quantity { get; set; }
        [Column("price")]
        public int Price { get; set; }
        [Column("tax_rate")]
        public int TaxRate { get; set; }
        [Column("tax_price")]
        public int TaxPrice { get; set; }
        [Column("amount")]
        public int Amount { get; set; }
        [Column("is_invoice_closed")]
        public int IsInvoiceClosed { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

    }
}
