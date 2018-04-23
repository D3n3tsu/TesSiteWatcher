using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineStoreWatcher.Controllers;
using OnlineStoreWatcher.Data;
using OnlineStoreWatcher.Models;
using OnlineStoreWatcher.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace OnlineStoreWatcher.Tests
{
    public class ProductControllerTests
    {

        [Fact]
        public void List_Uses_Products_Returned_For_Generating_ViewModel()
        {
            // Arrange
            IQueryable<Product> products = GetProductList().AsQueryable();
            Mock<IRepository> mock = new Mock<IRepository>();
            mock.Setup(m => m.GetProductsAsQueryable()).Returns(products);
            ProductController target = new ProductController(mock.Object);

            // Act
            var result = target.List();

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ProductPreviewViewModel>>(viewResult.Model);
            Assert.Equal(1M, model[0].CurrentPrice);
            Assert.Equal(1, model[0].Id);
            Assert.Equal("Name", model[0].Name);
        }

        // Create two test lists. Create new price with larger date. Prepend new price to first, then append to second. Check if results are correct.
        [Fact]
        public void List_Uses_Last_Price_For_Generating_ViewModel()
        {
            // Arrange            
            List<Product> rawProducts1 = GetProductList();
            List<Product> rawProducts2 = GetProductList();

            ProductPrice newPrice = GetPrice(rawProducts1.First());

            rawProducts1.First().Price = rawProducts1.First().Price.Append(newPrice).ToList();
            IQueryable<Product> products1 = rawProducts1.AsQueryable();
            Mock<IRepository> mock1 = new Mock<IRepository>();
            mock1.Setup(m => m.GetProductsAsQueryable()).Returns(products1);
            ProductController target1 = new ProductController(mock1.Object);

            rawProducts2.First().Price = rawProducts2.First().Price.Prepend(newPrice).ToList();
            IQueryable<Product> products2 = rawProducts2.AsQueryable();
            Mock<IRepository> mock2 = new Mock<IRepository>();
            mock2.Setup(m => m.GetProductsAsQueryable()).Returns(products2);
            ProductController target2 = new ProductController(mock2.Object);

            // Act
            var result1 = target1.List();
            var result2 = target2.List();

            // Assert
            var viewResult1 = Assert.IsAssignableFrom<ViewResult>(result1);
            var model1 = Assert.IsAssignableFrom<List<ProductPreviewViewModel>>(viewResult1.Model);
            Assert.Equal(200M, model1[0].CurrentPrice);

            var viewResult2 = Assert.IsAssignableFrom<ViewResult>(result2);
            var model2 = Assert.IsAssignableFrom<List<ProductPreviewViewModel>>(viewResult2.Model);
            Assert.Equal(200M, model2[0].CurrentPrice);
        }

        [Fact]
        public void ProductDetails_Uses_Reposytory_ProductsAndPrices()
        {
            // Arrange
            Product product = GetProductList().First();
            Mock<IRepository> mock = new Mock<IRepository>();
            mock.Setup(m => m.GetProductById(It.IsAny<int>())).Returns(product);
            mock.Setup(m => m.GetProductPrices(It.IsAny<int>()))
                .Returns(new List<ProductPrice>() { GetPrice(product) });
            ProductController target = new ProductController(mock.Object);

            // Act
            var result = target.ProductDetails(product.Id);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ProductDetailsViewModel>(viewResult.Model);
            Assert.Equal("Description", model.Description);
            Assert.Equal("Name", model.Name);
            Assert.Equal(200M, model.Prices.First().Price);
            Assert.Equal(DateTime.MaxValue, model.Prices.First().Date);
            Assert.Single(model.Prices);
        }

        private ProductPrice GetPrice(Product product)
        {
            return new ProductPrice()
            {
                Id = 3,
                LastUpdate = DateTime.MaxValue,
                Price = 200M,
                Product = product,
                ProductId = product.Id
            };
        }

        private List<Product> GetProductList()
        {
            Product product = new Product()
            {
                Id = 1,
                Description = "Description",
                Image = new byte[1],
                Name = "Name"
            };
            ProductPrice price = new ProductPrice()
            {
                Id = 2,
                LastUpdate = DateTime.MinValue,
                Price = 1M,
                Product = product,
                ProductId = product.Id
            };
            product.Price = new List<ProductPrice>() {
                price
            };
            return new List<Product>()
            {
                product
            };
        }
    }
}
