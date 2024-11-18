using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Parameters;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Results;

namespace Extensions.Plugins.Search.Index.Pipelines.Pipes.GetIndexableProducts
{
    public class AddAlternatePartNumbersToSqlStatement : IPipe<GetIndexableProductsParameter, GetIndexableProductsResult>
    {
        public int Order => 150;

        public GetIndexableProductsResult Execute(IUnitOfWork unitOfWork, GetIndexableProductsParameter parameter, GetIndexableProductsResult result)
        {
            result.CustomFields = @"STUFF(
                ISNULL(
                    (SELECT CHAR(255) + CAST(pp.Value AS NVARCHAR(40))
                    FROM CustomProperty pp WITH (NOLOCK)
                    JOIN search.SearchBoost sb WITH (NOLOCK) ON
                        sb.EntityType = 'Product' AND
                        sb.Query = 'Field_customProperties.escommerce_alternatePartNumbers' AND
                        sb.IsQueryable = 1
                    WHERE pp.ParentId = p.Id AND
                        pp.Name = 'escommerce_alternatePartNumbers'
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS AlternatePartNumbers";

                               

            return result;
        }
    }
}