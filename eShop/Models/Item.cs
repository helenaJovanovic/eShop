using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }
        
        [Required]
        public float Price { get; set; }

        [Required]
        //Calculated as Price - Discount*Price
        public float Discount { get; set; }

        [Required]
        public string Name { get; set; }
       
        public string Description { get; set; }

        public int? CategoryId { get; set; }

        public string ImagePath { get; set; }

    }
}
