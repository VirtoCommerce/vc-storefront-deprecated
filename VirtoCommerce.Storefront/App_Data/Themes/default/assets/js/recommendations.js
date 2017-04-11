var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('recommendationsController', ['$scope', '$timeout', 'recommendationService', function ($scope, $timeout, recommendationService) {
   
    $scope.isBlockVisible = false;
    $scope.productListRecommendationsLoaded = false;
    $scope.productListRecommendations = [];
   
    $scope.getRecommendations = function (evalContext) {
     
        if (_.isString(evalContext.productIds)) {
            if (evalContext.productIds.match(",")) {
                var values = evalContext.productIds.split(',');
                evalContext.productIds = values;
            }
            else {
                evalContext.productIds = [evalContext.productIds];
            }
        }
        recommendationService.getRecommendedProducts(evalContext).then(function (response) {
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