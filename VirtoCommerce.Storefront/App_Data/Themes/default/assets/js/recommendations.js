var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('recommendationsController', ['$scope', '$window', 'pricingService', function ($scope, $window, pricingService) {

    $scope.provider = "";
    $scope.type = "";
    $scope.productIds = "";

    initialize();

    function initialize() {
        getRecommendations($scope.type);
    }

    $scope.setType = function (provider, type, productIds) {
        $scope.provider = provider;
        $scope.type = type;
        $scope.productIds = productIds;
    }

    function getRecommendations() {
        //api call
    }
}]);