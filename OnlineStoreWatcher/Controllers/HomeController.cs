using Microsoft.AspNetCore.Mvc;
using OnlineStoreWatcher.Data;
using OnlineStoreWatcher.Infrastructure;
using OnlineStoreWatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStoreWatcherService _storeWatcherService;
        private readonly IRepository _repository;

        public HomeController(IStoreWatcherService storeWatcherService, IRepository repo)
        {
            _storeWatcherService = storeWatcherService;
            _repository = repo;
        }

        public IActionResult Index()
        {
            if (_storeWatcherService.IsActive)
            {
                return View("BusyService");
            }
            return View();
        }

        public async Task<IActionResult> RunServise()
        {
            if (!_storeWatcherService.IsActive)
            {
                await RunWatcher();
            }
            return RedirectToAction("List", "Product");
        }

        private async Task RunWatcher()
        {
            ICollection<NewProductModel> productModels = new List<NewProductModel>();
            if (!_storeWatcherService.IsActive)
            {
                productModels = await _storeWatcherService.WatchSite();
            }
            foreach (var productModel in productModels)
            {
                await _repository.AddOrUpdateProduct(productModel);
            }
        }
    }
}
