using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.Modules.Custom.CustomerPartNumbers.Models
{
    public class CustomerPartNumberCollectionParameter
    {
        public string PartNumber { get; set; }
        public string CustomerPartNumber { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string Sort { get; set; }
    }
}