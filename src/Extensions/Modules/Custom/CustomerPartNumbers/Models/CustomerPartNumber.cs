using Insite.Catalog.WebApi.V2.ApiModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.Modules.Custom.CustomerPartNumbers.Models
{
    public class CustomerPartNumber
    {
        public string id;
        public ProductModel product;
        public List<string> customerPartNumbers;
        public string validationMessage;
    }
}