using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Services.Handlers;
using Insite.Invoice.Services.Handlers.GetInvoiceCollectionHandler;
using Insite.Invoice.Services.Parameters;
using Insite.Invoice.Services.Results;

namespace Extensions.Modules.Invoice.Services.Handlers.GetInvoiceCollectionHandler
{
    [DependencyName(nameof(ApplyFilteringToStoredCollectionQuery))]
    public sealed class ESCApplyFilteringToStoredCollectionQuery : HandlerBase<GetInvoiceCollectionParameter, GetInvoiceCollectionResult>
    {
        public override int Order => 1000;

        public override GetInvoiceCollectionResult Execute(IUnitOfWork unitOfWork, GetInvoiceCollectionParameter parameter, GetInvoiceCollectionResult result)
        {
            if (result.IsRealTime)
            {
                return NextHandler.Execute(unitOfWork, parameter, result);
            }

            if (parameter.StatusCollection != null && parameter.StatusCollection.Any())
            {
                result.InvoicesQuery = result.InvoicesQuery.Where(o => parameter.StatusCollection.Contains(o.Status));
            }

            if (!parameter.InvoiceNumber.IsBlank())
            {
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.InvoiceNumber.Contains(parameter.InvoiceNumber));
            }

            if (!parameter.OrderNumber.IsBlank())
            {
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.InvoiceHistoryLines.Any(l => l.ErpOrderNumber.Contains(parameter.OrderNumber)));
            }

            if (!parameter.CustomerPO.IsBlank())
            {
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.CustomerPO.Contains(parameter.CustomerPO));
            }

            if (parameter.CustomerSequence != "-1")
            {
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.CustomerSequence == (parameter.CustomerSequence ?? string.Empty));
            }

            if (parameter.ToDate.HasValue)
            {
                var toDate = parameter.ToDate.Value.Date.AddDays(1).AddMinutes(-1);
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.InvoiceDate <= toDate);
            }

            if (parameter.ShowOpenOnly)
            {
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.IsOpen);
            }

            if (parameter.FromDate.HasValue)
            {
                var fromDate = parameter.FromDate.Value.Date;
                result.InvoicesQuery = result.InvoicesQuery.Where(o => o.InvoiceDate >= fromDate);
            }

            return NextHandler.Execute(unitOfWork, parameter, result);
        }
    }
}
