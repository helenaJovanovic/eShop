using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Models
{
    public class AddItemRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
