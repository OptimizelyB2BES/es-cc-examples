using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using Insite.Data.Entities;
using System;
using System.Net.Http;
using System.Web.Http;

namespace Extensions.WebApi.Controllers.ReplacementProducts
{
    [RoutePrefix("api/v1/ProductReplacementProducts")]
    public class ProductReplacementProductsController : BaseApiController
    {
        private IUnitOfWork unitOfWork;

        public ProductReplacementProductsController(ICookieManager cookieManager, IUnitOfWorkFactory unitOfWorkFactory) : base(cookieManager)
        {
            unitOfWork = unitOfWorkFactory.GetUnitOfWork();
        }

        [HttpGet, Route("GetReplacementProducts")]
        public HttpResponseMessage GetSubscriptionProducts(string productId)
        {
            var parseResult = Guid.TryParse(productId, out Guid productIdParsed);
            if (!parseResult)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent("GUID not in a valid format.") };
            }

            var product = unitOfWork.GetRepository<Product>().Get(productIdParsed);
            if (product == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent($"Product having ID {productId} does not exist.") };
            }

            var replacement = product.ReplacementProduct;
            if (replacement == null) return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("") };

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent($"{replacement.ErpNumber}") };

        }
    }
}