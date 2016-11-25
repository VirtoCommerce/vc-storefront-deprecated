//Call this to register our module to main application
var moduleName = "storefront.account";

if (storefrontAppDependencies !== undefined) {
    storefrontAppDependencies.push(moduleName);
}
angular.module(moduleName, ['ngResource', 'ngComponentRouter', 'credit-cards'])

.value('$routerRootComponent', 'vcAccountOrders')

.run(['$templateCache', function ($templateCache) {
    // cache application level templates
    $templateCache.put('pagerTemplate.html', '<uib-pagination boundary-links="true" max-size="$ctrl.pageSettings.numPages" items-per-page="$ctrl.pageSettings.itemsPerPageCount" total-items="$ctrl.pageSettings.totalItems" ng-model="$ctrl.pageSettings.currentPage" ng-change="$ctrl.pageSettings.pageChanged()" class="pagination-sm" previous-text="&lsaquo;" next-text="&rsaquo;" first-text="&laquo;" last-text="&raquo;"></uib-pagination>');
}])

.controller('accountController', ['$scope', 'storefront.accountApi', 'confirmService',
    function ($scope, accountApi, confirmService) {
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

        function updateAddresses(data) {
            return wrapLoading(function () {
                return accountApi.updateAddresses(data, $scope.getCustomer).$promise;
            });
        }

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

        // address management
        var addr = $scope.addr = {};
        addr.addNewAddress = function () {
            if (_.last(components).validate()) {
                $scope.customer.addresses.push(addr.newAddress);
                updateAddresses($scope.customer.addresses).then(function () {
                    addr.newAddress = null;
                });
            }
        };

        addr.submit = function ($index, addrCopy) {
            if (components[$index].validate()) {
                angular.copy(addrCopy, $scope.customer.addresses[$index]);
                updateAddresses($scope.customer.addresses);
            }
        };

        addr.cancel = function ($index, addrCopy) {
            angular.copy($scope.customer.addresses[$index], addrCopy);
        };

        $scope.clone = function (x) {
            return angular.copy(x);
        };

        addr.delete = function ($index) {
            confirmService.confirm('Delete this address?').then(function (confirmed) {
                if (confirmed) {
                    $scope.customer.addresses.splice($index, 1);
                    updateAddresses($scope.customer.addresses);
                }
            });
        };

        var components = [];
        addr.addComponent = function (component) {
            components.push(component);
        };
        addr.removeComponent = function (component) {
            components = _.without(components, component);
        };
    }])

.service('confirmService', ['$q', function ($q) {
    this.confirm = function (message) {
        return $q.when(window.confirm(message || 'Is it OK?'));
    };
}]);