namespace Extensions.Plugins.Search.Index.Pipelines.Pipes.GetIndexableProducts
{
    using Insite.Core.Interfaces.Data;
    using Insite.Core.Plugins.Pipelines;
    using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Parameters;
    using Insite.Search.Elasticsearch.DocumentTypes.Product.Index.Pipelines.Results;

    public class IndexableProductsSqlStatementParts : IPipe<GetIndexableProductsParameter, GetIndexableProductsResult>
    {
        public const string IncrementalFilterSqlStatement = " AND p.IndexStatus = 1";

        public const string IndexableProductsSqlStatement = @"
        SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

        DECLARE @dateTimeNow DateTime
        SELECT @dateTimeNow = GETUTCDATE()
        DECLARE @defaultLanguageId NVARCHAR(50)
        SELECT @defaultLanguageId = Id FROM Language  WHERE IsDefault = 1
        DECLARE @defaultPersonaId NVARCHAR(50)
        SELECT @defaultPersonaId = Id FROM Persona WHERE IsDefault = 1

        CREATE TABLE #InactiveStyleParent
        (
            Id uniqueidentifier not null
        )

        INSERT INTO #InactiveStyleParent (Id)
        SELECT
            p1.StyleParentId
        FROM
            (SELECT StyleParentId, COUNT(*) AS 'child' FROM product where StyleParentId is not null GROUP BY StyleParentId) p1
        LEFT JOIN
            (SELECT StyleParentId, COUNT(*) AS 'activeChild' FROM product where StyleParentId is not null AND ActivateOn <= @dateTimeNow AND (DeactivateOn IS NULL OR DeactivateOn > @dateTimeNow) GROUP BY StyleParentId) p2
        ON
            p1.StyleParentId = p2.StyleParentId
        WHERE
            p1.child > 0 AND p2.activeChild is null

        CREATE TABLE #ActiveLanguages
        (
            Id uniqueidentifier not null,
            LanguageCode nvarchar(50) not null
        )

        INSERT INTO #ActiveLanguages (Id, LanguageCode)
        SELECT DISTINCT
            l.Id,
            l.LanguageCode
        FROM
            Language l
            INNER JOIN WebSiteLanguage wl ON wl.LanguageId = l.Id AND wl.IsLive = 1
            INNER JOIN Website w ON w.Id = wl.WebSiteId AND w.IsActive = 1

        -- Aggregate the category information, it is used in many sub queries and when you need to see if a product is in an active category
        CREATE TABLE #CategoryInfo
        (
            LanguageId uniqueidentifier not null,
            ProductId uniqueidentifier not null,
            CategoryId uniqueidentifier not null,
            WebsiteId nvarchar(40) not null,
            ShortDescription nvarchar(max),
            CategoryTree nvarchar(max),
            CategoryTreeNames nvarchar(max),
            ProductSearchBoost decimal(18,5),
            UNIQUE CLUSTERED (ProductId, LanguageId, CategoryId)
        )

        INSERT INTO #CategoryInfo
        SELECT
            l.Id as LanguageId,
            cp.ProductId,
            ch1.Id AS CategoryId,
            CAST(ch1.WebsiteId AS NVARCHAR(40)) AS WebsiteId,
            COALESCE(NULLIF(tp1.TranslatedValue,''), ch1.ShortDescription) AS ShortDescription,
            convert(nvarchar(36), ch1.Id) + CHAR(255) +
                case when ch2.Id is null then '' else convert(nvarchar(36), ch2.Id) end  + CHAR(255) +
                case when ch3.Id is null then '' else convert(nvarchar(36), ch3.Id) end  + CHAR(255) +
                case when ch4.Id is null then '' else convert(nvarchar(36), ch4.Id) end  + CHAR(255) +
                case when ch5.Id is null then '' else convert(nvarchar(36), ch5.Id) end  + CHAR(255) +
                case when ch6.Id is null then '' else convert(nvarchar(36), ch6.Id) end
                as CategoryTree,
            COALESCE(NULLIF(tp1.TranslatedValue,''), ch1.ShortDescription) + ' ' +
                case when ch2.ShortDescription is null then '' else COALESCE(NULLIF(tp2.TranslatedValue,''), ch2.ShortDescription) + ' ' end  +
                case when ch3.ShortDescription is null then '' else COALESCE(NULLIF(tp3.TranslatedValue,''), ch3.ShortDescription) + ' ' end  +
                case when ch4.ShortDescription is null then '' else COALESCE(NULLIF(tp4.TranslatedValue,''), ch4.ShortDescription) + ' ' end  +
                case when ch5.ShortDescription is null then '' else COALESCE(NULLIF(tp5.TranslatedValue,''), ch5.ShortDescription) + ' ' end  +
                case when ch6.ShortDescription is null then '' else COALESCE(NULLIF(tp6.TranslatedValue,''), ch6.ShortDescription) + ' ' end
                as CategoryTreeNames,
            ch1.ProductSearchBoost
        FROM Product p
            INNER JOIN #ActiveLanguages l ON 1 = 1
            INNER JOIN CategoryProduct cp ON cp.ProductId = p.Id
            INNER JOIN Category ch1 ON ch1.Id = cp.CategoryId
            LEFT JOIN Category ch2 ON ch2.Id = ch1.ParentId
            LEFT JOIN Category ch3 ON ch3.Id = ch2.ParentId
            LEFT JOIN Category ch4 ON ch4.Id = ch3.ParentId
            LEFT JOIN Category ch5 ON ch5.Id = ch4.ParentId
            LEFT JOIN Category ch6 ON ch6.Id = ch5.ParentId
            LEFT JOIN TranslationProperty tp1 ON tp1.ParentId = ch1.Id AND tp1.ParentTable = 'Category' AND tp1.LanguageId = l.Id AND tp1.Name = 'ShortDescription'
            LEFT JOIN TranslationProperty tp2 ON tp2.ParentId = ch2.Id AND tp2.ParentTable = 'Category' AND tp2.LanguageId = l.Id AND tp2.Name = 'ShortDescription'
            LEFT JOIN TranslationProperty tp3 ON tp3.ParentId = ch3.Id AND tp3.ParentTable = 'Category' AND tp3.LanguageId = l.Id AND tp3.Name = 'ShortDescription'
            LEFT JOIN TranslationProperty tp4 ON tp4.ParentId = ch4.Id AND tp4.ParentTable = 'Category' AND tp4.LanguageId = l.Id AND tp4.Name = 'ShortDescription'
            LEFT JOIN TranslationProperty tp5 ON tp5.ParentId = ch5.Id AND tp5.ParentTable = 'Category' AND tp5.LanguageId = l.Id AND tp5.Name = 'ShortDescription'
            LEFT JOIN TranslationProperty tp6 ON tp6.ParentId = ch6.Id AND tp6.ParentTable = 'Category' AND tp6.LanguageId = l.Id AND tp6.Name = 'ShortDescription'
        WHERE ch1.ActivateOn <= @dateTimeNow AND (ch1.DeactivateOn IS NULL OR ch1.DeactivateOn > @dateTimeNow)
            AND (ch2.Id IS NULL OR (ch2.ActivateOn <= @dateTimeNow AND (ch2.DeactivateOn IS NULL OR ch2.DeactivateOn > @dateTimeNow)))
            AND (ch3.Id IS NULL OR (ch3.ActivateOn <= @dateTimeNow AND (ch3.DeactivateOn IS NULL OR ch3.DeactivateOn > @dateTimeNow)))
            AND (ch4.Id IS NULL OR (ch4.ActivateOn <= @dateTimeNow AND (ch4.DeactivateOn IS NULL OR ch4.DeactivateOn > @dateTimeNow)))
            AND (ch5.Id IS NULL OR (ch5.ActivateOn <= @dateTimeNow AND (ch5.DeactivateOn IS NULL OR ch5.DeactivateOn > @dateTimeNow)))
            AND (ch6.Id IS NULL OR (ch6.ActivateOn <= @dateTimeNow AND (ch6.DeactivateOn IS NULL OR ch6.DeactivateOn > @dateTimeNow)))
            AND p.IndexStatus <> 2
            AND p.ActivateOn <= @dateTimeNow
            AND (p.DeactivateOn IS NULL OR p.DeactivateOn > @dateTimeNow)
            {1}
        OPTION(OPTIMIZE FOR (@dateTimeNow = '99991231'))

        -- Aggregate the product information, it is not by language, so all sub queries that are not by language should be done here, so they don't have to be repeated for each language
        CREATE TABLE #ProductInfo
        (
            ProductId uniqueidentifier not null,
            StyleParentId uniqueidentifier,
            ContentManagerId uniqueidentifier,
            ConfigurationType nvarchar(50),
            LargeImagePath nvarchar(1024),
            MediumImagePath nvarchar(1024),
            SmallImagePath nvarchar(1024),
            ProductSearchBoost decimal(18,5),
            QtyOnHand int,
            Websites nvarchar(max),
            Price decimal(18,5),
            PriceFacet int,
            DocumentNames nvarchar(max),
            CustomerNames nvarchar(max),
            Customers nvarchar(max),
            RestrictionGroups nvarchar(max),
            CustomersFrequentlyPurchased nvarchar(max),
            CustomersLessFrequentlyPurchased nvarchar(max),
            Personas nvarchar(max),
            CustomProperties nvarchar(max)
            UNIQUE CLUSTERED (ProductId)
        )

        ;WITH ProductInfo AS
        (
            SELECT
                p.Id AS ProductId,
                p.StyleParentId,
                p.ContentManagerId,
                CASE WHEN p.ConfigurationObjectId IS NOT NULL THEN 'Advanced'
                     WHEN p.IsFixedConfiguration = 1 THEN 'Fixed'
                     WHEN p.IsConfigured = 1 THEN 'Standard'
                     ELSE 'None'
                END AS ConfigurationType,
                ProductImage.LargeImagePath,
                ProductImage.MediumImagePath,
                ProductImage.SmallImagePath,
                CategoryInfo.ProductSearchBoost,
                ProductWarehouse.QtyOnHand,
                STUFF(
                    ISNULL(
                        (SELECT
                            CHAR(255) + ci.WebSiteId AS [text()]
                        FROM #CategoryInfo ci
                        WHERE ci.ProductId = p.Id
                        GROUP BY WebsiteId
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS Websites,
                ISNULL(
                    (SELECT top 1 puom.QtyPerBaseUnitOfMeasure
                        FROM ProductUnitOfMeasure puom
                        WHERE puom.ProductId = p.Id AND puom.IsDefault = 1),
                    1)
                *
                (CASE
                    WHEN NOT p.BasicSaleStartDate IS NULL AND p.BasicSaleStartDate < @dateTimeNow AND (p.BasicSaleEndDate IS NULL OR p.BasicSaleEndDate > @dateTimeNow) THEN p.BasicSalePrice
                    ELSE p.BasicListPrice
                END) AS Price,
                (CASE WHEN (p.BasicListPrice <> 0) AND (p.IsConfigured = 0 OR p.IsFixedConfiguration = 1) AND (p.ConfigurationObjectId IS NULL) THEN
                    (CASE WHEN NOT p.BasicSaleStartDate IS NULL AND p.BasicSaleStartDate < @dateTimeNow AND (p.BasicSaleEndDate IS NULL OR p.BasicSaleEndDate > @dateTimeNow) THEN
                        (CASE WHEN p.BasicSalePrice >= 10 THEN
                            CAST(p.BasicSalePrice - p.BasicSalePrice % POWER(10,ROUND(LOG10(p.BasicSalePrice),0,1))as int)
                        ELSE 0 END)
                    ELSE
                        (CASE WHEN p.BasicListPrice >= 10 THEN
                            CAST(p.BasicListPrice - p.BasicListPrice % POWER(10,ROUND(LOG10(p.BasicListPrice),0,1))as int)
                        ELSE 0 END)
                    END)
                ELSE null
                END) AS PriceFacet,
                STUFF(
                    ISNULL(
                        (SELECT
                            CHAR(255) + d.Name AS [text()]
                        FROM Document d
                        WHERE d.ParentId = p.Id
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS DocumentNames,
                STUFF(
                    ISNULL(
                        (SELECT * FROM
                            (SELECT
                                CHAR(255) + CAST(cp.CustomerId AS NVARCHAR(40)) + CHAR(254) + customProp.Value + CHAR(254) + cp.UnitOfMeasure AS [text()]
                            FROM CustomerProduct cp
							LEFT JOIN CustomProperty customProp ON customProp.ParentId = cp.Id
                            WHERE cp.ProductId = p.Id
                            AND customProp.Value <> ''
                            UNION
                            SELECT
                                CHAR(255) + CAST(cp.CustomerId AS NVARCHAR(40)) + CHAR(254) + customProp.Value + CHAR(254) + cp.UnitOfMeasure AS [text()]
                            FROM Product sp
                            INNER JOIN CustomerProduct cp on cp.ProductId = sp.Id
							LEFT JOIN CustomProperty customProp ON customProp.ParentId = cp.Id
                            WHERE sp.StyleParentId = p.Id AND customProp.Value <> '') AS u
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS CustomerNames,
                STUFF(
                    ISNULL(
                        (SELECT
                            CHAR(255) + CAST(cp.CustomerId AS NVARCHAR(40)) AS [text()]
                        FROM CustomerProduct cp
						LEFT JOIN CustomProperty customProp ON customProp.ParentId = cp.Id
                        WHERE cp.ProductId = p.Id
                        AND customProp.Value <> ''
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS Customers,
                STUFF(
                    ISNULL(
                        (SELECT
                            CHAR(255) + CAST(rgp.RestrictionGroupId AS NVARCHAR(40)) AS [text()]
                        FROM RestrictionGroupProduct rgp
                        WHERE rgp.ProductId = p.Id
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS RestrictionGroups,
                '' AS CustomersFrequentlyPurchased, --CustomersFrequentlyPurchased
                '' AS CustomersLessFrequentlyPurchased, --CustomersLessFrequentlyPurchased
                STUFF(
                    ISNULL(
                        (SELECT
                            CHAR(255) + CAST(cp.PersonaId AS NVARCHAR(40)) AS [text()]
                        FROM CategoryPersona cp
                        JOIN #CategoryInfo ci ON ci.CategoryId = cp.CategoryId
                        WHERE ci.ProductId = p.Id
                        GROUP BY cp.PersonaId
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS Personas,
                STUFF(
                    ISNULL(
                        (SELECT
                            CONCAT(CHAR(255), pp.Name, CHAR(254), pp.Value)
                        FROM CustomProperty pp WITH (NOLOCK)
                        JOIN search.SearchBoost sb WITH (NOLOCK) ON
                            sb.EntityType = 'Product' AND
                            sb.Query = 'Field_customProperties.' + pp.Name AND
                            sb.IsQueryable = 1
                        WHERE pp.ParentId = p.Id
                        FOR XML PATH ('')),
                        ' '),
                    1, 1, '') AS CustomProperties
            FROM Product p
            OUTER APPLY
            (
                SELECT TOP 1
                    pi.LargeImagePath,
                    pi.MediumImagePath,
                    (CASE WHEN pi.SmallImagePath = '' THEN pi.MediumImagePath ELSE pi.SmallImagePath END) AS SmallImagePath
                FROM ProductImage pi
                WHERE pi.ProductId = p.Id ORDER BY pi.SortOrder
            ) ProductImage
            OUTER APPLY
            (
                SELECT MAX(ci.ProductSearchBoost) AS ProductSearchBoost FROM #CategoryInfo ci WHERE ci.ProductId = p.Id
            ) CategoryInfo
            OUTER APPLY
            (
                SELECT SUM(pw.ErpQtyAvailable - pw.QtyOnOrder) AS QtyOnHand FROM ProductWarehouse pw WHERE pw.ProductId = p.Id
            ) ProductWarehouse
            WHERE p.IndexStatus <> 2
            AND p.ActivateOn <= @dateTimeNow
            AND (p.DeactivateOn IS NULL OR p.DeactivateOn > @dateTimeNow)
            {1}
        )

        INSERT INTO #ProductInfo SELECT * FROM ProductInfo

        CREATE TABLE #CategoryProductIds ( ProductId uniqueidentifier primary key );

        INSERT INTO #CategoryProductIds
        SELECT DISTINCT ProductId FROM #CategoryInfo

        -- Aggregate the filter information, it is used in multiple sub-queries and expensive to get
        CREATE TABLE #FilterInfo
        (
            LanguageId uniqueidentifier not null,
            StyleParentId uniqueidentifier,
            ProductId uniqueidentifier not null,
            Filters nvarchar(max) not null,
            FilterNames nvarchar(max) not null
        )

        INSERT INTO #FilterInfo
        SELECT
            l.Id,
            p.StyleParentId,
            p.ProductId,
            CONVERT (nvarchar(50),wcat.WebsiteId)
                + CONVERT (nvarchar(50),av.Id ) + CHAR(254) +
                + av.Value + CHAR(254)
                + CONVERT (nvarchar(10),av.SortOrder ) + CHAR(254)
                + CONVERT (nvarchar(50),a.Id ) + CHAR(254)
                + a.Label + CHAR(254)
                + CONVERT (nvarchar(10),MAX(wcat.SortOrder) OVER(PARTITION BY wcat.WebsiteId, a.Id, av.Id)) + CHAR(254)
                + CONVERT (nvarchar(1),a.IsFilter )
                AS Filters,
            CONVERT (nvarchar(50),wcat.WebsiteId ) +
            COALESCE(NULLIF(td.Translation, ''),av.Value) AS FilterNames
        FROM ProductAttributeValue pav
        INNER HASH JOIN AttributeValue av  ON av.Id = pav.AttributeValueId
        INNER HASH JOIN AttributeType a  ON a.Id = av.AttributeTypeId
        INNER HASH JOIN #ProductInfo p  ON p.ProductId = pav.ProductId
        INNER HASH JOIN #CategoryProductIds cpi ON cpi.ProductId = p.ProductId
        INNER LOOP JOIN #ActiveLanguages l  ON 1 = 1
        LEFT JOIN TranslationDictionary td  ON td.LanguageId = l.Id AND td.Keyword = av.Value AND td.Source = 'AttributeValue'
        INNER JOIN (
            SELECT
                ci.WebSiteId,
                ci.ProductId,
                cat.AttributeTypeId,
                MAX(cat.SortOrder) AS SortOrder
            FROM CategoryAttributeType cat
            INNER  JOIN #CategoryInfo ci ON ci.CategoryId = cat.CategoryId
            WHERE cat.IsActive = 1
            GROUP BY ci.WebSiteId, ci.ProductId, cat.AttributeTypeId
        ) wcat ON wcat.ProductId = p.ProductId AND wcat.AttributeTypeId = a.Id
        WHERE a.IsActive = 1
        AND a.IsSearchable = 1
        AND av.IsActive = 1

        CREATE INDEX I1 ON #FilterInfo(ProductId,LanguageId) INCLUDE (StyleParentId, Filters, FilterNames) WHERE StyleParentId IS NULL
        CREATE INDEX I2 ON #FilterInfo(ProductId,LanguageId,StyleParentId) INCLUDE (Filters, FilterNames) WHERE StyleParentId IS NOT NULL

        -- the main select statement, if possible, for single sub queries that are by language, they should be done here so you don't just copy information to a temp table to then just select it here
        SELECT
            l.LanguageCode,
            p.Id AS ProductId,
            p.Name,
            p.ERPNumber,
            COALESCE(NULLIF(productShortDescriptionTp.TranslatedValue,''), NULLIF(p.ShortDescription,''), p.ErpDescription) as ShortDescription,
            p.SortOrder,
            p.SearchBoost,
            pi.ProductSearchBoost as CategorySearchBoost,
            p.ManufacturerItem,
            pi.LargeImagePath,
            pi.MediumImagePath,
            pi.SmallImagePath,
            p.UnitOfMeasure,
            COALESCE(NULLIF(uomDisplayTd.Translation, ''),p.UnitOfMeasure) AS UnitOfMeasureDisplay,
            p.BasicListPrice,
            p.BasicSalePrice,
            p.BasicSaleStartDate,
            p.BasicSaleEndDate,
            p.ShippingWeight,
            p.ActivateOn,
            p.DeactivateOn,
            p.SearchLookup,
            p.DefaultVisibility,
            COALESCE(NULLIF(productUrlSegmentTp.TranslatedValue, ''),p.UrlSegment) AS UrlSegment,
            p.ModifiedOn,
            p.IsSponsored,
            p.ProductCode,
            p.ErpDescription,
            p.MetaDescription,
            p.MetaKeywords,
            p.PageTitle,
            p.ModelNumber,
            p.Sku,
            p.UpcCode,
            p.Unspsc,
            p.PackDescription,
            COALESCE(NULLIF(uomDescriptionTd.Translation, ''),p.UnitOfMeasureDescription) AS UnitOfMeasureDescription,
            p.IsStocked,
            CONCAT(v.Name, ' ', v.VendorNumber) as Vendor,
            p.TrackInventory,
            p.IsDiscontinued,
            p.IsQuoteRequired,
            p.MinimumOrderQty,
            p.StyleClassId,
            p.BrandId as BrandId,
            COALESCE(NULLIF(brandNameTp.TranslatedValue,''), b.Name) AS BrandName,
            LOWER(LEFT(COALESCE(NULLIF(brandNameTp.TranslatedValue,''), b.Name),1)) AS BrandNameFirstCharacter,
            COALESCE(b.Manufacturer,'') AS BrandManufacturer,
            COALESCE(b.SearchBoost,0) AS BrandSearchBoost,
            ISNULL(b.IsSponsored, 0) AS BrandIsSponsored,
            COALESCE(NULLIF(brandUrlTp.TranslatedValue,''), b.UrlSegment) AS BrandUrlSegment,
            b.LogoSmallImagePath AS BrandLogoSmallImagePath,
            b.LogoLargeImagePath AS BrandLogoLargeImagePath,
            COALESCE(NULLIF(brandAltTextTp.TranslatedValue,''), b.LogoAltText) AS BrandLogoAltText,
            p.ProductLineId,
            COALESCE(NULLIF(productLineNameTp.TranslatedValue,''), pl.Name) AS ProductLineName,
            COALESCE(NULLIF(productLineUrlTp.TranslatedValue,''), pl.UrlSegment) AS ProductLineUrlSegment,
            ISNULL(pl.SearchBoost,0) AS ProductLineSearchBoost,
            ISNULL(pl.IsFeatured, 0) AS ProductLineIsFeatured,
            ISNULL(pl.IsSponsored, 0) AS ProductLineIsSponsored,
            pi.ConfigurationType,
            pi.DocumentNames,
            pi.Price,
            pi.PriceFacet,
            STUFF(
                ISNULL(
                    (SELECT
                        CHAR(255) + COALESCE(NULLIF(productImageTp.TranslatedValue,''), pim.AltText) AS [text()]
                    FROM ProductImage pim
                    LEFT JOIN TranslationProperty productImageTp ON productImageTp.ParentId = pim.Id AND productImageTp.ParentTable = 'ProductImage' AND productImageTp.LanguageId = l.Id AND productImageTp.Name = 'AltText'
                    WHERE pim.ProductId = p.Id
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS ImageAltText,
            STUFF(
                ISNULL(
                    (SELECT
                        CHAR(255) + CAST(c.CategoryId AS NVARCHAR(40)) + c.WebSiteId + c.ShortDescription AS [text()]
                    FROM #CategoryInfo c
                    WHERE c.LanguageId = l.Id AND c.ProductId = p.Id
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS Categories,
            pi.Personas,
            STUFF(
                ISNULL(
                    (SELECT
                        CHAR(255) + ci.CategoryTreeNames AS [text()]
                    FROM #CategoryInfo ci
                    WHERE ci.ProductId = p.Id and ci.LanguageId = l.Id
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS CategoryNames,
            STUFF(
                ISNULL(
                    (SELECT
                        CHAR(255) + CategoryTree AS [text()]
                    FROM #CategoryInfo ci
                    WHERE ci.ProductId = p.Id and ci.LanguageId = l.Id
                    FOR XML PATH ('')),
                     ' '),
                1, 1, '') AS CategoryTree,
            pi.Websites,
            STUFF(
                ISNULL(
                    (SELECT * FROM
                    (
                        SELECT DISTINCT
                            CHAR(255) + Filters AS [text()]
                        FROM #FilterInfo f
                        WHERE (f.StyleParentId is null AND f.LanguageId = l.Id AND f.ProductId = p.Id)
                        UNION
                        SELECT DISTINCT
                            CHAR(255) + Filters AS [text()]
                        FROM #FilterInfo f
                        WHERE (f.StyleParentId is not null AND f.LanguageId = l.Id AND f.ProductId = p.Id)
                        UNION
                        SELECT DISTINCT
                            CHAR(255) + Filters AS [text()]
                        FROM #FilterInfo f
                        WHERE (f.StyleParentId is not null AND f.LanguageId = l.Id AND f.StyleParentId = p.Id)
                    ) f
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS Filters,
            STUFF(
                ISNULL(
                    (SELECT * FROM
                    (
                        SELECT DISTINCT
                            CHAR(255) + FilterNames AS [text()]
                        FROM #FilterInfo f
                        WHERE (f.StyleParentId is null AND f.LanguageId = l.Id AND f.ProductId = p.Id)
                        UNION
                        SELECT DISTINCT
                            CHAR(255) + FilterNames AS [text()]
                        FROM #FilterInfo f
                        WHERE (f.StyleParentId is not null AND f.LanguageId = l.Id AND f.ProductId = p.Id)
                        UNION
                        SELECT DISTINCT
                            CHAR(255) + FilterNames AS [text()]
                        FROM #FilterInfo f
                        WHERE (f.StyleParentId is not null AND f.LanguageId = l.Id AND f.StyleParentId = p.Id)
                    ) f
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS FilterNames,
            STUFF(
                ISNULL(
                    (SELECT
                        CHAR(255) + sp.Name
                        + CHAR(255) + sp.ERPNumber
                        + CHAR(255) + COALESCE(NULLIF(productShortDescriptionTpSp.TranslatedValue,''), NULLIF(sp.ShortDescription,''), sp.ErpDescription)
                        + CHAR(255) + COALESCE(ProductImage.MediumImagePath, '')
                        + CHAR(255) + COALESCE(ProductImage.SmallImagePath, '')
                        + CHAR(255) + sp.ManufacturerItem
                        + CHAR(255) + CONVERT(nvarchar(36), sp.Id)
                        + CHAR(255) + sp.UpcCode
                        + CHAR(255) + sp.Sku
                        + CHAR(255) + sp.Unspsc
                        + CHAR(255) + COALESCE(NULLIF(td1.Translation, ''),stv.Value)
                        + CHAR(255) + COALESCE(NULLIF(td2.Translation, ''),st.Name)
                        + CHAR(255) + st.Description
                        + CHAR(255) + sp.ProductCode
                        + CHAR(255) + sp.ModelNumber AS [text()]
                    FROM Product sp
                    INNER JOIN StyleTraitValueProduct stvp on stvp.ProductId = sp.Id
                    INNER JOIN StyleTraitValue stv on stv.Id = stvp.StyleTraitValueId
                    INNER JOIN StyleTrait st on st.Id = stv.StyleTraitId
                    LEFT JOIN #ProductInfo pisp ON pisp.ProductId = sp.Id
                    LEFT JOIN TranslationDictionary td1 ON td1.LanguageId = l.Id AND td1.Keyword = stv.Value AND td1.Source = 'StyleTraitValue'
                    LEFT JOIN TranslationDictionary td2 ON td2.LanguageId = l.Id AND td2.Keyword = st.Name AND td2.Source = 'StyleTrait'
                    LEFT JOIN TranslationProperty productShortDescriptionTpSp ON productShortDescriptionTpSp.LanguageId = l.Id AND productShortDescriptionTpSp.ParentTable = 'Product' AND productShortDescriptionTpSp.ParentId = sp.Id AND productShortDescriptionTpSp.Name = 'ShortDescription'
                    OUTER APPLY
                    (
                        SELECT TOP 1
                            pi.MediumImagePath,
                            (CASE WHEN pi.SmallImagePath = '' THEN pi.MediumImagePath ELSE pi.SmallImagePath END) AS SmallImagePath
                        FROM ProductImage pi
                        WHERE pi.ProductId = sp.Id ORDER BY pi.SortOrder
                    ) ProductImage
                    WHERE sp.StyleParentId = p.Id
                    AND sp.ActivateOn <= @dateTimeNow
                    AND (sp.DeactivateOn IS NULL OR sp.DeactivateOn > @dateTimeNow)
                    AND (sp.IsDiscontinued = 0 OR (pisp.QtyOnHand > 0 AND sp.TrackInventory = 1))
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS StyledChildren,
            STUFF(
                ISNULL(
                    (SELECT
                        CHAR(255) + CAST(puom.Id AS NVARCHAR(40))
                        + CHAR(254) + puom.UnitOfMeasure
                        + CHAR(254) + COALESCE(NULLIF(td1.Translation, ''),puom.UnitOfMeasure)
                        + CHAR(254) + COALESCE(NULLIF(td2.Translation, ''),puom.Description)
                        + CHAR(254) + CAST(puom.QtyPerBaseUnitOfMeasure AS NVARCHAR(20))
                        + CHAR(254) + puom.RoundingRule
                        + CHAR(254) + CAST(puom.IsDefault AS NVARCHAR(5)) AS [text()]
                    FROM ProductUnitOfMeasure puom
                    LEFT JOIN TranslationDictionary td1 ON td1.LanguageId = l.Id AND td1.Keyword = puom.UnitOfMeasure AND td1.Source = 'UnitOfMeasure'
                    LEFT JOIN TranslationDictionary td2 ON td2.LanguageId = l.Id AND td2.Keyword = puom.Description AND td2.Source = 'UnitOfMeasure'
                    WHERE puom.ProductId = p.Id
                    FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS ProductUnitOfMeasures,
            pi.CustomerNames,
            pi.Customers,
            pi.RestrictionGroups,
            pi.CustomersFrequentlyPurchased,
            pi.CustomersLessFrequentlyPurchased,
            STUFF(
                ISNULL(
                    (SELECT TOP 1
                        c.Html
                    FROM Content c
                    WHERE c.ContentManagerId = p.ContentManagerId AND NOT c.PublishToProductionOn IS NULL AND c.PublishToProductionOn <= @dateTimeNow
                    AND (c.LanguageId = l.Id or c.LanguageId = @defaultLanguageId)
                        ORDER BY
                            CASE WHEN c.LanguageId = l.Id THEN 0 ELSE 1 END,
                            CASE WHEN c.PersonaId = @defaultPersonaId THEN 0 ELSE 1 END,
                            CASE WHEN c.DeviceType = 'Desktop' THEN 0 ELSE 1 END,
                            c.PublishToProductionOn DESC),
                    ' '),
                1, 1, '') AS Content,
            STUFF(
                ISNULL(
                    (SELECT
                        (SELECT TOP 1
                            c.Html
                        FROM Content c
                        WHERE c.ContentManagerId = s.ContentManagerId AND NOT c.PublishToProductionOn IS NULL AND c.PublishToProductionOn <= @dateTimeNow
                        AND (c.LanguageId = l.Id or c.LanguageId = @defaultLanguageId)
                            ORDER BY
                                CASE WHEN c.LanguageId = l.Id THEN 0 ELSE 1 END,
                                CASE WHEN c.PersonaId = @defaultPersonaId THEN 0 ELSE 1 END,
                                CASE WHEN c.DeviceType = 'Desktop' THEN 0 ELSE 1 END,
                                c.PublishToProductionOn DESC)
                        AS [text()]
                    FROM Specification s WHERE s.ProductId = p.Id FOR XML PATH ('')),
                    ' '),
                1, 1, '') AS Specifications,
            pi.CustomProperties
            {0} -- CustomFields
        FROM Product p
        JOIN #ActiveLanguages l  ON 1 = 1
        LEFT JOIN Vendor v ON v.Id = p.VendorId
        LEFT JOIN TranslationProperty productShortDescriptionTp  ON productShortDescriptionTp.LanguageId = l.Id AND productShortDescriptionTp.ParentTable = 'Product' AND productShortDescriptionTp.ParentId = p.Id AND productShortDescriptionTp.Name = 'ShortDescription'
        LEFT JOIN TranslationProperty productUrlSegmentTp  ON productUrlSegmentTp.LanguageId = l.Id AND productUrlSegmentTp.ParentTable = 'Product' AND productUrlSegmentTp.ParentId = p.Id AND productUrlSegmentTp.Name = 'UrlSegment'
        LEFT JOIN #ProductInfo pi ON pi.ProductId = p.Id
        LEFT JOIN Brand b ON b.Id = p.BrandId
        LEFT JOIN ProductLine pl ON pl.Id = p.ProductLineId
        LEFT JOIN TranslationProperty brandNameTp  ON brandNameTp.ParentId = b.Id AND brandNameTp.ParentTable = 'Brand' AND brandNameTp.LanguageId = l.Id AND brandNameTp.Name = 'Name'
        LEFT JOIN TranslationProperty brandUrlTp  ON brandUrlTp.ParentId = b.Id AND brandUrlTp.ParentTable = 'Brand' AND brandUrlTp.LanguageId = l.Id AND brandUrlTp.Name = 'UrlSegment'
        LEFT JOIN TranslationProperty brandAltTextTp  ON brandAltTextTp.ParentId = b.Id AND brandAltTextTp.ParentTable = 'Brand' AND brandAltTextTp.LanguageId = l.Id AND brandAltTextTp.Name = 'logoAltText'
        LEFT JOIN TranslationProperty productLineNameTp  ON productLineNameTp.ParentId = pl.Id AND productLineNameTp.ParentTable = 'ProductLine' AND productLineNameTp.LanguageId = l.Id AND productLineNameTp.Name = 'Name'
        LEFT JOIN TranslationProperty productLineUrlTp  ON productLineUrlTp.ParentId = pl.Id AND productLineUrlTp.ParentTable = 'ProductLine' AND productLineUrlTp.LanguageId = l.Id AND productLineUrlTp.Name = 'UrlSegment'
        LEFT JOIN TranslationDictionary uomDisplayTd ON uomDisplayTd.LanguageId = l.Id AND uomDisplayTd.Keyword = p.UnitOfMeasure AND uomDisplayTd.Source = 'UnitOfMeasure'
        LEFT JOIN TranslationDictionary uomDescriptionTd ON uomDescriptionTd.LanguageId = l.Id AND uomDescriptionTd.Keyword = p.UnitOfMeasureDescription AND uomDescriptionTd.Source = 'UnitOfMeasure'
        WHERE p.StyleParentId IS NULL
        AND p.ActivateOn <= @dateTimeNow
        AND (p.DeactivateOn IS NULL OR p.DeactivateOn > @dateTimeNow)
        AND p.IndexStatus <> 2
        AND (p.IsDiscontinued = 0 OR (pi.QtyOnHand > 0 AND p.TrackInventory = 1))
        AND p.Id NOT IN (SELECT Id FROM #InactiveStyleParent)
        {1}
        OPTION(OPTIMIZE FOR (@dateTimeNow = '99991231'))

        DROP TABLE #ActiveLanguages
        DROP TABLE #CategoryInfo
        DROP TABLE #ProductInfo
        DROP TABLE #FilterInfo
        DROP TABLE #InactiveStyleParent
        ";

        public int Order => 100;

        public GetIndexableProductsResult Execute(IUnitOfWork unitOfWork, GetIndexableProductsParameter parameter, GetIndexableProductsResult result)
        {
            result.SqlStatement = IndexableProductsSqlStatement;
            result.IncrementalFilter = parameter.IsIncremental
                ? IncrementalFilterSqlStatement
                : string.Empty;

            return result;
        }
    }
}