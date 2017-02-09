var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('productListController', ['$scope', '$window', '$localStorage', 'pricingService', 'catalogService', function ($scope, $window, $localStorage, pricingService, catalogService) {
    $scope.productListPricesLoaded = false;
    $scope.productListPrices = [];
    var productCompareList = $localStorage['productCompareList'];
    if (!productCompareList) {
        productCompareList = [];
    }

    pricingService.getActualProductPrices($window.productList).then(function (response) {
        var prices = response.data;
        if (prices.length) {
            for (var i = 0; i < prices.length; i++) {
                $scope.productListPrices[prices[i].productId] = prices[i];
            }
        }
        var productListPricesSize = $scope.getObjectSize($scope.productListPrices);
        $scope.productListPricesLoaded = productListPricesSize > 0;
    });

    $scope.addToProductCompareList = function (productId) {
        if ($window.productCompareListCapacity <= productCompareList.length) {
            alert('Product compare list capacity is ' + $window.productCompareListCapacity);
            return;
        }
        var product = _.find(productCompareList, function (p) { return p.id === productId });
        if (product) {
            productCompareList = _.without(productCompareList, product);
        } else {
            catalogService.getProduct([productId]).then(function (response) {
                if (response.data.length) {
                    var product = response.data[0];
                    _.each(product.properties, function (property) { property.productId = product.id });
                    productCompareList.push(product);
                    alert('Product "' + product.name + '" added to compare list');
                }
            });
        }
    }

    $scope.isInProductCompareList = function (productId) {
        var product = _.find(productCompareList, function (p) { return p.id === productId });
        return product !== undefined && product !== null;
    }
}]);