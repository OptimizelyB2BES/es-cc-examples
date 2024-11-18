using Insite.Search.Elasticsearch.DocumentTypes.Product.Index;

namespace Extensions.Plugins.Search.Elasticsearch.DocumentTypes.Product
{
    public class IndexableProductAlternatePartNumbers : IndexableProduct
    {
        public string AlternatePartNumbers { get; set; }
    }
}