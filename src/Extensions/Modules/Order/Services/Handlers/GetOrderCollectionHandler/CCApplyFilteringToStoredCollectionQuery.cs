using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Services.Handlers;
using Insite.Invoice.Services.Handlers.GetInvoiceCollectionHandler;
using Insite.Order.Services.Parameters;
using Insite.Order.Services.Results;

namespace Extensions.Modules.Order.Services.Handlers.GetOrderCollectionHandler
{
    [DependencyName(nameof(ApplyFilteringToStoredCollectionQuery))]
    public class ESCApplyFilteringToStoredCollectionQuery : HandlerBase<GetOrderCollectionParameter, GetOrderCollectionResult>
    {
        public override int Order => 1000;

        public override GetOrderCollectionResult Execute(IUnitOfWork unitOfWork, GetOrderCollectionParameter parameter, GetOrderCollectionResult result)
        {
            if (result.IsRealTime)
            {
                return NextHandler.Execute(unitOfWork, parameter, result);
            }

            if (parameter.StatusCollection != null && parameter.StatusCollection.Any())
            {
                result.OrdersQuery = result.OrdersQuery.Where(o => parameter.StatusCollection.Contains(o.Status));
            }

            if (!parameter.OrderNumber.IsBlank())
            {
                result.OrdersQuery = result.OrdersQuery.Where(o =>
                    o.ErpOrderNumber.Contains(parameter.OrderNumber)
                    || o.WebOrderNumber.Contains(parameter.OrderNumber));
            }

            if (!parameter.CustomerPO.IsBlank())
            {
                result.OrdersQuery = result.OrdersQuery.Where(o => o.CustomerPO.Contains(parameter.CustomerPO));
            }

            if (parameter.CustomerSequence != "-1")
            {
                result.OrdersQuery = result.OrdersQuery.Where(o => o.CustomerSequence == (parameter.CustomerSequence ?? string.Empty));
            }

            if (parameter.ToDate.HasValue)
            {
                var toDate = parameter.ToDate.Value.Date.AddDays(1).AddMinutes(-1);
                result.OrdersQuery = result.OrdersQuery.Where(o => o.OrderDate <= toDate);
            }

            if (parameter.FromDate.HasValue)
            {
                var fromDate = parameter.FromDate.Value.Date;
                result.OrdersQuery = result.OrdersQuery.Where(o => o.OrderDate >= fromDate);
            }

            if (!parameter.OrderTotalOperator.IsBlank())
            {
                if (parameter.OrderTotalOperator.Equals("Less Than", StringComparison.OrdinalIgnoreCase))
                {
                    result.OrdersQuery = result.OrdersQuery.Where(o => o.OrderTotal < parameter.OrderTotal);
                }
                else if (parameter.OrderTotalOperator.Equals("Greater Than", StringComparison.OrdinalIgnoreCase))
                {
                    result.OrdersQuery = result.OrdersQuery.Where(o => o.OrderTotal > parameter.OrderTotal);
                }
                else if (parameter.OrderTotalOperator.Equals("Equal To", StringComparison.OrdinalIgnoreCase))
                {
                    result.OrdersQuery = result.OrdersQuery.Where(o => o.OrderTotal == parameter.OrderTotal);
                }
            }

            if (!parameter.ProductErpNumber.IsBlank())
            {
                result.OrdersQuery = result.OrdersQuery.Where(
                    o => o.OrderHistoryLines.Any(
                        p => p.ProductErpNumber.Contains(parameter.ProductErpNumber)
                        || p.CustomerProductNumber.Contains(parameter.ProductErpNumber)));
            }

            if (!parameter.Search.IsBlank())
            {
                var search = parameter.Search.Trim();
                result.OrdersQuery = result.OrdersQuery.Where(o =>
                    o.CustomerPO.Contains(search) ||
                    o.ErpOrderNumber.Contains(search) ||
                    o.WebOrderNumber.Contains(search) ||
                    o.STCompanyName.Contains(search));
            }

            return NextHandler.Execute(unitOfWork, parameter, result);
        }
    }
}
