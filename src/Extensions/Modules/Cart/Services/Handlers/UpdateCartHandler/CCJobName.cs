using Insite.Cart.Services.Parameters;
using Insite.Cart.Services.Results;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Services.Handlers;
using Insite.Data.Entities;
using System;
using System.Linq;

namespace Extensions.Modules.Cart.Services.Handlers.UpdateCartHandler
{
    [DependencyName(nameof(ESCJobName))]
    public class ESCJobName : HandlerBase<UpdateCartParameter, UpdateCartResult>
    {
        public override int Order => 2855;

        public override UpdateCartResult Execute(IUnitOfWork unitOfWork, UpdateCartParameter parameter, UpdateCartResult result)
        {
            if (result == null || result?.GetCartResult?.Cart?.Id == null)
            {
                return NextHandler.Execute(unitOfWork, parameter, result);
            }

            if (!parameter.Status.EqualsIgnoreCase(CustomerOrder.StatusType.Submitted))
            {
                return NextHandler.Execute(unitOfWork, parameter, result);
            }

            if (parameter.Properties.ContainsKey(Constants.JobNameCustomerOrder))
            {
                var jobNameValue = string.Empty;
                parameter.Properties.TryGetValue(Constants.JobNameCustomerOrder, out jobNameValue);

                if (jobNameValue == string.Empty)
                {
                    return NextHandler.Execute(unitOfWork, parameter, result);
                }

                var customPropertyRepo = unitOfWork.GetRepository<CustomProperty>();
                var existingJobName = customPropertyRepo.GetTable().Where(c => c.ParentTable == "customerOrder" && c.Name == Constants.JobNameCustomerOrder && c.ParentId == result.GetCartResult.Cart.Id).FirstOrDefault();

                if (existingJobName == null)
                {
                    var jobNameCustomProperty = new CustomProperty
                    {
                        ParentId = result.GetCartResult.Cart.Id,
                        Name = Constants.JobNameCustomerOrder,
                        Value = jobNameValue,
                        ParentTable = "customerOrder",
                    };
                    customPropertyRepo.Insert(jobNameCustomProperty);
                }
                else
                {
                    existingJobName.Value = jobNameValue;
                }
                unitOfWork.Save();
            }
            return NextHandler.Execute(unitOfWork, parameter, result);
        }
    }
}
