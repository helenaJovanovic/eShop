using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Models
{
    public class AddItemRequest
    {
        [Required]
        public int ItemId { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
