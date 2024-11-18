using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.Modules.Custom.CustomerPartNumbers.Models
{
    public class CreateCustomerPartNumberParameter
    {
        public string PartId { get; set; }
        public List<string> CustomerPartNumbers { get; set; }
    }
}