using OnlineStoreWatcher.Models;
using OnlineStoreWatcher.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Data
{
    public interface IRepository
    {
        Task AddOrUpdateProduct(NewProductModel newProductModel);
        IQueryable<Product> GetProductsAsQueryable();
        Product GetProductById(int productId);
        ICollection<ProductPrice> GetProductPrices(int productId);
    }
}
