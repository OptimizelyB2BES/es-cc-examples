module insite.cart {
    "use strict";

    export class AlaskaCartLinesController extends CartLinesController {
        session: SessionModel;
        cartModel: CartModel;
        subscriptionStatus: any = [];

        static $inject = ["$scope", "cartService", "productSubscriptionPopupService", "addToWishlistPopupService", "spinnerService", "settingsService", "sessionService", "subscriptionsService", "$rootScope"];

        constructor(
            protected $scope: ICartScope,
            protected cartService: ICartService,
            protected productSubscriptionPopupService: catalog.ProductSubscriptionPopupService,
            protected addToWishlistPopupService: wishlist.AddToWishlistPopupService,
            protected spinnerService: core.ISpinnerService,
            protected settingsService: core.ISettingsService,
            protected sessionService: account.ISessionService,
            protected subscriptionsService: alaska.subscriptions.SubscriptionsService,
            protected $rootScope: ng.IRootScopeService) {
            super($scope, cartService, productSubscriptionPopupService, addToWishlistPopupService, spinnerService, settingsService);
            this.getSession();
        }

        protected getSession(): void {
            this.sessionService.getSession().then(
                (session: SessionModel) => { this.getSessionCompleted(session); },
                (error: any) => { this.getSessionFailed(error); });
        }

        protected getSessionCompleted(session: SessionModel): void {
            this.session = session;
        }
        
        protected getSessionFailed(error: any): void {
        }

        protected getSettingsCompleted(settingsCollection: core.SettingsCollection): void {
            this.productSettings = settingsCollection.productSettings;
            this.getProductSubscriptions();
        }

        getProductSubscriptions(): void {
            if (this.$scope.cart == null) return;
            var subscriptionString = "";
            var hit = false;
            for (var product of this.$scope.cart.cartLines) {
                subscriptionString += product.erpNumber;
                subscriptionString += ",";
                this.subscriptionStatus[product.erpNumber] = false;
                hit = true;
            }
            subscriptionString = subscriptionString.slice(0, -1);
            if (hit) {
                this.subscriptionsService.getProductStatusMulti(subscriptionString).then(
                    (subscriptions: any) => { this.getSubscriptionsCompleted(subscriptions) },
                    (error: any) => { this.getSubscriptionsFailed(error); }
                );
            }
        }

        protected getSubscriptionsCompleted(subscriptions: ProductDto[]): void {
            for (var subscription of subscriptions) {
                this.subscriptionStatus[subscription.erpNumber] = true;
                this.$rootScope.$broadcast("productSubStatusUpdated", true);
            }
        }

        protected getSubscriptionsFailed(subscriptions: any): void {

        }
    }

    angular
        .module("insite")
        .controller("CartLinesController", AlaskaCartLinesController);
}