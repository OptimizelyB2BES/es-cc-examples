using Insite.Brands.Services.Parameters;
using Insite.Brands.Services.Results;
using Insite.Brands.WebApi.V1.ApiModels;
using Insite.Brands.WebApi.V1.Mappers;
using Insite.Brands.WebApi.V1.Mappers.Interfaces;
using Insite.Core.Pipelines.GetResourceUris;
using Insite.Core.Plugins.Utilities;
using System;
using System.Linq;
using System.Net.Http;

namespace Extensions.WebApi.V1.Mappers
{
    public class ESCGetBrandCollectionMapper : GetBrandCollectionMapper
    {
        public ESCGetBrandCollectionMapper(IGetBrandMapper getBrandMapper, IGetResourceUrisPipeline brandPipeline, IObjectToObjectMapper objectToObjectMapper) : base(getBrandMapper, brandPipeline, objectToObjectMapper)
        {
        }

        public override GetBrandCollectionParameter MapParameter(BrandCollectionParameter apiParameter, HttpRequestMessage request)
        {

            var parameter = base.MapParameter(apiParameter, request);
            if (apiParameter.Expand == null)
            {
                apiParameter.Expand = string.Empty;
            }
            var source = apiParameter?.Expand.ToLower().Split(',');
            if (!source.Contains("properties"))
            {
                parameter.GetCustomProperties = true;
                apiParameter.Expand += ",properties";
            }
            return parameter;
        }
        public override BrandCollectionModel MapResult(GetBrandCollectionResult serviceResult, HttpRequestMessage request)
        {
            var result = base.MapResult(serviceResult, request);

            result.Brands = result.Brands.Where(b => !b.Properties.Any(p => p.Key == Constants.IsMultiBrandCustomProperty && p.Value.ToLower() == bool.TrueString.ToLower()))?.ToList();
            return result;
        }
    }
}
