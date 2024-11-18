using System.Collections.Generic;

namespace Extensions.WebApi.Controllers.ModelLookup.ModelLookup
{
    public class Arg
    {
        public string ModNo { get; set; }
    }

    public class ColumnMeta
    {
        public string catalog { get; set; }
        public string schema { get; set; }
        public string tableName { get; set; }
        public string columnName { get; set; }
        public string columnLabel { get; set; }
        public string columnType { get; set; }
    }

    public class Row
    {
        public string SalesPackageNumber { get; set; }
        public string ModelFamily { get; set; }
        public string SequenceNumber { get; set; }
        public string PartNumber { get; set; }
        public string ReplacedPartNumber { get; set; }
        public string PartDescription { get; set; }
        public string CriticalPart { get; set; }
        public string Groups { get; set; }
        public int PartQuantity { get; set; }
        public string PartHistory { get; set; }
        public string DrawingExists { get; set; }
        public string DrawingLink { get; set; }
        public string SigChartLink { get; set; }
    }

    public class Result
    {
        public bool truncated { get; set; }
        public int rowCount { get; set; }
        public int filteredCount { get; set; }
        public List<ColumnMeta> columnMeta { get; set; }
        public List<Row> rows { get; set; }
    }

    public class ModelLookupResponseModel
    {
        public Arg arg { get; set; }
        public List<Result> result { get; set; }
        public List<int> updateCount { get; set; }
    }
}