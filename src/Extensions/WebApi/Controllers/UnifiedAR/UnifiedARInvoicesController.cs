using Extensions.CustomSettings;
using Extensions.Integration.UnifiedAR.Services.UnifiedARInvoiceService;
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
    [RoutePrefix("api/v1/UnifiedARInvoice")]
    public class UnifiedARInvoicesController : BaseApiController
    {
        private readonly UnifiedARInvoiceSettings InvoiceSettings;
        private readonly IUnifiedARInvoiceService unifiedARInvoiceService;

        public UnifiedARInvoicesController(ICookieManager cookieManager, UnifiedARInvoiceSettings unifiedARSettings, IUnifiedARInvoiceService unifiedARInvoiceService) : base(cookieManager)
        {
            InvoiceSettings = unifiedARSettings;
            this.unifiedARInvoiceService = unifiedARInvoiceService;
        }

        [HttpGet]
        [Route("Invoice")]
        [ResponseType(typeof(OkResult))]
        public Task<HttpResponseMessage> Invoices(string invoiceNumber)
        {
            string key = InvoiceSettings.UARPrivateKey;
            string merchantKey = InvoiceSettings.MerchantKey;
            string uarUrl = InvoiceSettings.UARUrl;
            string invoiceURL = InvoiceSettings.InvoiceURL;
            var requestURL = $"https://{uarUrl}/Invoice/?merchantKey={merchantKey}&limit=25&offset=0&invoiceNumber={invoiceNumber}&includeDisputed=true&includeCredits=true&includeZeroDue=true&includeUnapproved=true";

            try
            {
                var response = unifiedARInvoiceService.GetInvoices(requestURL, invoiceNumber, merchantKey, key, invoiceURL, Request);
                return response;
            }
            catch (Exception ex)
            {
                LogHelper.For(this)
                    .Error("Error occurred while processing invoice request: " + ex.Message);
                return Task<HttpResponseMessage>.Factory.StartNew(() =>
                        Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }
    }
}
