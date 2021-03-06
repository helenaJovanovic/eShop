using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public int UserId { get; set; }

        [System.ComponentModel.DefaultValue(0)]
        public float Total { get; set; }
        public IList<CartItem> CartItems { get; set; }
    }
}
