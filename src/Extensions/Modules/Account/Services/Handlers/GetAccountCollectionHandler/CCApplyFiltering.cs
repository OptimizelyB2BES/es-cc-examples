namespace Extensions.Modules.Account.Services.Handlers.GetAccountCollectionHandler
{
    using System;
    using System.Linq;
    using Insite.Account.Services.Handlers.GetAccountCollectionHandler;
    using Insite.Account.Services.Parameters;
    using Insite.Account.Services.Results;
    using Insite.Common.Extensions;
    using Insite.Core.Context;
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Interfaces.Dependency;
    using Insite.Core.Services.Handlers;
    using Insite.Data.Entities;
    using Insite.Data.Extensions;

    [DependencyName(nameof(ApplyFiltering))]
    public sealed class ESCApplyFiltering
        : HandlerBase<GetAccountCollectionParameter, GetAccountCollectionResult>
    {
        public override int Order => 800;

        public override GetAccountCollectionResult Execute(
            IUnitOfWork unitOfWork,
            GetAccountCollectionParameter parameter,
            GetAccountCollectionResult result
        )
        {
            // if the user is not a salesperson, always filter by billto
            // if the user is a salesperson and they are getting their own account, they may not be assigned to the billto, so don't filter by billto
            // if the user is a salesperson and they are getting all of the accounts for the billto, then filter it by billto (Going to User Administration for example)

            // B2BES updated logic for 'canviewallcustomers' users
            var canViewAllCustomersValue = unitOfWork
                .GetRepository<UserProfile>()
                .Get(SiteContext.Current.UserProfileDto.Id)?
                .GetProperty("escommerce_canViewAllCustomers", bool.FalseString);

            if (!bool.TryParse(canViewAllCustomersValue, out var canViewAllCustomers))
                canViewAllCustomers = false;

            // If you can view all customers but you're looking for your id, skip billto filtering
            // otherwise, use base logic
            var filterByBillTo = false;

            if (canViewAllCustomers)
            {
                if (!parameter.UserProfileId.HasValue)
                {
                    filterByBillTo = true;
                }
            }
            else if (result.Salesperson == null || !parameter.UserProfileId.HasValue)
                filterByBillTo = true;


            if (filterByBillTo)
            {
                result.UserProfileQuery = result.UserProfileQuery.Where(
                    o => o.Customers.Any(p => p.Id == result.BillTo.Id)
                );
            }

            if (!parameter.SearchText.IsBlank())
            {
                result.UserProfileQuery = result.UserProfileQuery.Where(
                    p =>
                        (p.FirstName + " " + p.LastName + " " + p.UserName)
                            .ToLower()
                            .Contains(parameter.SearchText.ToLower())
                );
            }

            if (!result.CanViewUsers)
            {
                result.UserProfileQuery = result.UserProfileQuery.Where(
                    o => o.Id == SiteContext.Current.UserProfileDto.Id
                );
                return NextHandler.Execute(unitOfWork, parameter, result);
            }

            if (parameter.UserProfileId.HasValue)
            {
                result.UserProfileQuery = result.UserProfileQuery.Where(
                    o => o.Id == parameter.UserProfileId.Value
                );
            }

            if (!parameter.UserNames.IsNullOrEmpty())
            {
                result.UserProfileQuery = result.UserProfileQuery.WhereContains(
                    o => o.UserName,
                    parameter.UserNames
                );
            }

            return NextHandler.Execute(unitOfWork, parameter, result);
        }
    }
}
