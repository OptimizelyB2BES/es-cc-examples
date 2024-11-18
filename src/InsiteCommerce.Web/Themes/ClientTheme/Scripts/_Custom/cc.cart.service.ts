module alaska.cart {
    "use strict";

    export interface IAlaskaCartService extends insite.cart.ICartService {
        removeSelectedCartLines(cartModel: CartModel): ng.IPromise<CartModel>;
        saveCartLinesForPickup(cartLineCollection: CartLineCollectionModel): ng.IPromise<CartLineCollectionModel>;
        saveCartLinesForDelivery(cartLineCollection: CartLineCollectionModel): ng.IPromise<CartLineCollectionModel>;
        getCartWithLinesForReview(): ng.IPromise<CartModel>;
    }

    export class AlaskaCartService extends insite.cart.CartService implements IAlaskaCartService {
        alaskaServiceUri = "api/v1/CustomCartLineCollection";
        namedCartServiceUri = "api/v1/namedcarts";
        cartSettings: CartSettingsModel;

        static $inject = ["$http", "$rootScope", "$q", "addressErrorPopupService", "addToCartPopupService", "apiErrorPopupService", "httpWrapperService", "productService", "productPriceService", "invalidPricePopupService", "$window"];

        constructor(
            protected $http: ng.IHttpService,
            protected $rootScope: ng.IRootScopeService,
            protected $q: ng.IQService,
            protected addressErrorPopupService: insite.cart.IAddressErrorPopupService,
            protected addToCartPopupService: insite.cart.IAddToCartPopupService,
            protected apiErrorPopupService: insite.core.IApiErrorPopupService,
            protected httpWrapperService: insite.core.HttpWrapperService,
            protected productService: insite.catalog.IProductService,
            protected productPriceService: insite.catalog.IProductPriceService,
            protected invalidPricePopupService: insite.cart.IInvalidPricePopupService,
            protected $window: ng.IWindowService) {
            super($http,
                $rootScope,
                $q,
                addressErrorPopupService,
                addToCartPopupService,
                apiErrorPopupService,
                httpWrapperService,
                productPriceService,
                invalidPricePopupService,
                $window
            );
        }

        saveCartLinesForPickup(cartLineCollection: CartLineCollectionModel): ng.IPromise<CartLineCollectionModel> {
            return this.httpWrapperService.executeHttpRequest(
                this,
                this.$http({ url: this.namedCartServiceUri + "/addToPickupCart", method: "POST", data: cartLineCollection }),
                this.saveCartLinesForPickupCompleted,
                this.saveCartLinesForPickupFailed);
        }

        protected saveCartLinesForPickupCompleted(): void {
        }

        protected saveCartLinesForPickupFailed(): void {
        }

        saveCartLinesForDelivery(cartLineCollection: CartLineCollectionModel): ng.IPromise<CartLineCollectionModel> {
            return this.httpWrapperService.executeHttpRequest(
                this,
                this.$http({ url: this.namedCartServiceUri + "/addToShippingCart", method: "POST", data: cartLineCollection }),
                this.saveCartLinesForDeliveryCompleted,
                this.saveCartLinesForDeliveryFailed);
        }

        protected saveCartLinesForDeliveryCompleted(): void {
        }

        protected saveCartLinesForDeliveryFailed(): void {
        }

        removeSelectedCartLines(cartModel: CartModel): ng.IPromise<CartModel> {
            return this.httpWrapperService.executeHttpRequest(
                this,
                this.$http({ url: this.alaskaServiceUri + "/RemoveSelectedCartLines/" + cartModel.id, method: "POST", data: cartModel }),
                this.removeSelectedCartLinesCompleted,
                this.removeSelectedCartLinesFailed
            );
        }

        protected removeSelectedCartLinesCompleted(response: ng.IHttpPromiseCallbackArg<string>): void {
            this.getCart();
            this.$rootScope.$broadcast("cartChanged");
        }

        protected removeSelectedCartLinesFailed(error: ng.IHttpPromiseCallbackArg<any>): void {
        }

        getCartWithLinesForReview(): ng.IPromise<CartModel> {
            this.expand = "cartlines,costcodes,hiddenproducts";

            var currentCart;
            var productCollection;
            return this.getCart().then(
                (response: CartModel) => {
                    currentCart = response;
                    return response
                }
            ).then((cart: CartModel) => {
                var ids: System.Guid[] = [];
                cart.cartLines.forEach(cartLine => {
                    if (!ids.includes(cartLine.productId)) {
                        ids.push(cartLine.productId);
                    }
                });
                var param: IProductCollectionParameters = { productIds: ids };
                return this.productService.getProducts(param).then((products: ProductCollectionModel) => {
                    productCollection = products;
                    return this.getRealTimeInventory(currentCart).then((inventory: RealTimeInventoryModel) => {
                        return this.filterValidCartLines(currentCart, productCollection);
                    });
                })
            });
        }

        protected getRealTimeInventoryCompleted(response: ng.IHttpPromiseCallbackArg<RealTimeInventoryModel>, cart: CartModel): void {
            cart.cartLines.forEach((cartLine: CartLineModel) => {
                const productInventory = response.data.realTimeInventoryResults.find((productInventory: ProductInventoryDto) => productInventory.productId === cartLine.productId);
                if (productInventory) {
                    cartLine.qtyOnHand = productInventory.qtyOnHand;
                    var inventoryAvailability = productInventory.inventoryAvailabilityDtos.find(o => o.unitOfMeasure === cartLine.unitOfMeasure);
                    //Begin AIH-437 Customization
                    var warehouseAvailability = productInventory.inventoryWarehousesDtos.find(o => o.unitOfMeasure === cartLine.unitOfMeasure);
                    if (inventoryAvailability && warehouseAvailability) {
                        cartLine.availability = inventoryAvailability.availability;
                        if (!cart.hasInsufficientInventory && !cartLine.canBackOrder && !cartLine.quoteRequired && (inventoryAvailability.availability as any).messageType == 2) {
                            cart.hasInsufficientInventory = true;
                        }
                        cartLine.warehouseAvailability = warehouseAvailability;
                        if (productInventory.additionalResults["isNoPull"] && productInventory.additionalResults["isNoPull"] == "true") {
                            cartLine.isNoPull = true;
                            cartLine.notes = "";
                        }
                        if (productInventory.additionalResults["zeroAtStockingWarehouse"]) {
                            var number = Number(productInventory.additionalResults["zeroAtStockingWarehouse"]);
                            if (cartLine.qtyOrdered > number) {
                                cartLine.isNoPull = true;
                                cartLine.notes = "";
                            }
                        }
                        //End AIH-437 Customization
                    } else {
                        cartLine.availability = { messageType: 0 };
                    }
                } else {
                    cartLine.availability = { messageType: 0 };
                }
            });
        }

        protected filterValidCartLines(cart: CartModel, products: ProductCollectionModel): CartModel {
            var linesToReturn = [];
            for (let cartLine of cart.cartLines) {
                var product: ProductDto = products.products.find(function (product: ProductDto) {
                    return product.id === cartLine.productId;
                });                
                if (product) {
                    var code = this.getHazardCode(product.productCode);
                    var shipCode = this.getShippingCode(product.productCode);
                    var added = false;
                    if (cartLine.isNoPull) {
                        linesToReturn.push(cartLine);
                        added = true;
                    }
                    if (code != null && !(code.trim() === '')) {
                        if (cart.fulfillmentMethod != "PickUp") {
                            cartLine.hazardCode = code;
                            if (!added) {
                                linesToReturn.push(cartLine);
                                added = true;
                            }
                        }
                    }
                    else if (shipCode != null && !(shipCode.trim() === '')) {
                        if (cart.fulfillmentMethod != "PickUp") {
                            cartLine.shipRestrictionCode = shipCode;
                            if (!added) {
                                linesToReturn.push(cartLine);
                                added = true;
                            }
                        }
                    }
                } 
            }
            cart.cartLines = linesToReturn;
            return cart;
        }

        protected getCartCompleted(response: ng.IHttpPromiseCallbackArg<CartModel>, cartId: string): void {
            super.getCartCompleted(response, cartId);
            this.postProcessCarts([response.data]);
        }

        protected getCartsCompleted(response: ng.IHttpPromiseCallbackArg<CartCollectionModel>): void {
            if (response.data && response.data.carts) {
                this.postProcessCarts(response.data.carts);
            }
        }

        protected postProcessCarts(carts: CartModel[]) {
            for (let cart of carts) {
                if (cart.properties["savedName"]) {
                    var savedName = cart.properties["savedName"];
                    delete cart.properties["savedName"];
                    cart.isNamed = true;
                    if (savedName == "Pickup") {
                        cart.isSavedForPickup = true;
                    } else {
                        cart.isSavedForPickup = false;
                    }
                } else {
                    cart.isNamed = false;
                }
            }
        }

        protected getHazardCode(codes: string): string {
            var codeList = codes.split(',');
            for (let code of codeList) {
                if (code === "1" || code === "2" || code === "3" || code === "4") {
                    return code;
                }
            }
            return "";
        }

        protected getShippingCode(codes: string): string {
            var codeList = codes.split(',');
            for (let code of codeList) {
                if (code === "99" || code === "9") {
                    return code;
                }
            }
            return "";
        }
    }

    angular
        .module("insite")
        .service("cartService", AlaskaCartService);
}
