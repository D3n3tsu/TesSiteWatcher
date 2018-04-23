using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineStoreWatcher.Data;
using OnlineStoreWatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace OnlineStoreWatcher.Tests
{
    public class SqlProductRepositoryTests
    {
        [Fact]
        public void GetProductByName_Returns_Null_If_No_Product_Found()
        {
            // Arrange
            Product product = GetProductWithName("Name");
            ProductContext ctx = new ProductContext(new DbContextOptions<ProductContext>());
            ctx.Products = GetProductDbSet(product);
            SqlProductRepository target = new SqlProductRepository(ctx);

            // Act
            var result = target.GetProductByName("FalseName");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetProductByName_Returns_Product_If_Correct_Name()
        {
            // Arrange
            Product product = GetProductWithName("Name");
            ProductContext ctx = new ProductContext(new DbContextOptions<ProductContext>());
            ctx.Products = GetProductDbSet(product);
            SqlProductRepository target = new SqlProductRepository(ctx);

            // Act
            var result = target.GetProductByName("Name");

            // Assert
            Assert.IsAssignableFrom<Product>(result);
        }

        [Fact]
        public void GetProductsAsQueryable_Returns_All_Products()
        {
            // Arrange
            Product product1 = GetProductWithName("Name1");
            Product product2 = GetProductWithName("Name2");
            product2.Id = 2;
            ProductContext ctx = new ProductContext(new DbContextOptions<ProductContext>());
            ctx.Products = GetProductDbSet(product1, product2);
            SqlProductRepository target = new SqlProductRepository(ctx);

            // Act
            var result = target.GetProductsAsQueryable().ToList();

            // Assert
            Assert.Contains(product1, result);
            Assert.Contains(product2, result);
        }

        [Fact]
        public void GetProductById_Returns_Correct_Product()
        {
            // Arrange
            Product product1 = GetProductWithName("Name1");
            Product product2 = GetProductWithName("Name2");
            product2.Id = 2;
            ProductContext ctx = new ProductContext(new DbContextOptions<ProductContext>());
            ctx.Products = GetProductDbSet(product1, product2);
            SqlProductRepository target = new SqlProductRepository(ctx);

            // Act
            var result = target.GetProductById(2);

            // Assert
            Assert.StrictEqual(product2, result);
        }

        [Fact]
        public void GetProductPrices_Returns_Correct_Prices()
        {
            // Arrange
            Product product1 = GetProductWithName("Name1");
            Product product2 = GetProductWithName("Name2");
            product2.Id = 2;
            ProductContext ctx = new ProductContext(new DbContextOptions<ProductContext>());
            ctx.Products = GetProductDbSet(product1, product2);
            ProductPrice price1a = GetPrice(product1);
            ProductPrice price1b = GetPrice(product1);
            ProductPrice price2a = GetPrice(product2);
            ProductPrice price2b = GetPrice(product2);
            ctx.Prices = GetPricesDbSet(price1a, price1b, price2a, price2b);
            SqlProductRepository target = new SqlProductRepository(ctx);

            // Act
            var result = target.GetProductPrices(2);

            // Assert
            Assert.Contains(price2a, result);
            Assert.Contains(price2b, result);
        }

        private DbSet<Product> GetProductDbSet(params Product[] products)
        {
            return GetQueryableMockDbSet(products);
        }

        private DbSet<ProductPrice> GetPricesDbSet(params ProductPrice[] prices)
        {
            return GetQueryableMockDbSet(prices);
        }

        private Product GetProductWithName(string name)
        {
            Product product =  new Product()
            {
                Description = "Description",
                Id = 1,
                Image = new byte[1],
                Name = name
            };
            product.Price = new List<ProductPrice>() { GetPrice(product) };
            return product;
        }

        private ProductPrice GetPrice(Product product)
        {
            return new ProductPrice()
            {
                Id = 1,
                Price = 1M,
                LastUpdate = DateTime.MinValue,
                Product = product,
                ProductId = product.Id
            };
        }

        private static DbSet<T> GetQueryableMockDbSet<T>(params T[] sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSet.Object;
        }
    }
}
