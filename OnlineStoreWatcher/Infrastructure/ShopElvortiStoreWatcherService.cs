using AngleSharp;
using AngleSharp.Dom;
using OnlineStoreWatcher.Data;
using OnlineStoreWatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStoreWatcher.Infrastructure
{
    public class ShopElvortiStoreWatcherService : IStoreWatcherService
    {
        // Targeted site address
        string address = "http://www.shopelvorti.com/";

        
        private object _lockObject = new object();
        private volatile bool _isActive;

        // This property will help to control single StartWatch method execution
        public bool IsActive
        {
            get
            {
                lock (_lockObject)
                {
                    return _isActive;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _isActive = value;
                }
            }
        }

        public async Task<ICollection<NewProductModel>> WatchSite()
        {
            ICollection<NewProductModel> result = new List<NewProductModel>();
            // Rise flag that StartWatch method is running.
            _isActive = true;
            
            try
            {
                // Setup the configuration to support document loading. Add css selectors support.
                IConfiguration config = Configuration.Default.WithDefaultLoader().WithCss();
                // Asynchronously get the document in a new context using the configuration
                IDocument document = await BrowsingContext.New(config).OpenAsync(address);
                // Get all products link elements
                IHtmlCollection<IElement> productPageLinkElements = document.QuerySelectorAll("a.product_name");

                result = await GetProductsDescriptions(productPageLinkElements, address, config);
            }
            catch (Exception)
            {
                // Ensure method activity flag is set to false.
                _isActive = false;
                throw;
            }

            _isActive = false;
            return result;
        }

        // This method returns all products information
        private async Task<ICollection<NewProductModel>> GetProductsDescriptions(IHtmlCollection<IElement> startPageElement, string startPageAddress, IConfiguration config)
        {
            Random random = new Random();
            ICollection<NewProductModel> models = new List<NewProductModel>();

            startPageAddress = startPageAddress.Remove(startPageAddress.Length - 1);
            foreach (var productLinkElemment in startPageElement)
            {
                // Every product page is loaded with random delay(1-10 sec) to prevent blocking site access or more important - site crashing.
                Thread.Sleep(random.Next(1000, 10000));
                decimal productPrice = 0;
                string productDescription = null;
                byte[] productImg = null;
                string productName = productLinkElemment.TextContent;
                // Extract product page link from anchor's href attribute
                string link = startPageAddress + productLinkElemment.Attributes.Single(attr => attr.Name == "href")?.Value;
                if (!string.IsNullOrEmpty(link))
                {
                    IDocument productPageElement = await BrowsingContext.New(config).OpenAsync(link);
                    if (productPageElement == null)
                        continue;
                    IElement priceElement = productPageElement.QuerySelector("span.price");
                    string priceString = priceElement?.Attributes.Single(attr => attr.Name == "content")?.Value.Trim();
                    Decimal.TryParse(priceString, out productPrice);
                    IElement descriptionBlock = productPageElement?.QuerySelector("div.description_block_full");
                    productDescription = GetTextFromDomElement(descriptionBlock);
                    string imageLink = productPageElement?.QuerySelector("img.item_img")?.Attributes.Single(attr => attr.Name == "src")?.Value.Trim();
                    productImg = GetImgFromLink(startPageAddress + imageLink);
                }
                models.Add(new NewProductModel()
                {
                    Name = productName,
                    Description = productDescription,
                    Price = productPrice,
                    Image = productImg
                });
            }
            return models;
        }

        // This method returns byte array downloaded from argument link
        private byte[] GetImgFromLink(string imgLink)
        {
            if (imgLink == null)
            {
                return null;
            }
            byte[] result = null;
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    result = webClient.DownloadData(imgLink);
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        // This method gets element with product description and extracts all text from it.
        // Method is hardcoded to parse description from exact site. But all this service is devoted to explore same exact site.
        private string GetTextFromDomElement(IElement elementsBlock)
        {
            if (elementsBlock == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            IHtmlCollection<IElement> elements = elementsBlock.QuerySelectorAll("*");
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element.TagName.ToLower() == "p")
                    {
                        if (!string.IsNullOrEmpty(element.TextContent))
                        {
                            stringBuilder.AppendLine(element.TextContent);
                        }
                    }
                    else if (element.TagName.ToLower() == "ul")
                    {
                        ParseUlElement(element, stringBuilder);
                    }
                    else if (element.TagName.ToLower() == "br")
                    {
                        stringBuilder.AppendLine();
                    }
                }
            }
            string result = stringBuilder.ToString();
            stringBuilder.Clear();
            return result;
        }

        // This method helps to parse ul elements as lists in result string.
        private void ParseUlElement(IElement element, StringBuilder stringBuilder)
        {
            IHtmlCollection<IElement> listItemsElements = element.QuerySelectorAll("li");
            foreach (var listItemElement in listItemsElements)
            {
                if (!string.IsNullOrEmpty(listItemElement.TextContent))
                {
                    stringBuilder.AppendLine($"- {listItemElement.TextContent.Trim()}");
                }
            }
        }

    }
}
