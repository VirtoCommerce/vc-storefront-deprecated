var storefrontApp = angular.module('storefrontApp');
storefrontApp.controller('orderController', ['$scope', '$window', 'orderService', function ($scope, $window, orderService) {
    getOrder($window.orderNumber);

    function getOrder(orderNumber) {
        orderService.getOrder(orderNumber).then(function (response) {
            if (response && response.data) {
                $scope.order = response.data;
            }
        });
    }
}]);