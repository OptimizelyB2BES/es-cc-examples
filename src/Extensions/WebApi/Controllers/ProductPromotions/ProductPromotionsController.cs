using Insite.Common.DynamicLinq;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using Insite.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Extensions.WebApi.Controllers.ProductPromotions
{
    /// <summary>
    /// AST-43
    /// </summary>
    [RoutePrefix("api/v1/ProductPromotions")]
    public class ProductPromotionsController : BaseApiController
    {
        private IUnitOfWork unitOfWork;

        public ProductPromotionsController(ICookieManager cookieManager, IUnitOfWorkFactory unitOfWorkFactory) : base(cookieManager)
        {
            unitOfWork = unitOfWorkFactory.GetUnitOfWork();
        }

        [HttpGet, Route("GetProductPromotions")]
        public HttpResponseMessage GetProductPromotions(string productId)
        {
            var parseResult = Guid.TryParse(productId, out Guid productIdParsed);
            if (!parseResult)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("GUID not in a valid format.")
                };
            }

            var promotionsObj = new PromotionsObject();
            promotionsObj.promotions = new List<PromoViewModel>();

            try
            {
                var product = unitOfWork.GetRepository<Product>().Get(productIdParsed);

                var promotions = unitOfWork.GetRepository<PromotionResult>()
                                           .GetTable()
                                           .Where(o => o.ProductId == productIdParsed)
                                           .ToList();

                var promotionRules = unitOfWork.GetRepository<Promotion>()
                                               .GetTable()
                                               .Where(o => o.RuleManager.RuleClauses.Any(
                                                   p => p.CriteriaType == "OrderedProduct" &&
                                                   p.SimpleValue == productId ||
                                                   product.BrandId != null && p.SimpleValue == product.BrandId.ToString()));

                foreach (var category in product.Categories)
                {
                    var promotionRulesCategory = unitOfWork.GetRepository<Promotion>()
                                                           .GetTable()
                                                           .Where(o => o.RuleManager.RuleClauses.Any(
                                                               p => p.CriteriaType == "OrderedProductCategory" &&
                                                               p.SimpleValue == category.Id.ToString()));

                    foreach (var promotion in promotionRulesCategory)
                    {
                        SetIsLiveOnPromotion(promotion);

                        if (promotion.IsLive)
                        {
                            promotionsObj.promotions.Add(new PromoViewModel(promotion.Name, promotion.Description, category.UrlSegment));
                        }
                    }
                }

                foreach (var promotion in promotions)
                {
                    SetIsLiveOnPromotion(promotion.Promotion);

                    if (promotion.Promotion.IsLive)
                    {
                        promotionsObj.promotions.Add(new PromoViewModel(promotion.Promotion.Name, promotion.Promotion.Description));
                    }
                }

                foreach (var promotion in promotionRules)
                {
                    SetIsLiveOnPromotion(promotion);

                    if (promotion.IsLive)
                    {
                        promotionsObj.promotions.Add(new PromoViewModel(promotion.Name, promotion.Description));
                    }
                }

                // B2BES [AST-438] - Modify Customization - Base promotion.IsLive doesn't reflect the deactivate date of the promotion
                unitOfWork.Save();

                return Request.CreateResponse(HttpStatusCode.OK, promotionsObj.ToJson());
            }
            catch (Exception ex)
            {
                LogHelper.For(this).Error("Error retrieving promotions for product");
                return Request.CreateResponse(HttpStatusCode.OK, promotionsObj.ToJson());
            }
        }

        private void SetIsLiveOnPromotion(Promotion promotion)
        {
            if (DateTimeOffset.Compare(promotion.ActivateOn, DateTimeOffset.Now) >= 0 ||
                DateTimeOffset.Compare(promotion.DeactivateOn ?? DateTimeOffset.MaxValue, DateTimeOffset.Now) < 0)
            {
                promotion.IsLive = false;
            }
            else
            {
                promotion.IsLive = true;
            }
        }

        private class PromotionsObject
        {
            public List<PromoViewModel> promotions { get; set; }
        }

        private class PromoViewModel
        {
            public PromoViewModel(string name, string description, string url = null)
            {
                this.url = url;
                this.name = name;
                this.description = description;
            }

#pragma warning disable IDE1006 // Naming Styles
            public string url { get; set; }
            public string name { get; set; }
            public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
    }
}