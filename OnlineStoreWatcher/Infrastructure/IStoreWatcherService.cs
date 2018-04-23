using OnlineStoreWatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Infrastructure
{
    public interface IStoreWatcherService
    {
        bool IsActive { get; }
        Task<ICollection<NewProductModel>> WatchSite();
    }
}
