using Extensions.Integration.Prophet21.ApiService.Models;
using Extensions.Modules.Account.Services.Handlers.AddAccountHandler;
using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;

namespace Extensions.Integration.Prophet21.ApiService
{
    public interface IESCProphet21ApiService : IDependency
    {
        TransactionSetResult PostTransaction(IntegrationConnection integrationConnection, TransactionSet transactionSet);
        Contact CreateContact(IntegrationConnection integrationConnection, ESCContact contact);
        OrderImport OrderImport(IntegrationConnection integrationConnection, OrderImport orderImport, string jobName);
    }
}