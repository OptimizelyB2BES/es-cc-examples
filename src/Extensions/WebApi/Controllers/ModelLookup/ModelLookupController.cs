using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using Insite.Core.Interfaces.Data;
using Extensions.Modules.Custom.ModelLookup.Services;
using Extensions.CustomSettings;
using Extensions.WebApi.Controllers.ModelLookup.ModelLookup;
using Extensions.WebApi.Controllers.Models;

namespace Extensions.WebApi.Controllers.ModelLookup
{
    [RoutePrefix("api/escommerce/v1/modellookup")]
    public class ModelLookupController : BaseApiController
    {
        private readonly ModelLookupSettings epicApiSettings;
        private readonly HttpClient httpClient;
        private readonly IUnitOfWork unitOfWork;
        private readonly ModelLookupSoapService modelLookupSoapService;

        public ModelLookupController(ICookieManager cookieManager, HttpClient httpClient, ModelLookupSettings epicApiSettings, IUnitOfWorkFactory unitOfWorkFactory, ModelLookupSoapService modelLookupSoapService) 
            : base(cookieManager)
        {
            this.httpClient = httpClient;
            this.epicApiSettings = epicApiSettings;
            this.unitOfWork = unitOfWorkFactory.GetUnitOfWork();
            this.modelLookupSoapService = modelLookupSoapService;
        }

        [Route("model/{searchTerms}")]
        [HttpGet]
        public async Task<ModelLookupResponseModel> LookupModel(string searchTerms)
        {            
            try
            {
                if (string.IsNullOrEmpty(searchTerms))
                {
                    return null;
                }

                var accessToken = await this.GetAccessToken(this.httpClient);

                if (accessToken == null || string.IsNullOrEmpty(accessToken.AccessTokenResult))
                {
                    return null;
                }

                var modelResults = this.modelLookupSoapService.InvokeService(accessToken, searchTerms);

                var lookupResponseMessage = await this.LookupModelNumber(this.httpClient, searchTerms, accessToken.AccessTokenResult);

                return await Task.FromResult(lookupResponseMessage);                
            }
            catch (Exception)
            {
                return null;
            }
        }

        [Route("parts/{searchTerms}")]
        [HttpGet]
        public async Task<List<ListModelResultModel>> LookupPartsModel(string searchTerms)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerms))
                {
                    return null;
                }

                var accessToken = await this.GetAccessToken(this.httpClient);

                if (accessToken == null || string.IsNullOrEmpty(accessToken.AccessTokenResult))
                {
                    return null;
                }

                var modelResults = this.modelLookupSoapService.InvokeService(accessToken, searchTerms);

                return await Task.FromResult(modelResults);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<ModelLookupResponseModel> LookupModelNumber(HttpClient httpClient, string searchTerms, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var fullRequestUri = string.Empty;

            if (this.epicApiSettings.ModelLookupUrl.EndsWith("/"))
            {
                fullRequestUri = $"{this.epicApiSettings.ModelLookupUrl}{searchTerms}";
            }
            else
            {
                fullRequestUri = $"{this.epicApiSettings.ModelLookupUrl}/{searchTerms}";
            }

            var response = await httpClient.GetStringAsync
            (
                fullRequestUri
            );

            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            var lookupModel = JsonConvert.DeserializeObject<ModelLookupResponseModel>(response);

            return await Task.FromResult(lookupModel);
        }

        private async Task<AccessToken> GetAccessToken(HttpClient httpClient)
        {
            var authBytes = Encoding.ASCII.GetBytes($"{this.epicApiSettings.TokenAuthorizationUsername}:{this.epicApiSettings.TokenAuthorizationPassword}");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var tokenResponse = await httpClient.PostAsync
                (
                    this.epicApiSettings.TokenRequestUrl,
                    new FormUrlEncodedContent
                    (
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("username", this.epicApiSettings.TokenRequestUsername),
                            new KeyValuePair<string, string>("password", this.epicApiSettings.TokenRequestPassword)
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

