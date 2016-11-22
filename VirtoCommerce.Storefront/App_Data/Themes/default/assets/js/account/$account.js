//Call this to register our module to main application
var moduleName = "storefront.account";

if (storefrontAppDependencies !== undefined) {
    storefrontAppDependencies.push(moduleName);
}
angular.module(moduleName, ['ngResource'])

.run(['$templateCache', function ($templateCache) {
    // cache application level templates
    $templateCache.put('pagerTemplate.html', '<uib-pagination boundary-links="true" max-size="$ctrl.pageSettings.numPages" items-per-page="$ctrl.pageSettings.itemsPerPageCount" total-items="$ctrl.pageSettings.totalItems" ng-model="$ctrl.pageSettings.currentPage" ng-change="$ctrl.pageSettings.pageChanged()" class="pagination-sm" previous-text="&lsaquo;" next-text="&rsaquo;" first-text="&laquo;" last-text="&raquo;"></uib-pagination>');
}])

.controller('accountController', ['$scope', '$window', 'storefront.accountApi',
    function ($scope, $window, accountApi) {
        $scope.getOrders = function (pageNumber, pageSize, sortInfos, callback) {
            wrapLoading(function () {
                return accountApi.getOrders({ pageNumber: pageNumber, pageSize: pageSize, sortInfos: sortInfos }, callback).$promise;
            });
        };

        $scope.getQuotes = function (pageNumber, pageSize, sortInfos, callback) {
            wrapLoading(function () {
                return accountApi.getQuotes({ pageNumber: pageNumber, pageSize: pageSize, sortInfos: sortInfos }, callback).$promise;
            });
        };

        $scope.updateProfile = function (updateRequest) {
            wrapLoading(function () {
                return accountApi.updateAccount(updateRequest.changeData, $scope.getCustomer).$promise;
            });
        };

        $scope.updateAddresses = function (data) {
            return wrapLoading(function () {
                return accountApi.updateAddresses(data, $scope.getCustomer).$promise;
            });
        };

        $scope.availCountries = accountApi.getCountries();

        $scope.getCountryRegions = function (country) {
            return accountApi.getCountryRegions(country).$promise;
        };

        $scope.changePassword = function (changePasswordRequest) {
            wrapLoading(function () {
                return accountApi.changePassword(changePasswordRequest.changeData, function (result) {
                    changePasswordRequest.errors = result.errors;
                }).$promise;
            });
        };

        function wrapLoading(func) {
            $scope.isLoading = true;
            return func().then(function (result) {
                $scope.isLoading = false;
                return result;
            },
			function () { $scope.isLoading = false; });
        }

    }]);