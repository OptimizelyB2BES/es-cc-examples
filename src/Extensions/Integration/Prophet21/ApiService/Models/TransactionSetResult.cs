using System.Collections.Generic;
using System.Xml.Serialization;

namespace Extensions.Integration.Prophet21.ApiService.Models

{
    public class TransactionSetResult
    {
        [XmlElement("Results")]
        public ResultTransactionSet Results { get; set; }
        public Summary Summary { get; set; }
        public string[] Messages { get; set; }
    }

    public class Summary
    {
        public int Failed { get; set; }
        public int Other { get; set; }
        public int Succeeded { get; set; }
    }

    public class ResultTransactionSet
    {
        public string Name { get; set; }
        [XmlElement("Transactions")]
        public ResultTransactions Transactions { get; set; }
        public bool UseCodeValues { get; set; }
        public bool IgnoreDisabled { get; set; }
        public string TransactionSplitMethod { get; set; }
    }

    public class ResultTransactions
    {
        public ResultTransactions()
        {
            TransactionList = new List<ResultTransaction>();
        }
        [XmlElement("Transaction")]
        public List<ResultTransaction> TransactionList { get; set; }
    }

    public class ResultTransaction
    {
        [XmlElement("DataElements")]
        public ResultDataElements DataElements { get; set; }
        public string Status { get; set; }
        public Documents Documents { get; set; }
    }

    public class ResultDataElements
    {
        public ResultDataElements()
        {
            DataElementList = new List<ResultDataElement>();
        }

        [XmlElement("DataElement")]
        public List<ResultDataElement> DataElementList { get; set; }
    }

    public class Documents
    {
        [XmlAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public bool nil = true;
    }

    public class ResultDataElement
    {
        public ResultDataElement()
        {
            Keys = new List<string>();
        }
        [XmlArray("Keys")]
        [XmlArrayItem("", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public List<string> Keys { get; set; }
        public string Name { get; set; }

        [XmlElement("Rows")]
        public ResultRows Rows { get; set; }
        public string Type { get; set; }
    }

    public class ResultRows
    {
        [XmlElement("Row")]
        public List<ResultRow> RowsList { get; set; }
    }

    public class ResultRow
    {
        [XmlElement("Edits")]
        public ResultEdits Edits { get; set; }
        public RelativeDateEdits RelativeDateEdits { get; set; }
    }

    public class RelativeDateEdits
    {
    }

    public class ResultEdits
    {
        public ResultEdits()
        {
            EditsList = new List<ResultEdit>();
        }

        [XmlElement("Edit")]
        public List<ResultEdit> EditsList { get; set; }
    }


    public class ResultEdit
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public string IgnoreIfEmpty { get; set; }
    }
}