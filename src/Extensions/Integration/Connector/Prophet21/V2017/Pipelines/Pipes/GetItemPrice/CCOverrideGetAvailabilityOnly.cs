namespace Extensions.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice
{
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Plugins.Pipelines;
    using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
    using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

    //Customization to account for future configuration options
    //Overrides logic for GetAvailabilityOnly value originally set in CreateInitialRequest in this pipeline (order => 100)
    public class ESCOverrideGetAvailabilityOnly : IPipe<GetItemPriceParameter, GetItemPriceResult>
    {
        public int Order => 115;

        public GetItemPriceResult Execute(IUnitOfWork unitOfWork, GetItemPriceParameter parameter, GetItemPriceResult result)
        {
            result.GetItemPriceRequest.Request.GetAvailabilityOnly = "FALSE";

            return result;
        }
    }
}