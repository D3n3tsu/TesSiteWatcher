using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public string Name { get; set; }
        public ICollection<PriceViewModel> Prices { get; set; }
        public string ImageBase64 { get; set; }
        public string Description { get; set; }
    }
}
