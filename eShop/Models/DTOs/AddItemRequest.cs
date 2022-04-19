using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace eShop.Models.DTOs
{
    public class AddItemRequest
    {
        [Required]
        public float Price { get; set; }

        [Required]
        //Calculated as Price - Discount*Price
        public float Discount { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
        public int? CategoryId { get; set; }

        public string ImagePath { get; set; }

    }
}

