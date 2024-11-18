using Extensions.Modules.Custom.CustomerPartNumbers.Models;
using Extensions.Modules.Custom.CustomerPartNumbers.Services;
using Insite.Core.Extensions;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using Insite.Core.WebApi.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Extensions.WebApi.Controllers.CustomerPartNumbers
{
    [RoutePrefix("api/escommerce/v1/customerpartnumbers")]
    public class CustomerPartNumberController : BaseApiController
    {
        private readonly ICustomerPartNumberService CustomerPartNumberService;
        private readonly IUrlHelper UrlHelper;

        public CustomerPartNumberController(ICookieManager cookieManager,
            ICustomerPartNumberService customerPartNumberService,
            IUrlHelper urlHelper
            ) : base(cookieManager)
        {
            CustomerPartNumberService = customerPartNumberService;
            UrlHelper = urlHelper;
        }

        [Route("{customerPartNumberId}")]
        public HttpResponseMessage Delete([FromUri] string customerPartNumberId)
        {
            if (customerPartNumberId.IsBlank())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            CustomerPartNumberService.Delete(customerPartNumberId);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [Route("", Name = "CustomerPartNumbersV1")]
        [ResponseType(typeof(CustomerPartNumberCollectionModel))]
        public IHttpActionResult Get([FromUri] CustomerPartNumberCollectionParameter parameter)
        {
            if (parameter == null)
            {
                parameter = new CustomerPartNumberCollectionParameter();
            }
            var result = CustomerPartNumberService.GetCustomerPartNumbersWithProduct(parameter);
            result.Uri = UrlHelper.Link("CustomerPartNumbersV1", null, Request);

            if (result.Pagination.Page > 1)
            {
                result.Pagination.PrevPageUri = GetLink(result, Request, result.Pagination.Page - 1);
            }

            if (result.Pagination.Page < result.Pagination.NumberOfPages)
            {
                result.Pagination.PrevPageUri = GetLink(result, Request, result.Pagination.Page + 1);
            }

            return Ok(result);
        }

        [Route("{customerPartNumberId}")]
        [ResponseType(typeof(CustomerPartNumber))]
        public IHttpActionResult Patch([FromUri] string customerPartNumberId, [FromBody] CustomerPartNumber parameter)
        {
            if (parameter == null || customerPartNumberId.IsBlank())
            {
                return BadRequest("Bad Request");
            }

            var result = CustomerPartNumberService.Update(customerPartNumberId, parameter);

            return Ok(result);
        }

        [Route("")]
        [ResponseType(typeof(CustomerPartNumber))]
        public IHttpActionResult Post([FromBody] CreateCustomerPartNumberParameter parameter)
        {
            if (parameter == null)
            {
                return BadRequest("Bad Request");
            }

            var result = CustomerPartNumberService.Create(parameter);

            return Ok(result);
        }

        protected virtual string GetLink(CustomerPartNumberCollectionModel serviceResult, HttpRequestMessage request, int page)
        {
            var routeValues = new
            {
                sort = request.GetQueryString("sort"),
                pagesize = serviceResult.Pagination.PageSize,
                page
            };
            return UrlHelper.Link("CustomerPartNumbersV1", routeValues, request);
        }
    }
}