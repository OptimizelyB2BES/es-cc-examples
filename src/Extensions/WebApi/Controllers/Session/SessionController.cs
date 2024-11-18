using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Utilities;
using Insite.Core.WebApi;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http;
using Insite.Core.Context;
using Insite.Data.Entities.Dtos;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Common.Logging;
using Insite.Account.Services;

namespace Extensions.WebApi.Controllers.Session
{
    [RoutePrefix("api/escommerce/v1/session")]
    [Authorize]
    public class SessionController : BaseApiController
    {
        private IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        public SessionController(
            ICookieManager cookieManager,
            ISessionService sessionService,
            IUnitOfWorkFactory unitOfWorkFactory) : base(cookieManager)
        {
            _unitOfWork = unitOfWorkFactory.GetUnitOfWork();
            _sessionService = sessionService;
        }
        [Route("resetDefaultCustomers")]
        [ResponseType(typeof(HttpResponseMessage))]
        [HttpPost]
        public IHttpActionResult ResetDefaultCustomers()
        {
            ICustomerRepository typedRepository = _unitOfWork.GetTypedRepository<ICustomerRepository>();
            WebsiteDto websiteDto = SiteContext.Current.WebsiteDto;

            if (websiteDto == null)
            {
                LogHelper.For(this).Error("Current Website not found.");
                return BadRequest("Current Website not found.");
            }

            Customer defaultBillTo = typedRepository.GetDefaultBillTo(websiteDto, SiteContext.Current.UserProfileDto.Id);
            Customer defaultShipTo = typedRepository.GetDefaultShipTo(defaultBillTo, SiteContext.Current.UserProfileDto.Id, websiteDto.Id);

            if (defaultBillTo != null && defaultShipTo != null)
            {
                var updateSessionResult = _sessionService.UpdateSession(new Insite.Account.Services.Parameters.UpdateSessionParameter
                {
                    BillToId = defaultBillTo.Id,
                    ShipToId = defaultShipTo.Id,
                });

                if (updateSessionResult.ResultCode == Insite.Core.Services.ResultCode.Success)
                {
                    return Ok();
                }
                else
                {
                    LogHelper.For(this).Error("Semething Went Wrong.");
                    return BadRequest("Semething Went Wrong.");
                }
            }
            else
            {
                LogHelper.For(this).Error("Unable to fetch default BillTo Or ShipTo.");
                return BadRequest("Unable to fetch default BillTo Or ShipTo.");
            }
        }
    }
}
