declare module Insite.Cart.WebApi.V1.ApiModels {
    interface CartLineModel {
        hazardCode: string;
        shipRestrictionCode: string;
        isNoPull: boolean;
        warehouseAvailability: Insite.Core.Plugins.Inventory.InventoryWarehousesDto;
    }
    interface CartModel {
        isNamed: boolean;
        isSavedForPickup: boolean;
    }

    interface CartSettingsModel {
        noPullWarehouseCode: string;
    }
}

declare module Insite.Catalog.Services.Dtos {
    interface StyledProductDto {
        attributeTypes: Insite.Catalog.Services.Dtos.AttributeTypeDto[];
        documents: Insite.Catalog.Services.Dtos.DocumentDto[];
        specifications: Insite.Catalog.Services.Dtos.SpecificationDto[];
        htmlContent: string;
    }
}