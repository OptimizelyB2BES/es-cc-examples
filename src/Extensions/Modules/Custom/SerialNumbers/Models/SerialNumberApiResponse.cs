using Newtonsoft.Json;
using System.Text;

namespace Extensions.Modules.Custom.SerialNumbers.Models
{
    [JsonObject]
    public class SerialNumberApiResponse
    {
        public int Total { get; set; }
        public SerialNumberOrder[] Data { get; set; }

        public class SerialNumberOrder
        {
            public string invoice_no { get; set; }
        }

        public override string ToString()
        {
            return this != null ? JsonConvert.SerializeObject(this) : "null";
        }
    }
}