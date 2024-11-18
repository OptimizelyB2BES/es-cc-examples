using Extensions.CustomSettings;
using Extensions.Modules.Custom.CustomerPartNumbers.Models;
using Insite.Catalog.Services.V2;
using Insite.Catalog.Services.V2.Dtos.Product;
using Insite.Catalog.Services.V2.Parameters;
using Insite.Catalog.WebApi.V2.ApiModels.Product;
using Insite.Common.DynamicLinq;
using Insite.Common.Extensions;
using Insite.Common.Logging;
using Insite.Common.Providers;
using Insite.Core.Context;
using Insite.Core.Enums;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Localization;
using Insite.Core.Plugins.Catalog;
using Insite.Core.Plugins.Integration;
using Insite.Core.Plugins.Utilities;
using Insite.Core.Providers;
using Insite.Core.SystemSetting;
using Insite.Core.SystemSetting.Groups.SiteConfigurations;
using Insite.Core.WebApi;
using Insite.Data.Entities;
using Insite.Data.Extensions;
using Insite.Data.Repositories.Interfaces;
using Insite.Data.Repositories.Parameters;
using Insite.Data.Repositories.Results;
using Insite.Public.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Extensions.Modules.Custom.CustomerPartNumbers.Services
{
    public class CustomerPartNumberService : ICustomerPartNumberService
    {
        private readonly IESCCpnService ESCUnilogService;
        private readonly IProductService ProductService;
        private readonly IUnitOfWork UnitOfWork;
        private readonly CustomerPartNumberSettings CustomerPartNumberSettings;
        private readonly Lazy<ICatalogPathBuilder> catalogPathBuilder;
        private readonly Lazy<ICurrencyFormatProvider> currencyFormatProvider;
        private readonly Lazy<ITranslationLocalizer> translationLocalizer;
        private readonly IIntegrationJobSchedulingService integrationJobSchedulingService;

        public CustomerPartNumberService(IESCCpnService escUnilogService,
            IProductService productService,
            IUnitOfWorkFactory unitOfWorkFactory,
            CustomerPartNumberSettings CustomerPartNumberSettings,
            Lazy<ICatalogPathBuilder> catalogPathBuilder,
            Lazy<ICurrencyFormatProvider> currencyFormatProvider,
            Lazy<ITranslationLocalizer> translationLocalizer,
            IIntegrationJobSchedulingService integrationJobSchedulingService)
        {
            ESCUnilogService = escUnilogService;
            ProductService = productService;
            UnitOfWork = unitOfWorkFactory.GetUnitOfWork();
            this.CustomerPartNumberSettings = CustomerPartNumberSettings;

            this.catalogPathBuilder = catalogPathBuilder;
            this.currencyFormatProvider = currencyFormatProvider;
            this.translationLocalizer = translationLocalizer;
            this.integrationJobSchedulingService = integrationJobSchedulingService;
        }

        #region Get CPNs

        public CustomerPartNumberCollectionModel GetCustomerPartNumbers(CustomerPartNumberCollectionParameter parameter)
        {
            var result = new CustomerPartNumberCollectionModel();
            var query = UnitOfWork.GetRepository<CustomerProduct>()
                .GetTableAsNoTracking()
                .Expand(o => o.CustomProperties)
                .Expand(o => o.Product)
                .Where(o => o.CustomerId == SiteContext.Current.BillTo.Id);

            query = ApplyFilters(query, parameter);

            ExecuteQuery(query, result);

            return result;
        }

        public CustomerPartNumberCollectionModel GetCustomerPartNumbersWithProduct(CustomerPartNumberCollectionParameter parameter)
        {
            var result = new CustomerPartNumberCollectionModel();
            var query = UnitOfWork.GetRepository<CustomerProduct>()
                .GetTableAsNoTracking()
                .Expand(o => o.CustomProperties)
                .Expand(o => o.Product)
                .Where(o => o.CustomerId == SiteContext.Current.BillTo.Id);

            query = ApplyFilters(query, parameter);

            query = ApplySorting(query, parameter);

            query = ApplyPagination(query, parameter, result);

            ExecuteQueryWithProducts(query, result);

            return result;
        }

        private IQueryable<CustomerProduct> ApplyFilters(IQueryable<CustomerProduct> query, CustomerPartNumberCollectionParameter parameter)
        {
            if (parameter.PartNumber.IsNotBlank())
            {
                query = query.Where(o => o.Product.ErpNumber.Equals(parameter.PartNumber, StringComparison.OrdinalIgnoreCase));
            }

            if (parameter.CustomerPartNumber.IsNotBlank())
            {
                query = query.Where(o => o.CustomProperties.Any(cp =>
                    cp.Name.Equals(Constants.CpnList, StringComparison.OrdinalIgnoreCase) &&
                    cp.Value.Contains(parameter.CustomerPartNumber)
                ));
            }
            return query;
        }

        private IQueryable<CustomerProduct> ApplyPagination(IQueryable<CustomerProduct> query, CustomerPartNumberCollectionParameter parameter, CustomerPartNumberCollectionModel result)
        {
            var defaultPageSize = SettingsGroupProvider.Current.Get<StorefrontApiSettings>().DefaultPageSize;
            var pageSize = parameter.PageSize.HasValue && parameter.PageSize.Value > 0
                ? parameter.PageSize.Value
                : defaultPageSize;
            var page = parameter.Page ?? 0;
            page = page <= 0 ? 1 : page;

            var totalCount = query.Count();

            result.Pagination = new PaginationModel(page, pageSize, defaultPageSize, totalCount);

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private IQueryable<CustomerProduct> ApplySorting(IQueryable<CustomerProduct> query, CustomerPartNumberCollectionParameter parameter)
        {
            if (parameter.Sort.EqualsIgnoreCase("productNumber DESC"))
            {
                query = query.OrderByDescending(o => o.Product.ErpNumber);
            }
            else if (parameter.Sort.EqualsIgnoreCase("productTitle"))
            {
                query = query.OrderBy(o => o.Product.ShortDescription);
            }
            else if (parameter.Sort.EqualsIgnoreCase("productTitle DESC"))
            {
                query = query.OrderByDescending(o => o.Product.ShortDescription);
            }
            else if (parameter.Sort.EqualsIgnoreCase("customerPartNumbers"))
            {
                query = query.OrderBy(o => o.CustomProperties.FirstOrDefault(cp => cp.Name.Equals(Constants.CpnList, StringComparison.OrdinalIgnoreCase)).Value);
            }
            else if (parameter.Sort.EqualsIgnoreCase("customerPartNumbers DESC"))
            {
                query = query.OrderByDescending(o => o.CustomProperties.FirstOrDefault(cp => cp.Name.Equals(Constants.CpnList, StringComparison.OrdinalIgnoreCase)).Value);
            }
            else
            {
                query = query.OrderBy(o => o.Product.ErpNumber);
            }
            return query;
        }
        private void ExecuteQueryWithProducts(IQueryable<CustomerProduct> query, CustomerPartNumberCollectionModel result)
        {
            var customerProducts = query.ToList();

            var productIds = customerProducts.Select(o => o.Product.Id).ToList();
            var productDtoDictionary = GetProductDtoDictionary(productIds, result.Pagination.PageSize);

            result.CustomerPartNumbers = new List<CustomerPartNumber>();
            foreach (var customerProduct in customerProducts)
            {
                if (productDtoDictionary.TryGetValue(customerProduct.ProductId, out ProductDto productDto))
                {
                    var productModel = new ProductModel(productDto);
                    result.CustomerPartNumbers.Add(new CustomerPartNumber()
                    {
                        id = customerProduct.Id.ToString(),
                        product = productModel,
                        customerPartNumbers = customerProduct.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList()
                    });
                }
                else
                {
                    result.CustomerPartNumbers.Add(new CustomerPartNumber()
                    {
                        id = customerProduct.Id.ToString(),
                        product = null,
                        customerPartNumbers = customerProduct.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList()
                    });
                }
            }
        }

        private void ExecuteQuery(IQueryable<CustomerProduct> query, CustomerPartNumberCollectionModel result)
        {
            var customerProducts = query.ToList();

            result.CustomerPartNumbers = new List<CustomerPartNumber>();
            foreach (var customerProduct in customerProducts)
            {
                result.CustomerPartNumbers.Add(new CustomerPartNumber()
                {
                    id = customerProduct.ProductId.ToString(),
                    product = null,
                    customerPartNumbers = customerProduct.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList()
                });
            }
        }

        #endregion Get CPNs

        #region Create CPN

        public CustomerPartNumber Create(CreateCustomerPartNumberParameter parameter)
        {
            if (!ValidateCreateParameter(parameter, out string validationMessage))
            {
                return new CustomerPartNumber()
                {
                    validationMessage = validationMessage
                };
            }

            var partId = Guid.Parse(parameter.PartId);
            var productErpNumber = UnitOfWork.GetRepository<Product>()
                .GetTableAsNoTracking()
                .Where(o => o.Id == partId)
                .Select(o => o.ErpNumber)
                .FirstOrDefault();
            var results = HandleCpnApiChanges(productErpNumber, new List<string>(), parameter.CustomerPartNumbers);
            if (results.Any(o => o.Key == false))
            {
                return new CustomerPartNumber()
                {
                    validationMessage = results.FirstOrDefault(o => o.Key == false).Value
                };
            }

            var repository = UnitOfWork.GetRepository<CustomerProduct>();
            var newCustomerPartNumber = repository.Create();
            newCustomerPartNumber.CustomerId = SiteContext.Current.BillTo.Id;
            newCustomerPartNumber.ProductId = partId;
            newCustomerPartNumber.SetProperty(Constants.CpnList, string.Join(",", parameter.CustomerPartNumbers));
            repository.Insert(newCustomerPartNumber);

            var productIds = new List<Guid> { newCustomerPartNumber.ProductId };
            var productDtos = GetProductDtoDictionary(productIds, 1);
            ReindexProducts(productIds);
            return new CustomerPartNumber()
            {
                id = newCustomerPartNumber.Id.ToString(),
                product = new ProductModel(productDtos.First().Value),
                customerPartNumbers = newCustomerPartNumber.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList()
            };
        }

        public bool ValidateCreateParameter(CreateCustomerPartNumberParameter parameter, out string validationMessage)
        {
            validationMessage = string.Empty;
            if (!Guid.TryParse(parameter.PartId, out Guid productGuid))
            {
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Invalid_Part", "Invalid Product Selected");
                return false;
            }

            var alreadyExists = UnitOfWork
                .GetRepository<CustomerProduct>()
                .GetTableAsNoTracking()
                .Any(o => o.CustomerId == SiteContext.Current.BillTo.Id && o.ProductId == productGuid);
            if (alreadyExists)
            {
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Part_Already_Exists", "Product Already Exists");
                return false;
            }

            var removedCount = parameter.CustomerPartNumbers.RemoveAll(o => o.Length < 1);
            if (removedCount > 0)
            {
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Part_Empty_Items", "Some part numbers were empty. Please remove all empty part numbers and try again.");
                return false;
            }

            var hasInvalidWhitespace = parameter.CustomerPartNumbers.Any(o => char.IsWhiteSpace(o[0]) || char.IsWhiteSpace(o[o.Length - 1]));
            if (hasInvalidWhitespace)
            {
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Part_Starts_Or_Ends_With_Whitespace", "Part numbers cannot begin or end with spaces. Please remove all starting or trailing whitespaces.");
                return false;
            }

            var disallowedCharacters = CustomerPartNumberSettings.DisallowedCharacters.Split(new string[] { "|#|" }, StringSplitOptions.None);
            if (parameter.CustomerPartNumbers.Any(o => disallowedCharacters.Any(p => o.Contains(p))))
            {
                validationMessage = string.Format(MessageProvider.Current.GetMessage("MyPartNumbers_Disallowed_Character", "Part Numbers may not contain any of the following characters: {0}"), string.Join(" ", disallowedCharacters));
                return false;
            }

            var tooLong = parameter.CustomerPartNumbers.Where(o => o.Length > CustomerPartNumberSettings.CustomerPartNumberMaximumLength).ToList();
            if (tooLong.Any())
            {
                validationMessage = string.Format(MessageProvider.Current.GetMessage("MyPartNumbers_Part_Numbers_Too_Long", "The following part numbers were too long. The maximum length for part numbers is {0}: {1}"), CustomerPartNumberSettings.CustomerPartNumberMaximumLength, string.Join(",", tooLong));
                return false;
            }

            return true;
        }

        #endregion Create CPN

        #region Update CPN

        public CustomerPartNumber Update(string id, CustomerPartNumber parameter)
        {
            if (!ValidateUpdateParameter(id, parameter, out string validationMessage, out Guid guid))
            {
                return new CustomerPartNumber()
                {
                    validationMessage = validationMessage,
                };
            }
            parameter.customerPartNumbers?.RemoveAll(o => o.Length < 1);
            parameter.customerPartNumbers?.Each(o => o.Trim());
            var repository = UnitOfWork.GetRepository<CustomerProduct>();
            var customerProduct = repository.Get(guid);

            var productErpNumber = customerProduct.Product.ErpNumber;
            var currentValues = customerProduct.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList();
            var results = HandleCpnApiChanges(productErpNumber, currentValues, parameter.customerPartNumbers);
            if (results.Any(o => o.Key == false))
            {
                return new CustomerPartNumber()
                {
                    validationMessage = results.FirstOrDefault(o => o.Key == false).Value
                };
            }

            customerProduct.SetProperty(Constants.CpnList, string.Join(",", parameter.customerPartNumbers));
            UnitOfWork.Save();

            var productIds = new List<Guid> { customerProduct.ProductId };
            var productDtos = GetProductDtoDictionary(productIds, 1);
            ReindexProducts(productIds);
            return new CustomerPartNumber()
            {
                id = customerProduct.Id.ToString(),
                product = new ProductModel(productDtos.First().Value),
                customerPartNumbers = customerProduct.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList()
            };
        }

        public bool ValidateUpdateParameter(string id, CustomerPartNumber parameter, out string validationMessage, out Guid guid)
        {
            validationMessage = string.Empty;
            if (!Guid.TryParse(id, out Guid productGuid))
            {
                guid = Guid.Empty;
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Invalid_Part", "Invalid Product Selected");
                return false;
            }
            guid = productGuid;

            var removedCount = parameter.customerPartNumbers.RemoveAll(o => o.Length < 1);
            if (removedCount > 0)
            {
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Part_Empty_Items", "Some part numbers were empty. Please remove all empty part numbers and try again.");
                return false;
            }

            var hasInvalidWhitespace = parameter.customerPartNumbers.Any(o => char.IsWhiteSpace(o[0]) || char.IsWhiteSpace(o[o.Length - 1]));
            if (hasInvalidWhitespace)
            {
                validationMessage = MessageProvider.Current.GetMessage("MyPartNumbers_Part_Starts_Or_Ends_With_Whitespace", "Part numbers cannot begin or end with spaces. Please remove all starting or trailing whitespaces.");
                return false;
            }

            var tooLong = parameter.customerPartNumbers.Where(o => o.Length > CustomerPartNumberSettings.CustomerPartNumberMaximumLength).ToList();
            if (tooLong.Any())
            {
                validationMessage = string.Format(MessageProvider.Current.GetMessage("MyPartNumbers_Part_Numbers_Too_Long", "The following part numbers were too long. The maximum length for part numbers is {0}: {1}"), CustomerPartNumberSettings.CustomerPartNumberMaximumLength, string.Join(",", tooLong));
                return false;
            }

            var disallowedCharacters = CustomerPartNumberSettings.DisallowedCharacters.Split(new string[] { "|#|" }, StringSplitOptions.None);
            if (parameter.customerPartNumbers.Any(o => disallowedCharacters.Any(p => o.Contains(p))))
            {
                validationMessage = string.Format(MessageProvider.Current.GetMessage("MyPartNumbers_Disallowed_Character", "Part Numbers may not contain any of the following characters: {0}"), string.Join(" ", disallowedCharacters));
                return false;
            }

            return true;
        }

        #endregion Update CPN

        #region Delete CPN

        public bool Delete(string customerPartNumberId)
        {
            if (!Guid.TryParse(customerPartNumberId, out Guid guid))
            {
                return false;
            }
            var repository = UnitOfWork.GetRepository<CustomerProduct>();
            var customerProduct = repository.Get(guid);

            var productErpNumber = customerProduct.Product.ErpNumber;
            var currentValues = customerProduct.GetProperty(Constants.CpnList, string.Empty).Split(',').ToList();
            var results = HandleCpnApiChanges(productErpNumber, currentValues, new List<string>());
            if (results.Any(o => o.Key == false))
            {
                return false;
            }

            repository.Delete(customerProduct);
            UnitOfWork.Save();

            ReindexProducts(new List<Guid>() { customerProduct.ProductId });

            return true;
        }

        #endregion Delete CPN

        #region Utility Methods

        private Dictionary<Guid, ProductDto> GetProductDtoDictionary(List<Guid> productIds, int? pageSize)
        {
            var parameter = new GetProductCollectionParameter(SiteContext.Current, productIds, pageSize, null, null);
            var productSearcResults = ProductService.GetProductCollection(parameter);

            if (productSearcResults.ResultCode == Insite.Core.Services.ResultCode.Error)
            {
                throw new Exception("Error occured during product lookup");
            }

            //BESUP-653
            var missingProductIds = productIds.Where(o => !productSearcResults.ProductDtos.Any(p => p.Id == o));

            if (missingProductIds.Any())
            {
                var siteContext = SiteContext.Current;

                var productDataParameter = new GetProductDataParameter(
                    siteContext.WebsiteDto.Id,
                    siteContext.LanguageDto.Id,
                    siteContext.BillTo?.Id,
                    siteContext.ShipTo?.Id,
                    null,
                    true,
                    null,
                    null
                );

                productDataParameter.ProductIds = missingProductIds;
                productDataParameter.GetHiddenProducts = true;
                var productDataResult = UnitOfWork
                    .GetTypedRepository<IProductRepository>()
                    .GetProductData(parameter.GetProductDataParameter);
                var missingDtos = new List<ProductDto>();

                var products = UnitOfWork
                    .GetTypedRepository<IProductRepository>()
                    .GetTable()
                    .Where(o => missingProductIds.Contains(o.Id));

                foreach (var missingProductId in missingProductIds)
                {
                    var product = products.First(p => p.Id == missingProductId);
                    missingDtos.Add(ProductToDto(product, productDataResult));
                }

                productSearcResults.ProductDtos = productSearcResults.ProductDtos.Concat(missingDtos).ToList();
            }

            return productSearcResults.ProductDtos.ToDictionary(o => o.Id);
        }

        private ProductDto ProductToDto(
            Product product,
            GetProductDataResult getProductDataResult
        ) =>
            new ProductDto(
                product,
                getProductDataResult.ProductImages.NullableGetValueOrDefault(product.Id),
                getProductDataResult.ProductUnitOfMeasures.NullableGetValueOrDefault(
                    product.Id
                ),
                getProductDataResult.Brands.NullableGetValueOrDefault(product.BrandId),
                getProductDataResult.ProductLines.NullableGetValueOrDefault(
                    product.ProductLineId
                ),
                getProductDataResult.CustomerProducts.NullableGetValueOrDefault(product.Id),
                getProductDataResult.TranslationProperties,
                SiteContext.Current,
                catalogPathBuilder.Value,
                currencyFormatProvider.Value,
                translationLocalizer.Value
            );

        private List<KeyValuePair<bool, string>> HandleCpnApiChanges(string productNumber, List<string> previousState, List<string> newState)
        {
            var removedItems = previousState.Where(o => !newState.Contains(o, StringComparer.OrdinalIgnoreCase)).ToList();
            var addedItems = newState.Where(o => !previousState.Contains(o, StringComparer.OrdinalIgnoreCase)).ToList();
            var customerNumber = SiteContext.Current.BillTo.CustomerNumber;

            var apiResponses = new List<KeyValuePair<bool, string>>();
            foreach (var removedItem in removedItems)
            {
                LogHelper.For(this).Debug($"Deleting CPN part number: {removedItem}");
                var deleteResult = ESCUnilogService.DeleteCustomerPartNumber(customerNumber, productNumber, removedItem);
                if (!deleteResult.Key)
                {
                    deleteResult = new KeyValuePair<bool, string>(
                        false, string.Format(
                            MessageProvider.Current.GetMessage("MyPartNumbers_Part_Not_Deleted", "An error occurred deleting part number {0}: {1}"),
                            removedItem,
                            deleteResult.Value
                        )
                    );
                    apiResponses.Add(deleteResult);
                    break;
                }
                apiResponses.Add(deleteResult);
            }

            if (apiResponses.Any(o => o.Key == false))
            {
                return apiResponses;
            }

            foreach (var addedItem in addedItems)
            {
                var createResult = ESCUnilogService.CreateCustomerPartNumber(customerNumber, productNumber, addedItem);
                if (!createResult.Key)
                {
                    createResult = new KeyValuePair<bool, string>(
                        false, string.Format(
                            MessageProvider.Current.GetMessage("MyPartNumbers_Part_Not_Created", "An error occurred creating part number {0}: {1}"),
                            addedItem,
                            createResult.Value
                        )
                    );
                    apiResponses.Add(createResult);
                    break;
                }
                apiResponses.Add(createResult);
            }

            return apiResponses;
        }

        private void ReindexProducts(List<Guid> productIds)
        {
            FlagProductsForReindexing(productIds);

            var options = new RebuildIndexOptions()
            {
                IndexType = "product",
                IsPartial = true,
                RebuildDynamicCategories = false,
                RebuildRestrictionGroups = false,
            };
            var jobDefinition = GetRebuildSearchIndexJob();
            var jobParameters =
                integrationJobSchedulingService.PrepareRebuildIndexJobParameters(
                    jobDefinition.JobDefinitionParameters,
                    options
                );

            UnitOfWork
                .GetRepository<IntegrationJob>()
                .Insert(
                    new IntegrationJob
                    {
                        JobDefinitionId = jobDefinition.Id,
                        Status = "Queued",
                        IntegrationJobParameters = jobParameters,
                        ScheduleDateTime = DateTimeProvider.Current.Now,
                        RunStandalone = true,
                        WebsiteId = null
                    }
                );
            UnitOfWork.Save();
        }



        protected JobDefinition GetRebuildSearchIndexJob()
        {
            var standardJobName = JobDefinitionStandardJobName.RebuildSearchIndex.ToString();
            var standardJobNameDisplay =
                JobDefinitionStandardJobName.RebuildSearchIndex.GetAttribute<DisplayAttribute>().Name;

            var jobDefinition = UnitOfWork
                .GetTypedRepository<IJobDefinitionRepository>()
                .GetTableAsNoTracking()
                .Expand(o => o.JobDefinitionParameters)
                .FirstOrDefault(o => o.StandardJobName == standardJobName);
            if (jobDefinition == null)
            {
                var errorMessage = $"Unable to find a JobDefinition for: {standardJobNameDisplay}";
                LogHelper.For(this).Error(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            return jobDefinition;
        }

        protected void FlagProductsForReindexing(List<Guid> productIds)
        {
            var products = UnitOfWork.GetRepository<Product>().GetTable().Where(o => productIds.Contains(o.Id));
            foreach (var product in products)
            {
                if (product.IndexStatus != (int)Product.IndexStatusType.NeverIndex)
                {
                    product.IndexStatus = (int)Product.IndexStatusType.NeedsIndexing;
                }
            }

            UnitOfWork.Save();
        }

        #endregion Utility Methods
    }
}