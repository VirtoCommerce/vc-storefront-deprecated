angular.module('storefront.account')
.component('vcAccountSubscriptions', {
    templateUrl: "themes/assets/js/account/account-subscriptions.tpl.liquid",
    bindings: {
        customer: '<'
    },
    //$routeConfig: [
    // { path: '/', name: 'SubscriptionList', component: 'vcAccountSubscriptionsList', useAsDefault: true },
    // { path: '/:number', name: 'SubscriptionDetail', component: 'vcAccountSubscriptionDetail' }
    //]
})

.component('vcAccountSubscriptionsList', {
    templateUrl: "account-subscriptions-list.tpl",
    controller: ['storefront.subscriptionApi', 'confirmService', 'loadingIndicatorService', function (subscriptionApi, confirmService, loader) {
        var $ctrl = this;
        $ctrl.loader = loader;
        $ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        $ctrl.pageSettings.pageChanged = function () {
            loader.wrapLoading(function () {
                return subscriptionApi.getSubscriptions({
                    pageNumber: $ctrl.pageSettings.currentPage,
                    pageSize: $ctrl.pageSettings.itemsPerPageCount,
                    sortInfos: $ctrl.sortInfos
                }, function (data) {
                    $ctrl.entries = data.results;
                    $ctrl.pageSettings.totalItems = data.totalCount;
                }).$promise;
            });
        };

        //this.$routerOnActivate = function (next) {
        //    $ctrl.pageSettings.currentPage = next.params.pageNumber || $ctrl.pageSettings.currentPage;
        $ctrl.pageSettings.pageChanged();
        //};

        $ctrl.showDetails = function (entryNumber) {
            $ctrl.entryNumber = entryNumber;

            loader.wrapLoading(function () {
                return subscriptionApi.get({ number: $ctrl.entryNumber }, function (result) {
                    $ctrl.subscription = result;
                }).$promise;
            });
        };

        $ctrl.cancel = function () {
            confirmService.confirm('Cancel this subscription?').then(function (confirmed) {
                if (confirmed) {
                    loader.wrapLoading(function () {
                        return subscriptionApi.cancel({ number: $ctrl.entryNumber }, function (result) {
                            $ctrl.subscription = null;
                        }).$promise;
                    });
                }
            });
        };
    }]
})
;
