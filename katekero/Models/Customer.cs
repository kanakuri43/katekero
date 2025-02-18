using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace katekero.Models
{
    [Table("customers")]
    public class Customer
    {
        public int Id { get; set; }
        [Column("state")]
        public int State { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("zip_code")]
        public string ZipCode { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("invoice_closing_day")]
        public int InvoiceClosingDay { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

    }
}
