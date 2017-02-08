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
        if (uniqueValues.length == 1 && (_.isUndefined(uniqueValues[0]) || _.isNull(uniqueValues[0]))) {
            return false;
        }
        if (onlyDifferences && properties.length > 1 && uniqueValues.length == 1) {
            return false;
        }
        return true;
    }

    function getProductProperties() {
        var properties = _.flatten(_.map($localStorage['productCompareList'], function (product) { return product.properties; }));
        $scope.properties = _.groupBy(properties, 'displayName');
    }
}]);