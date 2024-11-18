using System.Collections.Generic;
using System.Xml.Serialization;

namespace Extensions.Integration.Prophet21.ApiService.Models
{
    [XmlRoot(Namespace = "http://schemas.datacontract.org/2004/07/P21.Transactions.Model.V2")]
    public class TransactionSet
    {
        public TransactionSet()
        {
            Transactions = new List<Transaction>();
        }

        public string Name { get; set; }
        public List<Transaction> Transactions { get; set; }
        public bool UseCodeValues { get; set; }
        public bool IgnoreDisabled { get; set; }
        public string TransactionSplitMethod { get; set; }
    }

    public class Transaction
    {
        public Transaction()
        {
            DataElements = new List<DataElement>();
        }

        public List<DataElement> DataElements { get; set; }
        public string Status { get; set; }
        public Documents Documents { get; set; }
    }

    public class DataElement
    {
        public DataElement()
        {
            Keys = new List<string>();
            Rows = new List<Row>();
        }
        [XmlArray("Keys")]
        [XmlArrayItem("", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public List<string> Keys { get; set; }
        public string Name { get; set; }
        public List<Row> Rows { get; set; }
        public string Type { get; set; }
    }

    public class Row
    {
        public Row()
        {
            Edits = new List<Edit>();
        }

        public List<Edit> Edits { get; set; }
        public RelativeDateEdits RelativeDateEdits { get; set; }
    }

    public class Edit
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public string IgnoreIfEmpty { get; set; }
    }
}