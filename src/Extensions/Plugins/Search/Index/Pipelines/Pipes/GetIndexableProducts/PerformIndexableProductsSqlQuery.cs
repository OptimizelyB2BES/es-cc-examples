using Extensions.Plugins.Search.Elasticsearch.DocumentTypes.Product;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Parameters;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Results;

namespace Extensions.Plugins.Search.Index.Pipelines.Pipes.GetIndexableProducts
{
    public class PerformIndexableProductsSqlQuery : IPipe<GetIndexableProductsParameter, GetIndexableProductsResult>
    {
        public int Order => 300;

        public GetIndexableProductsResult Execute(IUnitOfWork unitOfWork, GetIndexableProductsParameter parameter, GetIndexableProductsResult result)
        {
            result.IndexableProducts = unitOfWork.DataProvider.SqlQuery<IndexableProductAlternatePartNumbers>(result.FormattedSqlStatement, null, false, parameter.QueryTimeout);

            return result;
        }
    }
}