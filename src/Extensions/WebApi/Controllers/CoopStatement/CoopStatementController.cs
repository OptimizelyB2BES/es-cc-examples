using Extensions.CustomSettings;
using Extensions.WebApi.Controllers.Models;
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
    [RoutePrefix("api/escommerce/v1/coopstatement")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CoopStatementController : BaseApiController
    {
        private const string tokenEndpointUrl = "https://erp.escommerce.com:5443/Token";
        private const string coopStatementEndpointUrl = "https://erp.escommerce.com:5443/api/UnilogGetPDF/1";
        private readonly CoopStatementSettings coopStatementSettings;

        private readonly HttpClient httpClient;
        
        public CoopStatementController(ICookieManager cookieManager, HttpClient httpClient, CoopStatementSettings coopStatementSettings) 
            : base(cookieManager)
        {
            this.httpClient = httpClient;
            this.coopStatementSettings = coopStatementSettings;
        }

        [Route("pdf")]
        public async Task<HttpResponseMessage> Get()
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
                    return await Task.FromResult(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }

                var customerNumber = SiteContext.Current.BillTo?.CustomerNumber;

                var pdfResponseMessage = await this.GetCoopPdf(this.httpClient, customerNumber, accessToken.AccessTokenResult);

                return await Task.FromResult(pdfResponseMessage);
                
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Request.CreateResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        private async Task<HttpResponseMessage> GetCoopPdf(HttpClient httpClient, string customerNumber, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var pdfResponse = await httpClient.GetAsync
            (
                $"{coopStatementEndpointUrl}/{customerNumber}"
            );

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

            if (string.IsNullOrEmpty(returnValue))
            {
                return null;
            }

            var accessToken = JsonConvert.DeserializeObject<AccessToken>(returnValue);

            return await Task.FromResult(accessToken);
        }
    }
}

