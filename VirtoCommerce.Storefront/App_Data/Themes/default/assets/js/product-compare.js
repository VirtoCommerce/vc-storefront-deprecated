var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('productCompareListController', ['$rootScope', '$scope', '$localStorage', '$window', 'catalogService', 'dialogService',
function ($rootScope, $scope, $localStorage, $window, catalogService, dialogService) {
    if (!$localStorage['productCompareList']) {
        $localStorage['productCompareList'] = [];
    }

    $scope.products = $localStorage['productCompareList'];

    $scope.isInProductCompareList = function (productId) {
        return _.some($localStorage['productCompareList'], function (p) { return p.id == productId });
    }

    $scope.addProductToCompareList = function (productId, event) {
        event.preventDefault();
        var existingProduct = _.find($localStorage['productCompareList'], function (p) { return p.id === productId });
        if (existingProduct) {
            dialogService.showDialog(existingProduct, 'productCompareListDialogController', 'storefront.product-compare-list-dialog.tpl');
            return;
        }
        if ($window.productCompareListCapacity <= $localStorage['productCompareList'].length) {
            dialogService.showDialog({ capacityExceeded: true }, 'productCompareListDialogController', 'storefront.product-compare-list-dialog.tpl');
            return;
        }
        catalogService.getProduct([productId]).then(function (response) {
            if (response.data && response.data.length) {
                var product = response.data[0];
                _.each(product.properties, function (property) {
                    property.productId = product.id;
                    if (property.valueType.toLowerCase() === 'number') {
                        property.value = formatNumber(property.value);
                    }
                });
                $localStorage['productCompareList'].push(product);
                dialogService.showDialog(product, 'productCompareListDialogController', 'storefront.product-compare-list-dialog.tpl');
                $rootScope.$broadcast('productCompareListChanged');
            }
        });
    }

    $scope.getProductProperties = function () {
        var grouped = {};
        var properties = _.flatten(_.map($scope.products, function (product) { return product.properties; }));
        var propertyDisplayNames = _.uniq(_.map(properties, function (property) { return property.displayName; }));
        _.each(propertyDisplayNames, function (displayName) {
            grouped[displayName] = [];
            var props = _.where(properties, { displayName: displayName });
            _.each($scope.products, function (product) {
                var productProperty = _.find(props, function (prop) { return prop.productId === product.id });
                if (productProperty) {
                    grouped[displayName].push(productProperty);
                } else {
                    grouped[displayName].push({ valueType: 'ShortText', value: '-' });
                }
            });
        });
        $scope.properties = grouped;
    }

    $scope.hasValues = function (properties, onlyDifferences) {
        var uniqueValues = _.uniq(_.map(properties, function (p) { return p.value }));
        if (onlyDifferences && properties.length > 1 && uniqueValues.length == 1) {
            return false;
        }
        return true;
    }

    $scope.clearCompareList = function () {
        $localStorage['productCompareList'] = [];
        $rootScope.$broadcast('productCompareListChanged');
        $scope.products = $localStorage['productCompareList'];
    }

    $scope.removeProduct = function (product) {
        $localStorage['productCompareList'] = _.without($localStorage['productCompareList'], product);
        $scope.products = $localStorage['productCompareList'];
        $rootScope.$broadcast('productCompareListChanged');
        $scope.getProductProperties();
    }

    function formatNumber(number) {
        var float = parseFloat(number);
        return !isNaN(float) ? float : number;
    }
}]);

storefrontApp.controller('productCompareListDialogController', ['$scope', '$window', 'dialogData', '$uibModalInstance',
function ($scope, $window, dialogData, $uibModalInstance) {
    $scope.dialogData = dialogData;

    $scope.close = function () {
        $uibModalInstance.close();
    }

    $scope.redirect = function (url) {
        $window.location = url;
    }
}]);

storefrontApp.controller('productCompareListBarController', ['$scope', '$localStorage',
function ($scope, $localStorage) {
    $scope.itemsCount = $localStorage['productCompareList'] ? $localStorage['productCompareList'].length : 0;
    $scope.$on('productCompareListChanged', function (event, data) {
        $scope.itemsCount = $localStorage['productCompareList'].length;
    });
}]);