using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineStoreWatcher.Controllers;
using OnlineStoreWatcher.Data;
using OnlineStoreWatcher.Infrastructure;
using OnlineStoreWatcher.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OnlineStoreWatcher.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_Returns_BusyService_View_If_IsAlive_True()
        {
            // Arrange
            Mock<IRepository> repoMock = new Mock<IRepository>();
            Mock<IStoreWatcherService> serviceMock = new Mock<IStoreWatcherService>();
            serviceMock.Setup(s => s.IsActive).Returns(true);
            HomeController target = new HomeController(serviceMock.Object, repoMock.Object);

            // Act
            var result = target.Index();

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.Equal("BusyService", viewResult.ViewName);
        }

        [Fact]
        public void Index_Returns_Default_View_If_IsAlive_False()
        {
            // Arrange
            Mock<IRepository> repoMock = new Mock<IRepository>();
            Mock<IStoreWatcherService> serviceMock = new Mock<IStoreWatcherService>();
            serviceMock.Setup(s => s.IsActive).Returns(false);
            HomeController target = new HomeController(serviceMock.Object, repoMock.Object);

            // Act
            var result = target.Index();

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task RunServise_Runs_RunWatcher_If_IsAlive_False()
        {
            // Arrange
            Mock<IRepository> repoMock = new Mock<IRepository>();
            Mock<IStoreWatcherService> serviceMock = new Mock<IStoreWatcherService>();
            serviceMock.Setup(s => s.IsActive).Returns(false);
            ICollection<NewProductModel> returnValue = new List<NewProductModel>();
            serviceMock.Setup(s => s.WatchSite()).Returns(Task.FromResult(returnValue));
            HomeController target = new HomeController(serviceMock.Object, repoMock.Object);

            // Act
            await target.RunServise();

            // Assert
            serviceMock.Verify(n => n.WatchSite(), Times.Once);
        }

        [Fact]
        public async Task RunServise_Does_Not_Run_RunWatcher_If_IsAlive_True()
        {
            // Arrange
            Mock<IRepository> repoMock = new Mock<IRepository>();
            Mock<IStoreWatcherService> serviceMock = new Mock<IStoreWatcherService>();
            serviceMock.Setup(s => s.IsActive).Returns(true);
            HomeController target = new HomeController(serviceMock.Object, repoMock.Object);

            // Act
            await target.RunServise();

            // Assert
            serviceMock.Verify(n => n.WatchSite(), Times.Never);
        }
    }
}
