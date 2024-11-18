using Extensions.CustomSettings;
using Extensions.Integration.UnifiedAR.Services.UnifiedARSSOService;
using Insite.Common.Logging;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace Extensions.WebApi.Controllers.UnifiedAR
{
    [Authorize]
    [RoutePrefix("api/v1/PayNowSession")]
    public class PayNowSessionController : BaseApiController
    {
        private readonly UnifiedARSettings PayNowSettings;
        private readonly IUnifiedARSSOService unifiedARSSOService;

        public PayNowSessionController(ICookieManager cookieManager, UnifiedARSettings unifiedARSettings, IUnifiedARSSOService unifiedARSSOService) : base(cookieManager)
        {
            PayNowSettings = unifiedARSettings;
            this.unifiedARSSOService = unifiedARSSOService;
        }

        [HttpPost]
        [Route("SSO")]
        [ResponseType(typeof(OkResult))]
        public Task<HttpResponseMessage> SSOToken(string customerNumber, string callbackUrl)
        {
            string key = PayNowSettings.UARPrivateKey;
            string merchantKey = PayNowSettings.MerchantKey;
            string payNowKey = PayNowSettings.PayNowKey;
            string uarUrl = PayNowSettings.UARUrl;
            var requestURL = $"https://{uarUrl}/Merchant/{merchantKey}/PayNowSession";
            var publicRedirectUrl = "https://www.gounified.com";

            try
            {
                return unifiedARSSOService.GetSSOToken(requestURL, customerNumber, payNowKey, publicRedirectUrl, callbackUrl, key);
            }
            catch (Exception ex)
            {
                LogHelper.For(this)
                    .Error("Error occurred while processing Payment Portal request: " + ex.Message);
                return Task<HttpResponseMessage>.Factory.StartNew(() =>
                        Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }
    }
}
