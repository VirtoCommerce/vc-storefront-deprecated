angular.module('storefront.account')
    .component('vcAccountLists', {
        templateUrl: "themes/assets/js/account/account-lists.tpl.liquid",
        $routeConfig: [
            { path: '/', name: 'WishList', component: 'vcAccountLists', useAsDefault: true }
        ],
        controller: ['listService', '$rootScope', 'cartService', '$translate', 'loadingIndicatorService', '$timeout', function (listService, $rootScope, cartService, $translate, loader, $timeout) {
            var $ctrl = this;
            $ctrl.loader = loader;
            $ctrl.selectedList = {};

            $ctrl.initialize = function (lists) {
                if (lists && lists.length > 0) {
                    $ctrl.lists = lists;
                    $ctrl.selectList(lists[0]);
                    angular.forEach($ctrl.lists, function (list) {
                        var titleKey = 'wishlist.general.' + list.name + '_list_title';
                        var descriptionKey = 'wishlist.general.' + list.name + '_list_description';
                        $translate([titleKey, descriptionKey]).then(function (translations) {
                            list.title = translations[titleKey];
                            list.description = translations[descriptionKey];
                        }, function (translationIds) {
                            list.title = translationIds[titleKey];
                            list.description = translationIds[descriptionKey];
                        });
                    });
                }
            };


            $ctrl.selectList = function (list) {
                $ctrl.selectedList = list;
                loader.wrapLoading(function () {
                    return listService.getWishlist(list.name).then(function (response) {
                        $ctrl.selectedList.items = response.data.items;                     
                    });
                });
            };

            $ctrl.removeLineItem = function (lineItem, list) {  
                loader.wrapLoading(function () {
                    return listService.removeLineItem(lineItem.id, list.name).then(function (response) {
                        $ctrl.selectList(list);
                    });
                });
            };

            $ctrl.addToCart = function (lineItem) {
                loader.wrapLoading(function () {
                    return cartService.addLineItem(lineItem.productId, 1).then(function (response) {
                        $ctrl.productAdded = true;
                        $timeout(function () {
                            $ctrl.productAdded = false;
                        }, 2000);
                    });
                });
            }
        }]
    });
