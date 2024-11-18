using Insite.Search.Elasticsearch.DocumentTypes.Product;
using Nest;
using System.Collections.Generic;

namespace Extensions.Plugins.Search.Elasticsearch.DocumentTypes.Product
{
    [ElasticsearchType(Name = "product")]
    public class ElasticsearchProductAlternatePartNumbers : ElasticsearchProduct
    {
        public ElasticsearchProductAlternatePartNumbers()
        {
        }

        public ElasticsearchProductAlternatePartNumbers(ElasticsearchProduct elasticsearchProduct)
        {
            this.BasicListPrice = elasticsearchProduct.BasicListPrice;
            this.BasicSaleEndDate = elasticsearchProduct.BasicSaleEndDate;
            this.BasicSalePrice = elasticsearchProduct.BasicSalePrice;
            this.BasicSaleStartDate = elasticsearchProduct.BasicSaleStartDate;
            this.BrandFacet = elasticsearchProduct.BrandFacet;
            this.BrandId = elasticsearchProduct.BrandId;
            this.BrandIsSponsored = elasticsearchProduct.BrandIsSponsored;
            this.BrandLogoAltText = elasticsearchProduct.BrandLogoAltText;
            this.BrandLogoLargeImagePath = elasticsearchProduct.BrandLogoLargeImagePath;
            this.BrandLogoSmallImagePath = elasticsearchProduct.BrandLogoSmallImagePath;
            this.BrandLogoSmallImagePathExists = elasticsearchProduct.BrandLogoSmallImagePathExists;
            this.BrandManufacturer = elasticsearchProduct.BrandManufacturer;
            this.BrandName = elasticsearchProduct.BrandName;
            this.BrandNameFirstCharacter = elasticsearchProduct.BrandNameFirstCharacter;
            this.BrandNameSort = elasticsearchProduct.BrandNameSort;
            this.BrandProductLineFacet = elasticsearchProduct.BrandProductLineFacet;
            this.BrandSearchBoost = elasticsearchProduct.BrandSearchBoost;
            this.BrandUrlSegment = elasticsearchProduct.BrandUrlSegment;
            this.Boost = elasticsearchProduct.Boost;
            this.CantBuy = elasticsearchProduct.CantBuy;
            this.Categories = elasticsearchProduct.Categories;
            this.CategoryNames = elasticsearchProduct.CategoryNames;
            this.CategoryTree = elasticsearchProduct.CategoryTree;
            this.Content = elasticsearchProduct.Content;
            this.ConfigurationType = elasticsearchProduct.ConfigurationType;
            this.CustomProperties = elasticsearchProduct.CustomProperties;
            this.CustomerNames = elasticsearchProduct.CustomerNames;
            this.Customers = elasticsearchProduct.Customers;
            this.CustomerNamesDisplay = elasticsearchProduct.CustomerNamesDisplay;
            this.CustomersFrequentlyPurchased = elasticsearchProduct.CustomersFrequentlyPurchased;
            this.CustomersLessFrequentlyPurchased = elasticsearchProduct.CustomersLessFrequentlyPurchased;
            this.CustomerUnitOfMeasures = elasticsearchProduct.CustomerUnitOfMeasures;
            this.DefaultVisibility = elasticsearchProduct.DefaultVisibility;
            this.DocumentNames = elasticsearchProduct.DocumentNames;
            this.ErpDescription = elasticsearchProduct.ErpDescription;
            this.ErpNumber = elasticsearchProduct.ErpNumber;
            this.ErpNumberWithoutSpecialCharacters = elasticsearchProduct.ErpNumberWithoutSpecialCharacters;
            this.ErpNumberNgram = elasticsearchProduct.ErpNumberNgram;
            this.ErpNumberSort = elasticsearchProduct.ErpNumberSort;
            this.ErpNumberNgramWithoutSpecialCharacters = elasticsearchProduct.ErpNumberNgramWithoutSpecialCharacters;
            this.FilterNames = elasticsearchProduct.FilterNames;
            this.Filters = elasticsearchProduct.Filters;
            this.Id = elasticsearchProduct.Id;
            this.ImageAltText = elasticsearchProduct.ImageAltText;
            this.IsSponsored = elasticsearchProduct.IsSponsored;
            this.IsDiscontinued = elasticsearchProduct.IsDiscontinued;
            this.IsQuoteRequired = elasticsearchProduct.IsQuoteRequired;
            this.IsStocked = elasticsearchProduct.IsStocked;
            this.LanguageCode = elasticsearchProduct.LanguageCode;
            this.LargeImagePath = elasticsearchProduct.LargeImagePath;
            this.ManufacturerItem = elasticsearchProduct.ManufacturerItem;
            this.ManufacturerItemWithoutSpecialCharacters = elasticsearchProduct.ManufacturerItemWithoutSpecialCharacters;
            this.MediumImagePath = elasticsearchProduct.MediumImagePath;
            this.MetaDescription = elasticsearchProduct.MetaDescription;
            this.MetaKeywords = elasticsearchProduct.MetaKeywords;
            this.ModelNumber = elasticsearchProduct.ModelNumber;
            this.ModifiedOn = elasticsearchProduct.ModifiedOn;
            this.ManufacturerItemNgram = elasticsearchProduct.ManufacturerItemNgram;
            this.ManufacturerItemNgramWithoutSpecialCharacters = elasticsearchProduct.ManufacturerItemNgramWithoutSpecialCharacters;
            this.MinimumOrderQty = elasticsearchProduct.MinimumOrderQty;
            this.Name = elasticsearchProduct.Name;
            this.NameSort = elasticsearchProduct.NameSort;
            this.PackDescription = elasticsearchProduct.PackDescription;
            this.PageTitle = elasticsearchProduct.PageTitle;
            this.Price = elasticsearchProduct.Price;
            this.PriceFacet = elasticsearchProduct.PriceFacet;
            this.ProductCode = elasticsearchProduct.ProductCode;
            this.ProductId = elasticsearchProduct.ProductId;
            this.ProductUrlSegment = elasticsearchProduct.ProductUrlSegment;
            this.Personas = elasticsearchProduct.Personas;
            this.ProductLineFacet = elasticsearchProduct.ProductLineFacet;
            this.ProductLineId = elasticsearchProduct.ProductLineId;
            this.ProductLineIsFeatured = elasticsearchProduct.ProductLineIsFeatured;
            this.ProductLineIsSponsored = elasticsearchProduct.ProductLineIsSponsored;
            this.ProductLineName = elasticsearchProduct.ProductLineName;
            this.ProductLineSearchBoost = elasticsearchProduct.ProductLineSearchBoost;
            this.ProductLineUrlSegment = elasticsearchProduct.ProductLineUrlSegment;
            this.ProductUnitOfMeasures = elasticsearchProduct.ProductUnitOfMeasures;
            this.RestrictionGroups = elasticsearchProduct.RestrictionGroups;
            this.SearchLookup = elasticsearchProduct.SearchLookup;
            this.ShippingWeight = elasticsearchProduct.ShippingWeight;
            this.ShortDescription = elasticsearchProduct.ShortDescription;
            this.ShortDescriptionSort = elasticsearchProduct.ShortDescriptionSort;
            this.Sku = elasticsearchProduct.Sku;
            this.SmallImagePath = elasticsearchProduct.SmallImagePath;
            this.SortOrder = elasticsearchProduct.SortOrder;
            this.Specifications = elasticsearchProduct.Specifications;
            this.SpellingCorrection = elasticsearchProduct.SpellingCorrection;
            this.StyledChildren = elasticsearchProduct.StyledChildren;
            this.StyleClassId = elasticsearchProduct.StyleClassId;
            this.UnitOfMeasure = elasticsearchProduct.UnitOfMeasure;
            this.UnitOfMeasureDescription = elasticsearchProduct.UnitOfMeasureDescription;
            this.Unspsc = elasticsearchProduct.Unspsc;
            this.UpcCode = elasticsearchProduct.UpcCode;
            this.UnitOfMeasureDisplay = elasticsearchProduct.UnitOfMeasureDisplay;
            this.TrackInventory = elasticsearchProduct.TrackInventory;
            this.Vendor = elasticsearchProduct.Vendor;
            this.Version = elasticsearchProduct.Version;
            this.Websites = elasticsearchProduct.Websites;
            this.WebsiteFilters = elasticsearchProduct.WebsiteFilters;
        }

        [Keyword(Name = "alternatePartNumbers", Index = true)]
        public List<string> AlternatePartNumbers { get; set; }
    }
}