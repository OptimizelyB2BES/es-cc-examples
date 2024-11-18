using Extensions.CustomSettings;
using Extensions.WebApi.Controllers.Models;
using Insite.Common.Logging;
using Insite.Core.Context;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Extensions.WebApi.Controllers.CoopStatement
{
    [RoutePrefix("api/escommerce/v1/invoicepdf")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InvoicePdfController : BaseApiController
    {
        private const string tokenEndpointUrl = "https://erp.escommerce.com:5443/Token";
        private const string coopStatementEndpointUrl = "https://erp.escommerce.com:5443/api/GetInvoicePDF";
        private readonly CoopStatementSettings coopStatementSettings;

        private readonly HttpClient httpClient;
        
        public InvoicePdfController(ICookieManager cookieManager, HttpClient httpClient, CoopStatementSettings coopStatementSettings) 
            : base(cookieManager)
        {
            this.httpClient = httpClient;
            this.coopStatementSettings = coopStatementSettings;
        }

        [Route("pdf")]
        public async Task<HttpResponseMessage> Get(string invoiceNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(SiteContext.Current.BillTo?.CustomerNumber))
                {
                    return await Task.FromResult(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }

                var accessToken = await this.GetAccessToken(this.httpClient);

                if (accessToken == null || string.IsNullOrEmpty(accessToken.AccessTokenResult))
                {
                    LogHelper.For(this).Error($"Error fetching invoice access token.");
                    return await Task.FromResult(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }

                var pdfResponseMessage = await this.GetCoopPdf(this.httpClient, invoiceNumber, accessToken.AccessTokenResult);

                return await Task.FromResult(pdfResponseMessage);
                
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Request.CreateResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        private async Task<HttpResponseMessage> GetCoopPdf(HttpClient httpClient, string invoiceNumber, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var pdfResponse = await httpClient.GetAsync
            (
                $"{coopStatementEndpointUrl}/{invoiceNumber}"
            );

            LogHelper.For(this).Debug($"Invoice PDF API response code: {pdfResponse?.StatusCode} | Invoice number: {invoiceNumber}");

            return await Task.FromResult(pdfResponse);
        }

        private async Task<AccessToken> GetAccessToken(HttpClient httpClient)
        {
            var tokenResponse = await httpClient.PostAsync
                (
                    tokenEndpointUrl,
                    new FormUrlEncodedContent
                    (
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("username", coopStatementSettings.CoopStatementUserName),
                            new KeyValuePair<string, string>("password", coopStatementSettings.CoopStatementPassword)
                        }
                    )
                );

            var returnValue = tokenResponse.Content.ReadAsStringAsync().Result;

            LogHelper.For(this).Debug($"Access token API response: {returnValue})");

            if (string.IsNullOrEmpty(returnValue))
            {
                return null;
            }

            var accessToken = JsonConvert.DeserializeObject<AccessToken>(returnValue);

            return await Task.FromResult(accessToken);
        }
    }
}

