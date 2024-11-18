using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using System.Xml.Serialization;

namespace Extensions.Modules.Account.Services.Handlers.AddAccountHandler
{
    [XmlRoot(ElementName = "Contact")]
    public class ESCContact : Contact
    {
        public UserDefinedFields UserDefinedFields { get; set; }
    }
}