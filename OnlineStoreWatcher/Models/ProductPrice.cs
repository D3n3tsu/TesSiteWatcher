using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Models
{
    public class ProductPrice
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdate { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }
    }
}
