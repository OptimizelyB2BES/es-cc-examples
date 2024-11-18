using Extensions.Integration.Prophet21.ApiService.Models;
using Extensions.Modules.Account.Services.Handlers.AddAccountHandler;
using Insite.Common.Helpers;
using Insite.Common.Logging;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Extensions.Integration.Prophet21.ApiService
{
    public class ESCProphet21ApiService : IESCProphet21ApiService
    {
        private const string ContactsApiPath = "/api/entity/contacts/";
        private const string TokenApiPath = "/api/security/token/";
        private const string TransactionApiPath = "/uiserver0/api/v2/transaction";
        private const string ECommerceApiPath = "/api/ecommerce/";

        public Contact CreateContact(IntegrationConnection integrationConnection, ESCContact contact)
        {
            var xmlRequest = Prophet21SerializationService.Serialize(contact);
            LogHelper.For(this).Debug($"Create Contact Request: {xmlRequest}");

            var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
            var xmlResponse = CallMiddlewareApi(integrationConnection, HttpMethod.Post, ContactsApiPath, requestContent);
            LogHelper.For(this).Debug($"Create Contact Response: {xmlResponse}");

            return Prophet21SerializationService.Deserialize<Contact>(xmlResponse);
        }

        public TransactionSetResult PostTransaction(IntegrationConnection integrationConnection, TransactionSet transactionSet)
        {
            var xmlRequest = Prophet21SerializationService.Serialize(transactionSet);
            LogHelper.For(this).Debug($"Post Transaction Request: {xmlRequest}");

            var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
            var xmlResponse = CallMiddlewareApi(integrationConnection, HttpMethod.Post, TransactionApiPath, requestContent);
            LogHelper.For(this).Debug($"Post Transaction Response: {xmlResponse}");

            return Prophet21SerializationService.Deserialize<TransactionSetResult>(xmlResponse);
        }

        private static string GetRequestUri(IDictionary<string, string> parameters)
        {
            return parameters != null && parameters.Any() ? $"?{string.Join("&", parameters.Select(o => $"{o.Key}={o.Value}"))}" : string.Empty;
        }

        public OrderImport OrderImport(
            IntegrationConnection integrationConnection,
            OrderImport orderImport,
            string jobName
            )
        {
            var xmlRequest = Prophet21SerializationService.Serialize(orderImport);

            // AST-711 all order import requests need to include the "autoapproval" value, defaulting to "N".
            xmlRequest = AddAutoApprovalAndJobNameValueToRequest(xmlRequest, jobName);

            LogHelper.For(this).Debug($"OrderImport Request: {xmlRequest}");

            var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
            var xmlResponse = CallMiddlewareApi(
                integrationConnection,
                HttpMethod.Post,
                ECommerceApiPath,
                requestContent
            );

            LogHelper.For(this).Debug($"OrderImport Response: {xmlResponse}");

            return Prophet21SerializationService.Deserialize<OrderImport>(xmlResponse);
        }

        private string AddAutoApprovalAndJobNameValueToRequest(string xmlRequest, string jobName)
        {
            try
            {
                LogHelper.For(this).Debug($"AddAutoApprovalAndJobNameValueToRequest started");

                var orderImportElement = XElement.Parse(xmlRequest);
                var lineItemsElements = orderImportElement.Elements()
                    .Where(e => e.Name == "Request")
                    .FirstOrDefault();

                if (lineItemsElements != null)
                {
                    XElement autoApproval = null;

                    if (jobName == string.Empty)
                    {
                        autoApproval =
                       new XElement("ListOfOptionalElements",
                           new XElement("OptionalElement",
                               new XElement("id", "approved"),
                               new XElement("value", "N")
                           )
                       );
                    }
                    else
                    {
                        autoApproval =
                           new XElement("ListOfOptionalElements",
                               new XElement("OptionalElement",
                                   new XElement("id", "approved"),
                                   new XElement("value", "N")
                               ), new XElement("OptionalElement",
                                   new XElement("id", "JobName"),
                                   new XElement("value", jobName)
                               )
                           );
                    }
                    lineItemsElements.Add(autoApproval);
                }

                var result = orderImportElement.ToString();

                LogHelper.For(this).Debug($"AddAutoApprovalAndJobNameValueToRequest finished: {result}");

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.For(this).Debug($"OrderImport: Set AutoApproval field failed.", ex);
            }

            return xmlRequest;
        }

        private string CallMiddlewareApi(
            IntegrationConnection integrationConnection,
            HttpMethod httpMethod,
            string path,
            HttpContent content,
            IDictionary<string, string> parameters = null,
            bool shouldSetAuthorizationHeader = true)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(integrationConnection.Url + path);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                if (shouldSetAuthorizationHeader)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken(integrationConnection));
                }

                var requestUri = GetRequestUri(parameters);

                var httpResponse = httpMethod == HttpMethod.Get
                    ? httpClient.GetAsync(requestUri).Result
                    : httpClient.PostAsync(requestUri, content).Result;

                if (httpResponse == null)
                {
                    throw new DataException($"Error calling Prophet 21 API. Response is null.");
                }

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new DataException(
                        $"Error calling Prophet 21 API. Status Code: {httpResponse.StatusCode}. Reason Phrase: {httpResponse.ReasonPhrase}. " +
                        $"Additional Info: {httpResponse.Content.ReadAsStringAsync().Result}");
                }

                return httpResponse.Content.ReadAsStringAsync().Result;
            }
        }

        private string GetToken(IntegrationConnection integrationConnection)
        {
            if (integrationConnection.TypeName.Trim().EqualsIgnoreCase("ApiClientCredentialsEndpoint"))
            {
                var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "username", integrationConnection.LogOn },
                        { "client_secret", EncryptionHelper.DecryptAes(integrationConnection.Password) },
                        { "client-type", "application/xml" }
                    });

                var xmlResponse = CallMiddlewareApi(integrationConnection, HttpMethod.Post, TokenApiPath, requestContent, null, false);

                return Prophet21SerializationService.Deserialize<Token>(xmlResponse).AccessToken;
            }
            else
            {
                var parameters = new Dictionary<string, string>
                    {
                        { "username", integrationConnection.LogOn },
                        { "password", HttpUtility.UrlEncode(EncryptionHelper.DecryptAes(integrationConnection.Password)) }
                    };

                return CallMiddlewareApi(integrationConnection, HttpMethod.Post, TokenApiPath, null, parameters, false);
            }
        }
    }
}