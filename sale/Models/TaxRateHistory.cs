using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sale.Models
{
    [Table("tax_rate_histories")]
    public class TaxRateHistory
    {
        public int Id { get; set; }

        [Column("started_date")]
        public DateTime StartedDate { get; set; }

        [Column("primary_tax_rate")]
        public int PrimaryTaxRate { get; set; }

        [Column("secondary_tax_rate")]
        public int SecondaryTaxRate { get; set; }

        [Column("tertiary_tax_rate")]
        public int TeritiaryTaxRate { get; set; }

    }
}
