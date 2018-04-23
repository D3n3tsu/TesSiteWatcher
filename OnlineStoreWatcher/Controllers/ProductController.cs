using Microsoft.AspNetCore.Mvc;
using OnlineStoreWatcher.Data;
using OnlineStoreWatcher.Models;
using OnlineStoreWatcher.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Controllers
{
    public class ProductController : Controller
    {
        private readonly IRepository _repository;

        public ProductController(IRepository repo)
        {
            _repository = repo;
        }

        public ActionResult List()
        {
            List<ProductPreviewViewModel> models = new List<ProductPreviewViewModel>();
            try
            {
                IQueryable<Product> products = _repository.GetProductsAsQueryable();
                models = products.Select(product => new ProductPreviewViewModel()
                {
                    Id = product.Id,
                    Name = product.Name,
                    CurrentPrice = product.Price.OrderByDescending(price => price.LastUpdate).Select(prod => prod.Price).FirstOrDefault()
                }).ToList();
            }
            catch (Exception)
            {
                RedirectToAction("Index", "Home");
            }
            return View(models);
        }

        public ActionResult ProductDetails(int productId)
        {
            Product product = null;
            ICollection<ProductPrice> prices = null;
            try
            {
                product = _repository.GetProductById(productId);
                prices = _repository.GetProductPrices(productId).OrderByDescending(price => price.LastUpdate).ToList();
            }
            catch (Exception)
            {
                RedirectToAction("Index", "Home");
            }
            ProductDetailsViewModel model = new ProductDetailsViewModel()
            {
                Name = product.Name,
                Description = product.Description,
                ImageBase64 = $"data:image/png;base64, {Convert.ToBase64String(product.Image)}",
                Prices = prices.Select(price => new PriceViewModel() { Date = price.LastUpdate, Price = price.Price }).ToList()
            };

            return View(model);
        }
    }
}
