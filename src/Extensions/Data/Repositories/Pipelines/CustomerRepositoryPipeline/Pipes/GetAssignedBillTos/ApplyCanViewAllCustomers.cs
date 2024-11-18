namespace Extensions.Data.Repositories.Pipelines.CustomerRepositoryPipeline.Pipes.GetAssignedBillTos
{
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Plugins.Pipelines;
    using Insite.Data.Entities;
    using Insite.Data.Repositories.Pipelines.CustomerRepositoryPipeline.Parameters;
    using Insite.Data.Repositories.Pipelines.CustomerRepositoryPipeline.Results;

    public class ApplyCanViewAllCustomers : IPipe<GetAssignedBillTosParameter, GetAssignedBillTosResult>
    {
        public int Order => 450;

        public GetAssignedBillTosResult Execute(
            IUnitOfWork unitOfWork,
            GetAssignedBillTosParameter parameter,
            GetAssignedBillTosResult result
        )
        {
            var canViewAllCustomersValue = unitOfWork
                .GetRepository<UserProfile>()
                .Get(parameter.UserProfileId)?
                .GetProperty("escommerce_canViewAllCustomers", bool.FalseString);
            if (bool.TryParse(canViewAllCustomersValue, out bool canViewAllCustomers) && canViewAllCustomers)
            {
                result.ExitPipeline = true;
                return result;
            }

            return result;
        }
    }
}