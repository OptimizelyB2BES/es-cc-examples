using Extensions.Modules.Custom.SerialNumbers.Models;
using Insite.Core.Interfaces.Dependency;
using System.Threading.Tasks;

namespace Extensions.Modules.Custom.SerialNumbers.Services
{
    public interface IESCSerialNumberApiService : IDependency
    {
        SerialNumberApiResponse GetInvoicesBySerialNumber(SerialNumberApiRequest request);
    }
}