using Extensions.CustomSettings;
using Extensions.Modules.Custom.SerialNumbers.Models;
using Insite.Common.Helpers;
using Insite.Common.HttpUtilities;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Plugins.Caching;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using static IdentityServer3.Core.Constants;

namespace Extensions.Modules.Custom.SerialNumbers.Services
{
    public class ESCSerialNumberApiService : IESCSerialNumberApiService
    {
        private readonly string AccessTokenCacheKey;
        private readonly ICacheManager CacheManager;
        private readonly HttpClient HttpClient;
        private readonly SerialNumberApiSettings SerialNumberSettings;

        public ESCSerialNumberApiService(ICacheManager cacheManager, SerialNumberApiSettings serialNumberSettings, HttpClientProvider httpClientProvider)
        {
            CacheManager = cacheManager;
            SerialNumberSettings = serialNumberSettings;
            HttpClient = httpClientProvider.GetHttpClient(new Uri($"{SerialNumberSettings.SerialNumberApiUrl}"));
            AccessTokenCacheKey = $"SerialNumber_{SerialNumberSettings.SerialNumberApiTokenUrl}_AccessToken";
        }

        public SerialNumberApiResponse GetInvoicesBySerialNumber(SerialNumberApiRequest apiRequest)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
            HttpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            HttpResponseMessage httpResponse = MakeSerialNumberAPIRequest(apiRequest);

            if (httpResponse.Content != null && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                try
                {
                    LogHelper.For(this).Debug("Parsing serialNumberResponse");
                    var serialNumberApiResponse = httpResponse.Content.ReadAsAsync<SerialNumberApiResponse>().Result;
                    LogHelper.For(this).Debug($"Serial number API response: {serialNumberApiResponse}");
                    httpResponse?.EnsureSuccessStatusCode();
                    return serialNumberApiResponse;
                }
                catch (Exception ex)
                {
                    LogHelper.For(this).Error(ex, "Invalid JSON");
                    throw new Exception("Invalid JSON", ex);
                }
            }
            else
            {
                LogHelper.For(this).Error("Invalid SerialNumberAPI Response");
                throw new Exception("Invalid SerialNumberAPI Response");
            }
        }

        private static (string accessToken, TimeSpan tokenExpiration) ParseAccessTokenAndExpiration(string response)
        {
            try
            {
                var responseObject = JObject.Parse(response);
                var accessToken = responseObject["access_token"].ToString();
                var tokenExpiration = TimeSpan.FromSeconds(responseObject["expires_in"].ToObject<int>());

                return (accessToken, tokenExpiration);
            }
            catch
            {
                throw new Exception($"Error parsing access_token and expires_in from response: {response}");
            }
        }

        private string GetAccessToken()
        {
            ValidateSerialNumberSettings();

            if (TryGetCachedToken(out var cachedToken))
            {
                LogHelper.For(this).Debug("Token grabbed from cache");
                return cachedToken;
            }

            HttpResponseMessage tokenResponse;
            string returnValue;
            try
            {
                tokenResponse = HttpClient.PostAsync($"{SerialNumberSettings.SerialNumberApiTokenUrl}", GetApiTokenArgs()).Result;
                returnValue = tokenResponse.Content.ReadAsStringAsync().Result;
                LogHelper.For(this).Debug($"Serial number API Token response: {returnValue}");
            }
            catch (Exception ex)
            {
                LogHelper.For(this).Error(ex, "Error retrieving access token");
                throw new Exception($"Exception making HTTP request to {SerialNumberSettings.SerialNumberApiTokenUrl}", ex);
            }

            tokenResponse.EnsureSuccessStatusCode();

            var (accessToken, expiration) = ParseAccessTokenAndExpiration(returnValue);

            CacheManager.Add(AccessTokenCacheKey, EncryptionHelper.EncryptAes(accessToken), expiration);

            return accessToken;
        }

        private FormUrlEncodedContent GetApiTokenArgs()
        {
            return new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", SerialNumberSettings.ClientId),
                new KeyValuePair<string, string>("client_secret", SerialNumberSettings.ClientSecret)
            });
        }

        private HttpResponseMessage MakeSerialNumberAPIRequest(SerialNumberApiRequest apiRequest)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/invoices/find")
            {
                Content = new FormUrlEncodedContent(ObjectToDictionary(apiRequest))
            };

            HttpResponseMessage httpResponse;

            try
            {
                httpResponse = HttpClient.SendAsync(httpRequest).Result;
            }
            catch (Exception ex)
            {
                LogHelper.For(this).Error(ex, "Error making HTTP call");
                throw new Exception("Error making HTTP call", ex);
            }

            return httpResponse;
        }

        private Dictionary<string, string> ObjectToDictionary(object data)
        {
            return data.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(data)?.ToString() ?? "");
        }

        private bool TryGetCachedToken(out string cachedToken)
        {
            cachedToken = string.Empty;

            if (CacheManager.Contains(AccessTokenCacheKey))
            {
                cachedToken = EncryptionHelper.DecryptAes(CacheManager.Get<string>(AccessTokenCacheKey));
                return true;
            }

            return false;
        }

        private void ValidateSerialNumberSettings()
        {
            if (string.IsNullOrWhiteSpace(SerialNumberSettings.ClientSecret))
            {
                throw new Exception("Serial Number settings must be set in the admin console.");
            }
        }
    }
}