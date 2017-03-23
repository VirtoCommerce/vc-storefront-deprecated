var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('recommendationsController', ['$scope', '$window', '$http', 'recomendationService', function ($scope, $window, $http, recomendationService) {
    $scope.provider = "";
    $scope.type = "";
    $scope.productIds = "";
    $scope.size = 0;

    $scope.productListRecomendationsLoaded = false;
    $scope.productListRecomendations = [];

    $scope.setType = function (provider, type, productIds, size) {
        $scope.provider = provider;
        $scope.type = type;
        $scope.productIds = productIds;
        $scope.size = size;

        getRecommendations();
    }

    function getRecommendations() {
        var requestData = {
            provider: $scope.provider,
            type : $scope.type,
            skip: 0,
            take: $scope.size,
            productIds: $scope.productIds
        };

        recomendationService.getRecomendedProducts(requestData).then(function (response) {
            var products = response.data;
            if (products.length) {
                for (var i = 0; i < products.length; i++) {
                    $scope.productListRecomendations.push(products[i]);
                }
            }
            $scope.productListRecomendationsLoaded = true;
        });
    }
}]);