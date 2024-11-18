using Extensions.CustomSettings;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Extensions.WebApi.Controllers.CustomRma
{
    [RoutePrefix("api/v1/CustomRma")]
    public class CustomRmaController : BaseApiController
    {
        private IUnitOfWork unitOfWork;
        private RmaIntegrationSettings rmaIntegrationSettings;

        public CustomRmaController(ICookieManager cookieManager, RmaIntegrationSettings rmaIntegrationSettings) : base(cookieManager)
        {
            this.rmaIntegrationSettings = rmaIntegrationSettings;
        }

        [HttpGet, Route("GetWarrantyUrl")]
        public HttpResponseMessage GetWarrantyUrl()
        {
            var warehouseName = SiteContext.Current.BillTo.DefaultWarehouse?.State;
            var warrantyUrl = new WarrantyUrl();
            if (warehouseName == "MN")
            {
                warrantyUrl.WarrantyUrlValue = rmaIntegrationSettings.WarrantyMnUrl;
            }
            else if (warehouseName == "WI")
            {
                warrantyUrl.WarrantyUrlValue = rmaIntegrationSettings.WarrantyWiUrl;
            }


            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(warrantyUrl.ToJson(), Encoding.UTF8, "application/json") };


            return result;
        }

        public class WarrantyUrl
        {
            public string WarrantyUrlValue { get; set; }
        }
    }
}