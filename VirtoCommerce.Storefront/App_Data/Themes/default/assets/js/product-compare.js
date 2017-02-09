var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('productCompareListController', ['$scope', '$localStorage', function ($scope, $localStorage) {
    getProductProperties();

    $scope.showOnlyDifferences = false;

    $scope.clearCompareList = function () {
        $localStorage['productCompareList'] = [];
    }

    $scope.removeProduct = function (product) {
        $localStorage['productCompareList'] = _.without($localStorage['productCompareList'], product);
        getProductProperties();
    }

    $scope.hasValues = function (properties, onlyDifferences) {
        var uniqueValues = _.uniq(_.map(properties, function (p) { return p.value }));
        if (onlyDifferences && properties.length > 1 && uniqueValues.length == 1) {
            return false;
        }
        return true;
    }

    function getProductProperties() {
        var grouped = {};
        var properties = _.flatten(_.map($localStorage['productCompareList'], function (product) { return product.properties; }));
        var propertyDisplayNames = _.uniq(_.map(properties, function (property) { return property.displayName; }));
        _.each(propertyDisplayNames, function (displayName) {
            grouped[displayName] = [];
            var props = _.where(properties, { displayName: displayName });
            _.each($localStorage['productCompareList'], function (product) {
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
}]);