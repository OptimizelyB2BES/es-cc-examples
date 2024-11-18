module alaska.cart {
    "use strict";

    import account = insite.account;
    import cart = insite.cart;
    import common = insite.common;
    import core = insite.core;
    import promotions = insite.promotions;
    import wishlist = insite.wishlist;
    import ICartScope = cart.ICartScope;
    import catalog = insite.catalog;

    export class AlaskaCartController extends cart.CartController {
        cart: CartModel;
        cartIdParam: string;
        promotions: PromotionModel[];
        settings: CartSettingsModel;
        showInventoryAvailability = false;
        productsCannotBePurchased = false;
        requiresRealTimeInventory = false;
        failedToGetRealTimeInventory = false;
        canAddAllToList = false;
        requisitionSubmitting = false;
        enableWarehousePickup = false;
        fulfillmentMethod: string;
        pickUpWarehouse: WarehouseModel;
        session: SessionModel;
        warehouseLeadTime: string;
        cartContainsNoPullLines: boolean = false;
        hasWtItems: boolean = false;

        static $inject = ["$scope", "cartService", "promotionService", "settingsService", "coreService", "$localStorage", "addToWishlistPopupService", "spinnerService", "sessionService", "ipCookie", "productRecommendationsService", "checkoutPopupService", "$location", "accountService"];

        constructor(
            protected $scope: ICartScope,
            protected cartService: IAlaskaCartService,
            protected promotionService: promotions.IPromotionService,
            protected settingsService: core.ISettingsService,
            protected coreService: core.ICoreService,
            protected $localStorage: common.IWindowStorage,
            protected addToWishlistPopupService: wishlist.AddToWishlistPopupService,
            protected spinnerService: core.ISpinnerService,
            protected sessionService: account.ISessionService,
            protected ipCookie: any,
            protected productRecommendationsService: catalog.ProductRecommendationsService,
            protected checkoutPopupService: alaska.cart.CheckoutPopupService,
            protected $location: ng.ILocationService,
            protected accountService: account.IAccountService) {
            super($scope, cartService, promotionService, settingsService, coreService, $localStorage, addToWishlistPopupService, spinnerService, sessionService, ipCookie, productRecommendationsService);

        }

        protected getSessionCompleted(session: SessionModel): void {
            this.session = session;
            this.fulfillmentMethod = session.fulfillmentMethod;
            this.pickUpWarehouse = session.pickUpWarehouse;
            if (this.fulfillmentMethod == "PickUp") {
                this.warehouseLeadTime = this.pickUpWarehouse.hours;
            }
        }

        protected onSessionUpdated(session: SessionModel): void {
            super.onSessionUpdated(session);
            if (session.fulfillmentMethod == "PickUp") {
                this.warehouseLeadTime = session.pickUpWarehouse.hours;
            }
        }

        protected getCartCompleted(cart: CartModel): void {
            super.getCartCompleted(cart);
            this.hasWtItems = false;
            cart.cartLines.forEach(cartLine => {
                if (cartLine.notes.includes("warehouse transfer")) {
                    this.hasWtItems = true;
                    this.cart.showLineNotes = true;
                }
            });
        }

        protected validateCartSubmission($event: Event) {
            $event.preventDefault();
            this.spinnerService.show();
            this.cartService.getCartWithLinesForReview().then(
                (cartWithLinesForReview: CartModel) => { this.checkCartLinesForReview(cartWithLinesForReview); }
            );
        }

        protected checkCartLinesForReview(cartWithLinesForReview: CartModel) {
            if (cartWithLinesForReview.cartLines.length > 0) {
                this.triggerModal();
            } else {
                var checkoutPath = $('#alaska_checkout_link').attr('href');
                if (checkoutPath) {
                    this.$location.path(checkoutPath);
                } else {
                    //??? fail case? Address page?
                }
            }
        }

        protected triggerModal(): void {
            this.checkoutPopupService.display(null);
        }

        displayCart(cart: CartModel): void {
            super.displayCart(cart);
            this.cartService.getRealTimeInventory(this.cart).then(() => {
                this.checkForNoPullLines();
            });
        }

        protected checkForNoPullLines(): void {
            this.cartContainsNoPullLines = this.cart.cartLines.some(o => o.isNoPull);
        }
    }

    angular
        .module("insite")
        .controller("CartController", AlaskaCartController);
}
