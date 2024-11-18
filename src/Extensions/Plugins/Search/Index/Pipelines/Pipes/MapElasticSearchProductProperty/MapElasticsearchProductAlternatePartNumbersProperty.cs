using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Parameters;
using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Results;
using Nest;

namespace Extensions.Plugins.Search.Index.Pipelines.Pipes.MapElasticSearchProductProperty
{
    public class MapElasticsearchProductAlternatePartNumbersProperty : IPipe<MapElasticSearchProductPropertyParameter, MapElasticSearchProductPropertyResult>
    {
        public int Order => 110;

        public MapElasticSearchProductPropertyResult Execute(IUnitOfWork unitOfWork, MapElasticSearchProductPropertyParameter parameter, MapElasticSearchProductPropertyResult result)
        {
            if (parameter.SearchBoost != null && parameter.PropertyDefinition != null && parameter.SearchBoost.IsQueryable && parameter.PropertyDefinition.IsCustomProperty
                && parameter.PropertyDefinition.Name == "escommerce_alternatePartNumbers")
            {
                result.Property = new TextProperty
                {
                    Boost = 9,
                };
            }

            return result;
        }
    }
}