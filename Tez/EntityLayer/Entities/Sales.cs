﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Sales
    {
        public int Id { get; set; }

        [Display(Name = "ProductId")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Image")]
        public string Image { get; set; }

        [Display(Name = "UserId")]
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
