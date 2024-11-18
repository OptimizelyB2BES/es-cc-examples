using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.WebApi.Controllers.ModelLookup.ModelLookup
{
    public class ListModelResultModel
    {
        public ListModelResultModel()
        {
            PackageLines = new List<PackageLineModel>();
        }

        public string ModelApplication { get; set; }
        public string ModelDescription { get; set; }
        public string ModelNumber { get; set; }
        public List<PackageLineModel> PackageLines { get; set; }
    }

    public class PackageLineModel
    {
        public string ModelApplication { get; set; }
        public string PackageVolts { get; set; }
        public string SalesPackage { get; set; }
    }
}