using Extensions.Plugins.Search.Elasticsearch.DocumentTypes.Product;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Query.Pipelines.Parameters;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Query.Pipelines.Results;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.Plugins.Search.ElasticSearch.DocumentTypes.Product.Query.Pipelines.Pipes.RunProductSearch
{
    public class FormAlternatePartNumbersSearch : IPipe<RunProductSearchParameter, RunProductSearchResult>
    {
        public int Order => 650;

        public RunProductSearchResult Execute(IUnitOfWork unitOfWork, RunProductSearchParameter parameter, RunProductSearchResult result)
        {
            var alternatePartNumbersQuery = result.ElasticsearchQueryBuilder.MakeFieldQuery(nameof(ElasticsearchProductAlternatePartNumbers.AlternatePartNumbers).ToCamelCase(),
                parameter.ProductSearchParameter.SearchCriteria, Insite.Core.Plugins.Search.Enums.FieldMatch.All,
                false,
                9);

            result.AllQueries.Add(alternatePartNumbersQuery);

            return result;
        }
    }
}