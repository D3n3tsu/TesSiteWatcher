using Microsoft.EntityFrameworkCore;
using OnlineStoreWatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Data
{
    public class SqlProductRepository : IRepository
    {
        private readonly ProductContext _context;

        public SqlProductRepository(ProductContext ctx)
        {
            _context = ctx;
        }

        public async Task AddOrUpdateProduct(NewProductModel newProductModel)
        {
            Product product = await FindOrCreateProduct(newProductModel.Name, newProductModel.Description, newProductModel.Image);

            await SaveNewPrice(product.Id, newProductModel.Price);
        }

        public Product GetProductByName(string name)
        {
            Product product = null;
            try
            {
                product = _context.Products.SingleOrDefault(row => row.Name == name);
            }
            catch (Exception ex)
            {
                throw new Exception("There is more than one product with name provided!", ex);
            }
            return product;
        }

        public IQueryable<Product> GetProductsAsQueryable()
        {
            return _context.Products.Include(product => product.Price);
        }

        public Product GetProductById(int productId)
        {
            return _context.Products.FirstOrDefault(product => product.Id == productId);
        }

        public ICollection<ProductPrice> GetProductPrices(int productId)
        {
            return _context.Prices.Where(price => price.ProductId == productId).ToList();
        }

        #region HelperMethods

        private async Task<ProductPrice> SaveNewPrice(int productId, decimal price)
        {
            ProductPrice newPrice = new ProductPrice()
            {
                Price = price,
                ProductId = productId,
                LastUpdate = DateTime.Now
            };

            try
            {
                await _context.Prices.AddAsync(newPrice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving new price to db!", ex);
            }

            return newPrice;
        }

        private async Task<Product> FindOrCreateProduct(string name, string description, byte[] img)
        {
            Product product = GetProductByName(name);
            if (product == null)
            {
                product = new Product()
                {
                    Name = name,
                    Description = description,
                    Image = img
                };
                try
                {
                    await _context.Products.AddAsync(product);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while saving new product to db!", ex);
                }
            }
            if (product.Description != description)
            {
                product.Description = description;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while saving new description to product id:{product.Id}!", ex);
                }
            }
            if (product.Image != img)
            {
                product.Image = img;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while saving new image to product id:{product.Id}!", ex);
                }
            }

            return product;
        }
        #endregion

    }
}
