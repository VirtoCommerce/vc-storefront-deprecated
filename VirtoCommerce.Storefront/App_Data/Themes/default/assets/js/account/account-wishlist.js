angular.module('storefront.account')
    .component('vcAccountWishlist', {
        templateUrl: "themes/assets/js/account/account-wishlist.tpl.liquid",
        $routeConfig: [
            { path: '/', name: 'WishList', component: 'vcAccountWishlist', useAsDefault: true }
        ],
        controller: ['wishlistService', '$rootScope', 'cartService', function (wishlistService, $rootScope, cartService) {
            var $ctrl = this;
            $ctrl.listNames = [
                {
                    title: 'Shopping List'
                },
                {
                    title: 'Wish List'
                }
            ];

            $ctrl.getWishlist = function (listName) {
                wishlistService.getWishlist(listName).then(function (response) {
                    var wishlist = response.data;
                    $ctrl.wishlist = wishlist;
                    $ctrl.wishlistIsUpdating = false;
                });
            };

            $ctrl.select = function (selectedTab) {
                $ctrl.cartIsUpdating = true;
                angular.forEach($ctrl.listNames, function (tab) {
                    if (tab.active && tab !== selectedTab) {
                        tab.active = false;
                    }
                });
                selectedTab.active = true;
                $ctrl.selectedTab = selectedTab;
                $ctrl.getWishlist(selectedTab.title);
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

                wishlistService.removeLineItem(lineItem.id, $ctrl.selectedTab.title).then(function (response) {
                    $ctrl.getWishlist($ctrl.selectedTab.title);
                    $rootScope.$broadcast('wishlistItemsChanged');
                }, function (response) {
                    $ctrl.wishlistIsUpdating = false;
                    $ctrl.wishlist.items = initialItems;
                });
            };

            $ctrl.addToCart = function (lineItem) {
                $ctrl.wishlistIsUpdating = true;
                cartService.addLineItem(lineItem.productId, 1).then(function (response) {
                    $ctrl.wishlistIsUpdating = false;
                });
            }
        }]
    });
