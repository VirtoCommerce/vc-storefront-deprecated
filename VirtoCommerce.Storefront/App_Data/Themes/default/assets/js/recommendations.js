var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('recommendationsController', ['$rootScope', '$scope', '$window', '$http', 'recommendationService', function ($rootScope, $scope, $window, $http, recommendationService) {
    $scope.provider = "";
    $scope.type = "";
    $scope.productIds = "";
    $scope.size = 0;

    $scope.productListRecommendationsLoaded = false;
    $scope.productListRecommendations = [];

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
            type: $scope.type,
            skip: 0,
            take: $scope.size,
            productIds: $scope.productIds
        };

        recommendationService.getRecommendedProducts(requestData).then(function (response) {
            var products = response.data;
            if (products.length) {
                for (var i = 0; i < products.length; i++) {
                    $scope.productListRecommendations.push(products[i]);
                }
            }
            $scope.productListRecommendationsLoaded = true;
        });
    }

    $scope.recommendationClick = function (productId, eventType) {
        $rootScope.$broadcast('storefrontEvents', { productIds: productId, eventType: 'RecommendationClick' });
    }
}]);