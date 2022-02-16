using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        [Required]
        public int ItemId { get; set; }
        public Item Item;
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int Quantity { get; set; }

    }

}
