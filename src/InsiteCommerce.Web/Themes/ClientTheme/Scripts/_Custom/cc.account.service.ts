module alaska.account {
    "use strict";


    export class AlaskaAccountService {
        serviceUri = "/api/v1/AlaskaAccount";
        expand = "";

        static $inject = ["$http", "$window", "httpWrapperService"];

        constructor(
            protected $http: ng.IHttpService,
            protected $window: ng.IWindowService,
            protected httpWrapperService: insite.core.HttpWrapperService) {
        }

        validateMilitaryCode(codeString: string): ng.IPromise<string> {
            console.log("codestring", codeString);
            return this.httpWrapperService.executeHttpRequest(
                this,
                this.$http({ url: this.serviceUri + "/ValidateMilitaryMember", method: "GET", params: { code: codeString } }),
                this.validateMilitaryCodeCompleted,
                this.validateMilitaryCodeFailed
            );
        }

        protected validateMilitaryCodeCompleted(response: ng.IHttpPromiseCallbackArg<string>): void {
        }

        protected validateMilitaryCodeFailed(error: ng.IHttpPromiseCallbackArg<any>): void {
        }

        deleteWebAccount(account: AccountModel): ng.IPromise<string> {
            return this.httpWrapperService.executeHttpRequest(
                this,
                this.$http({ url: this.serviceUri + "/RemoveWebAccount", method: "GET", params: { id: account.id } }),
                this.deleteWebAccountCompleted,
                this.deleteWebAccountFailed
            );
        }

        protected deleteWebAccountCompleted(response: ng.IHttpPromiseCallbackArg<string>): void {
        }

        protected deleteWebAccountFailed(error: ng.IHttpPromiseCallbackArg<any>): void {
        }

    }

    angular
        .module("insite")
        .service("alaskaAccountService", AlaskaAccountService);

    
}