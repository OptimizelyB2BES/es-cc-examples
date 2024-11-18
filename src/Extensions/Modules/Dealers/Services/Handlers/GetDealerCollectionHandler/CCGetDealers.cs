namespace Extensions.Modules.Dealers.Services.Handlers.GetDealerCollectionHandler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Insite.Core.Context;
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Interfaces.Dependency;
    using Insite.Core.Interfaces.EnumTypes;
    using Insite.Core.Plugins.Utilities;
    using Insite.Core.Services.Handlers;
    using Insite.Data.Entities;
    using Insite.Data.Entities.Dtos;
    using Insite.Data.Repositories.Interfaces;
    using Insite.Dealers.Services.Parameters;
    using Insite.Dealers.Services.Results;
    using Insite.Dealers.SystemSettings;

    [DependencyName(nameof(GetDealers))]
    public sealed class GetDealers : HandlerBase<GetDealerCollectionParameter, GetDealerCollectionResult>
    {
        private readonly LocationsSettings locationsSettings;

        private readonly IObjectToObjectMapper objectToObjectMapper;

        public GetDealers(LocationsSettings locationsSettings, IObjectToObjectMapper objectToObjectMapper)
        {
            this.locationsSettings = locationsSettings;
            this.objectToObjectMapper = objectToObjectMapper;
        }

        public override int Order => 700;

        public override GetDealerCollectionResult Execute(IUnitOfWork unitOfWork, GetDealerCollectionParameter parameter, GetDealerCollectionResult result)
        {
            if (parameter.DealerId.HasValue)
            {
                var dealers = unitOfWork.GetRepository<Dealer>().GetTable().Where(o => o.Id == parameter.DealerId).ToList();
                result.DealerDtos = this.objectToObjectMapper.Map<List<Dealer>, List<DealerDto>>(dealers);
                result.TotalCount = result.DealerDtos.Count;

                return this.NextHandler.Execute(unitOfWork, parameter, result);
            }

            result.PageSize = parameter.PageSize ?? this.locationsSettings.PageSize;
            result.Page = !parameter.Page.HasValue || parameter.Page.Value <= 0 ? 1 : parameter.Page.Value;
            result.TotalPages = ((result.TotalCount - 1) / result.PageSize) + 1;
            var startRow = result.Page == 0 ? result.Page : (result.Page - 1) * result.PageSize;
            var lastRow = startRow + result.PageSize;

            // AST-458 Restrict dealer locations to warehouse/alt warehouse if Pickup fulfillment.
            if (SiteContext.Current.BillTo != null
                && SiteContext.Current.FulfillmentMethod == Enum.GetName(typeof(FulfillmentMethod), FulfillmentMethod.PickUp))
            {
                var defaultWarehouse = unitOfWork.GetRepository<Warehouse>().Get((Guid)SiteContext.Current.BillTo.DefaultWarehouseId);
                var relevantWarehouses = defaultWarehouse.AlternateWarehouses;
                relevantWarehouses.Add(defaultWarehouse);

                result.DealerDtos = unitOfWork.GetTypedRepository<IDealerRepository>()
                    .GetDealers(result.Latitude, result.Longitude, result.Radius, SiteContext.Current.WebsiteDto.Id, startRow, lastRow, parameter.Name)
                        .Where(o => relevantWarehouses
                            .Select(w => w.City).Contains(o.City)
                            && relevantWarehouses.Select(w => w.State).Contains(o.State))
                        .Select(o => o)
                        .ToList();
                
                result.TotalCount = result.DealerDtos.Count;
            }
            else
            {
                result.DealerDtos = unitOfWork.GetTypedRepository<IDealerRepository>()
                    .GetDealers(result.Latitude, result.Longitude, result.Radius, SiteContext.Current.WebsiteDto.Id, startRow, lastRow, parameter.Name)
                    .ToList();
            }

            return this.NextHandler.Execute(unitOfWork, parameter, result);
        }
    }
}