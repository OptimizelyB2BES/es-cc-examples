namespace Extensions.Plugins.Search.Index.Pipelines.Pipes.CreateElasticsearchProduct
{
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Plugins.Pipelines;
    using Insite.Search.Elasticsearch.DocumentTypes.Product;
    using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Parameters;
    using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Results;
    using System;
    using System.Collections.Generic;

    public class UpdateProductCustomerNames : IPipe<CreateElasticsearchProductParameter, CreateElasticsearchProductResult>
    {
        public int Order => 105;

        public CreateElasticsearchProductResult Execute(IUnitOfWork unitOfWork, CreateElasticsearchProductParameter parameter, CreateElasticsearchProductResult result)
        {
            UpdateCustomerNames(parameter.IndexableProduct.CustomerNames, result.ElasticsearchProduct);

            return result;
        }

        private static void UpdateCustomerNames(string content, ElasticsearchProduct elasticsearchProduct)
        {
            var customerNames = new List<string>();
            var customerNamesDisplay = new List<string>();

            var customerSpecificDataRows = content?.Split(new[] { Convert.ToChar(255) }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            foreach (var customerSpecificDataRow in customerSpecificDataRows)
            {
                var customerSpecificDataFields = customerSpecificDataRow?.Split(new[] { Convert.ToChar(254) }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
                if (customerSpecificDataFields.Length < 2)
                {
                    continue;
                }

                var customerId = customerSpecificDataFields[0].ToLowerInvariant();
                if (customerSpecificDataFields.Length >= 2)
                {
                    var customerName = customerSpecificDataFields[1];
                    string[] names = customerName?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
                    foreach (string name in names)
                    {
                        customerNames.Add($"{customerId}{name.Trim().ToLowerInvariant()}");
                        customerNamesDisplay.Add($"{customerId}{name}");
                    }
                }
            }

            elasticsearchProduct.Boost = 9;
            elasticsearchProduct.CustomerNames = customerNames;
            elasticsearchProduct.CustomerNamesDisplay = customerNamesDisplay;
        }
    }
}