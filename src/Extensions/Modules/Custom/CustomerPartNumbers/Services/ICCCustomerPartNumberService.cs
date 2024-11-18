using Extensions.Modules.Custom.CustomerPartNumbers.Models;
using Insite.Core.Interfaces.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.Modules.Custom.CustomerPartNumbers.Services
{
    public interface ICustomerPartNumberService : IDependency
    {
        CustomerPartNumberCollectionModel GetCustomerPartNumbers(CustomerPartNumberCollectionParameter parameter);
        CustomerPartNumberCollectionModel GetCustomerPartNumbersWithProduct(CustomerPartNumberCollectionParameter parameters);
        CustomerPartNumber Create(CreateCustomerPartNumberParameter parameter);
        CustomerPartNumber Update(string id, CustomerPartNumber parameter);

        bool Delete(string customerPartNumberId);
    }
}