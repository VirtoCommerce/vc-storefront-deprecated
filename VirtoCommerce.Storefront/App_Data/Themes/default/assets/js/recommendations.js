var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('recommendationsController', ['$scope', '$timeout', 'recommendationService', function ($scope, $timeout, recommendationService) {
    $scope.provider = "";
    $scope.type = "";
    $scope.productIds = "";
    $scope.size = 0;
    $scope.isBlockVisible = false;

    $scope.productListRecommendationsLoaded = false;
    $scope.productListRecommendations = [];

    $scope.setType = function (provider, type, productIds, size) {
        $scope.provider = provider;
        $scope.type = type;
        $scope.productIds = productIds;
        $scope.size = size;

        if(_.isString(productIds))
        {
            if(productIds.match(","))
            {
                var values = productIds.split(',');
                $scope.productIds = values;
            }
        }
        
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

                $scope.isBlockVisible = products.length > 0;
            }

            $scope.productListRecommendationsLoaded = true;            
        });
    }
    $scope.startRecordInteraction = function () {
        //Necessary condition for ensure what angularjs rendering process finished
        $timeout(function () {
           window.startRecordInteraction();
        });
    }
}]);