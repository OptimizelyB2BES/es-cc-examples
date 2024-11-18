using Extensions.Plugins.Search.Elasticsearch.DocumentTypes.Product;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Parameters;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.Plugins.Search.Index.Pipelines.Pipes.CreateElasticsearchProduct
{
    public class FillAlternatePartNumbersProperty : IPipe<CreateElasticsearchProductParameter, CreateElasticsearchProductResult>
    {
        public int Order => 110;

        public CreateElasticsearchProductResult Execute(IUnitOfWork unitOfWork, CreateElasticsearchProductParameter parameter, CreateElasticsearchProductResult result)
        {
            var elasticsearchProductExt = new ElasticsearchProductAlternatePartNumbers(result.ElasticsearchProduct);
            var indexableProductExt = parameter.IndexableProduct as IndexableProductAlternatePartNumbers;
            elasticsearchProductExt.AlternatePartNumbers = ExtractList(indexableProductExt?.AlternatePartNumbers).Distinct().ToList();

            result.ElasticsearchProduct = elasticsearchProductExt;

            return result;
        }

        private static List<string> ExtractList(string content)
        {
           return content?.ToLower().Split(new[] { Convert.ToChar(124) }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        }
    }
}