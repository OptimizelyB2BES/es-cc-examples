using Insite.Core.WebApi;
using System.Collections.Generic;

namespace Extensions.Modules.Custom.CustomerPartNumbers.Models
{
    public class CustomerPartNumberCollectionModel
    {
        public string Uri;
        public List<CustomerPartNumber> CustomerPartNumbers;
        public PaginationModel Pagination;
    }
}