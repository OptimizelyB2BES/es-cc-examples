using System;

namespace Extensions.Modules.Custom.SerialNumbers.Models
{
    public class SerialNumberApiRequest
    {
        public string CustomerId { get; set; }
        public string PageSize { get; set; } = "15";
        public string PageNumber { get; set; } = "1";
        public string CompanyNo { get; set; } = "1";
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Search { get; set; }
        public string FullObject { get; set; } = "true";
    }
}