﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sale.Models
{
    [Table("products")]
    public class Product
    {
        public int Id { get; set; }
        [Column("state")]
        public int State { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("category_code")]
        public string CategoryCode { get; set; }
        [Column("price")]
        public int Price { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

    }
}
