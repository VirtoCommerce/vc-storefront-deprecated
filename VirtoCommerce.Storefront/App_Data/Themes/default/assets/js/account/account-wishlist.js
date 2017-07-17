angular.module('storefront.account')
    .component('vcAccountWishlist', {
        templateUrl: "themes/assets/js/account/account-wishlist.tpl.liquid",
        $routeConfig: [
            { path: '/', name: 'WishList', component: 'vcAccountWishlist', useAsDefault: true }
        ],
        controller: ['wishlistService','$rootScope', function (wishlistService, $rootScope) {
            var $ctrl = this;
            $ctrl.getWishlist = function() {

                wishlistService.getWishlist().then(function (response) {
                    var wishlist = response.data;
                    $ctrl.wishlist = wishlist;

                });
            };

            $ctrl.removeLineItem = function (lineItem) {
                var lineItem = _.find($ctrl.wishlist.items, function (i) { return i.id == lineItem.id });
                if (!lineItem || $ctrl.wishlistIsUpdating) {
                    return;
                }
                $ctrl.cartIsUpdating = true;
                var initialItems = angular.copy($ctrl.wishlist.items);
                $ctrl.recentCartItemModalVisible = false;
                $ctrl.wishlist.items = _.without($ctrl.wishlist.items, lineItem);
                $ctrl.wishlistIsUpdating = true;
                wishlistService.removeLineItem(lineItem.id).then(function (response) {
                    $ctrl.getWishlist();
                    $rootScope.$broadcast('wishlistItemsChanged');
                    $ctrl.wishlistIsUpdating = false;
                }, function (response) {
                    $ctrl.wishlistIsUpdating = false;
                    $ctrl.wishlist.items = initialItems;
                });
            }

            $ctrl.getWishlist();
        }]
    })


storefrontApp.controller('recentlyAddedWishlistItemDialogController', ['$scope', '$window', '$uibModalInstance', 'dialogData', function ($scope, $window, $uibModalInstance, dialogData) {
    $scope.$on('wishlistItemsChanged', function (event, data) {
        dialogData.updated = true;
    });

    $scope.dialogData = dialogData;

    $scope.close = function () {
        $uibModalInstance.close();
    }

    $scope.redirect = function (url) {
        $window.location = url;
    }
}]);