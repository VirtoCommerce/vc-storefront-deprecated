/* Modernizr 2.8.2 (Custom Build) | MIT & BSD
 * Build: http://modernizr.com/download/#-fontface-csstransforms-csstransforms3d-touch-cssclasses-teststyles-testprop-testallprops-prefixes-domprefixes-cssclassprefix:supports!
 */
;window.Modernizr=function(a,b,c){function z(a){j.cssText=a}function A(a,b){return z(m.join(a+";")+(b||""))}function B(a,b){return typeof a===b}function C(a,b){return!!~(""+a).indexOf(b)}function D(a,b){for(var d in a){var e=a[d];if(!C(e,"-")&&j[e]!==c)return b=="pfx"?e:!0}return!1}function E(a,b,d){for(var e in a){var f=b[a[e]];if(f!==c)return d===!1?a[e]:B(f,"function")?f.bind(d||b):f}return!1}function F(a,b,c){var d=a.charAt(0).toUpperCase()+a.slice(1),e=(a+" "+o.join(d+" ")+d).split(" ");return B(b,"string")||B(b,"undefined")?D(e,b):(e=(a+" "+p.join(d+" ")+d).split(" "),E(e,b,c))}var d="2.8.2",e={},f=!0,g=b.documentElement,h="modernizr",i=b.createElement(h),j=i.style,k,l={}.toString,m=" -webkit- -moz- -o- -ms- ".split(" "),n="Webkit Moz O ms",o=n.split(" "),p=n.toLowerCase().split(" "),q={},r={},s={},t=[],u=t.slice,v,w=function(a,c,d,e){var f,i,j,k,l=b.createElement("div"),m=b.body,n=m||b.createElement("body");if(parseInt(d,10))while(d--)j=b.createElement("div"),j.id=e?e[d]:h+(d+1),l.appendChild(j);return f=["&#173;",'<style id="s',h,'">',a,"</style>"].join(""),l.id=h,(m?l:n).innerHTML+=f,n.appendChild(l),m||(n.style.background="",n.style.overflow="hidden",k=g.style.overflow,g.style.overflow="hidden",g.appendChild(n)),i=c(l,a),m?l.parentNode.removeChild(l):(n.parentNode.removeChild(n),g.style.overflow=k),!!i},x={}.hasOwnProperty,y;!B(x,"undefined")&&!B(x.call,"undefined")?y=function(a,b){return x.call(a,b)}:y=function(a,b){return b in a&&B(a.constructor.prototype[b],"undefined")},Function.prototype.bind||(Function.prototype.bind=function(b){var c=this;if(typeof c!="function")throw new TypeError;var d=u.call(arguments,1),e=function(){if(this instanceof e){var a=function(){};a.prototype=c.prototype;var f=new a,g=c.apply(f,d.concat(u.call(arguments)));return Object(g)===g?g:f}return c.apply(b,d.concat(u.call(arguments)))};return e}),q.touch=function(){var c;return"ontouchstart"in a||a.DocumentTouch&&b instanceof DocumentTouch?c=!0:w(["@media (",m.join("touch-enabled),("),h,")","{#modernizr{top:9px;position:absolute}}"].join(""),function(a){c=a.offsetTop===9}),c},q.csstransforms=function(){return!!F("transform")},q.csstransforms3d=function(){var a=!!F("perspective");return a&&"webkitPerspective"in g.style&&w("@media (transform-3d),(-webkit-transform-3d){#modernizr{left:9px;position:absolute;height:3px;}}",function(b,c){a=b.offsetLeft===9&&b.offsetHeight===3}),a},q.fontface=function(){var a;return w('@font-face {font-family:"font";src:url("https://")}',function(c,d){var e=b.getElementById("smodernizr"),f=e.sheet||e.styleSheet,g=f?f.cssRules&&f.cssRules[0]?f.cssRules[0].cssText:f.cssText||"":"";a=/src/i.test(g)&&g.indexOf(d.split(" ")[0])===0}),a};for(var G in q)y(q,G)&&(v=G.toLowerCase(),e[v]=q[G](),t.push((e[v]?"":"no-")+v));return e.addTest=function(a,b){if(typeof a=="object")for(var d in a)y(a,d)&&e.addTest(d,a[d]);else{a=a.toLowerCase();if(e[a]!==c)return e;b=typeof b=="function"?b():b,typeof f!="undefined"&&f&&(g.className+=" supports-"+(b?"":"no-")+a),e[a]=b}return e},z(""),i=k=null,e._version=d,e._prefixes=m,e._domPrefixes=p,e._cssomPrefixes=o,e.testProp=function(a){return D([a])},e.testAllProps=F,e.testStyles=w,g.className=g.className.replace(/(^|\s)no-js(\s|$)/,"$1$2")+(f?" supports-js supports-"+t.join(" supports-"):""),e}(this,this.document);
var storefrontAppDependencies = [
    'ui.bootstrap',
    'ngStorage',
    'pascalprecht.translate',
    'ngSanitize',
]
var storefrontApp = angular.module('storefrontApp', storefrontAppDependencies);

storefrontApp.factory('httpErrorInterceptor', ['$q', '$rootScope', function ($q, $rootScope) {
    var httpErrorInterceptor = {};

    httpErrorInterceptor.responseError = function (rejection) {
        if (rejection.data && rejection.data.message) {
            $rootScope.$broadcast('storefrontError', {
                type: 'error',
                title: [rejection.config.method, rejection.config.url, rejection.status, rejection.statusText, rejection.data.message].join(' '),
                message: rejection.data.stackTrace,
            });
        }
        return $q.reject(rejection);
    };
    httpErrorInterceptor.requestError = function (rejection) {
        if (rejection.data && rejection.data.message) {
            $rootScope.$broadcast('storefrontError', {
                type: 'error',
                title: [rejection.config.method, rejection.config.url, rejection.status, rejection.statusText, rejection.data.message].join(' '),
                message: rejection.data.stackTrace,
            });
        }
        return $q.reject(rejection);
    };

    return httpErrorInterceptor;
}])

storefrontApp.config(['$httpProvider', '$translateProvider', function ($httpProvider, $translateProvider) {
    $httpProvider.interceptors.push('httpErrorInterceptor');

    $translateProvider.useSanitizeValueStrategy('sanitizeParameters');
    $translateProvider.useUrlLoader(BASE_URL + 'themes/localization.json');
    $translateProvider.preferredLanguage('en');

}]);
var storefrontApp = angular.module('storefrontApp');

storefrontApp.service('dialogService', ['$uibModal', function ($uibModal) {
    return {
        showDialog: function (dialogData, controller, templateUrl) {
            var modalInstance = $uibModal.open({
                controller: controller,
                templateUrl: templateUrl,
                resolve: {
                    dialogData: function () {
                        return dialogData;
                    }
                }
            });
        }
    }
}]);

storefrontApp.service('feedbackService', ['$http', function ($http) {
    return {
        postFeedback: function (data) {
            return $http.post('storefrontapi/feedback', { model: data });
        }
    }
}]);

storefrontApp.service('customerService', ['$http', function ($http) {
    return {
        getCurrentCustomer: function () {
            return $http.get('storefrontapi/account?t=' + new Date().getTime());
        }
    }
}]);

storefrontApp.service('marketingService', ['$http', function ($http) {
    return {
        getDynamicContent: function (placeName) {
            return $http.get('storefrontapi/marketing/dynamiccontent/' + placeName + '?t=' + new Date().getTime());
        },
    }
}]);

storefrontApp.service('pricingService', ['$http', function ($http) {
	return {
		getActualProductPrices: function (products) {
		    return $http.post('storefrontapi/pricing/actualprices', { products: products });
		}
	}
}]);

storefrontApp.service('catalogService', ['$http', function ($http) {
    return {
        getProduct: function (productIds) {
            return $http.get('storefrontapi/products?productIds=' + productIds + '&t=' + new Date().getTime());
        },
        search: function (criteria) {
            return $http.post('storefrontapi/catalog/search', { searchCriteria: criteria });
        },
        searchCategories: function (criteria) {
            return $http.post('storefrontapi/categories/search', { searchCriteria: criteria });
        }
    }
}]);

storefrontApp.service('cartService', ['$http', function ($http) {
    return {
        getCart: function () {
            return $http.get('storefrontapi/cart?t=' + new Date().getTime());
        },
        getCartItemsCount: function () {
            return $http.get('storefrontapi/cart/itemscount?t=' + new Date().getTime());
        },
        addLineItem: function (productId, quantity) {
            return $http.post('storefrontapi/cart/items', { id: productId, quantity: quantity });
        },
        changeLineItemQuantity: function (lineItemId, quantity) {
            return $http.put('storefrontapi/cart/items', { lineItemId: lineItemId, quantity: quantity });
        },
        removeLineItem: function (lineItemId) {
            return $http.delete('storefrontapi/cart/items?lineItemId=' + lineItemId);
        },
        changeLineItemPrice: function (lineItemId, newPrice) {
        	return $http.put('storefrontapi/cart/items/price', { lineItemId: lineItemId, newPrice: newPrice});
        },
        clearCart: function () {
            return $http.post('storefrontapi/cart/clear');
        },
        getCountries: function () {
            return $http.get('storefrontapi/countries?t=' + new Date().getTime());
        },
        getCountryRegions: function (countryCode) {
        	return $http.get('storefrontapi/countries/' + countryCode + '/regions?t=' + new Date().getTime());
        },
        addCoupon: function (couponCode) {
            return $http.post('storefrontapi/cart/coupons/' + couponCode);
        },
        removeCoupon: function () {
            return $http.delete('storefrontapi/cart/coupons');
        },
        addOrUpdateShipment: function (shipment) {
            return $http.post('storefrontapi/cart/shipments', shipment);
        },
        addOrUpdatePayment: function (payment) {
            return $http.post('storefrontapi/cart/payments', payment );
        },
        getAvailableShippingMethods: function (shipmentId) {
            return $http.get('storefrontapi/cart/shipments/' + shipmentId + '/shippingmethods?t=' + new Date().getTime());
        },
        getAvailablePaymentMethods: function () {
            return $http.get('storefrontapi/cart/paymentmethods?t=' + new Date().getTime());
        },
        addOrUpdatePaymentPlan: function (plan) {
            return $http.post('storefrontapi/cart/paymentPlan', plan);
        },
        removePaymentPlan: function () {
            return $http.delete('storefrontapi/cart/paymentPlan');
        },
        createOrder: function (bankCardInfo) {
            return $http.post('storefrontapi/cart/createorder', { bankCardInfo: bankCardInfo });
        }
    }
}]);

storefrontApp.service('listService', ['$http', function ($http) {
    return {
        getWishlist: function (listName) {
            return $http.get('storefrontapi/lists/' + listName + '?t=' + new Date().getTime());
        },
        contains: function (productId, listName) {
            return $http.get('storefrontapi/lists/' + listName +'/items/'+ productId + '/contains?t=' + new Date().getTime());
        },
        addLineItem: function (productId, listName) {
            return $http.post('storefrontapi/lists/' + listName + '/items', { productId: productId });
        },
        removeLineItem: function (lineItemId, listName) {
            return $http.delete('storefrontapi/lists/' + listName + '/items/' + lineItemId);
        }
    }
}]);

storefrontApp.service('quoteRequestService', ['$http', function ($http) {
    return {
        getCurrentQuoteRequest: function () {
            return $http.get('storefrontapi/quoterequest/current?t=' + new Date().getTime());
        },
        getQuoteRequest: function (number) {
            return $http.get('storefrontapi/quoterequests/' + number + '?t=' + new Date().getTime());
        },
        getQuoteRequestItemsCount: function (number) {
            return $http.get('storefrontapi/quoterequests/' + number + '/itemscount?t=' + new Date().getTime());
        },
        addProductToQuoteRequest: function (productId, quantity) {
            return $http.post('storefrontapi/quoterequests/current/items', { productId: productId, quantity: quantity });
        },
        removeProductFromQuoteRequest: function (quoteRequestNumber, quoteItemId) {
            return $http.delete('storefrontapi/quoterequests/' + quoteRequestNumber + '/items/' + quoteItemId);
        },
        submitQuoteRequest: function (quoteRequestNumber, quoteRequest) {
            return $http.post('storefrontapi/quoterequests/' + quoteRequestNumber + '/submit', { quoteForm: quoteRequest });
        },
        rejectQuoteRequest: function (quoteRequestNumber) {
            return $http.post('storefrontapi/quoterequests/' + quoteRequestNumber + '/reject');
        },
        updateQuoteRequest: function (quoteRequestNumber, quoteRequest) {
            return $http.put('storefrontapi/quoterequests/' + quoteRequestNumber + '/update', { quoteRequest: quoteRequest });
        },
        getTotals: function (quoteRequestNumber, quoteRequest) {
            return $http.post('storefrontapi/quoterequests/' + quoteRequestNumber + '/totals', { quoteRequest: quoteRequest });
        },
        confirmQuoteRequest: function (quoteRequestNumber, quoteRequest) {
            return $http.post('storefrontapi/quoterequests/' + quoteRequestNumber + '/confirm', { quoteRequest: quoteRequest });
        }
    }
}]);

storefrontApp.service('recommendationService', ['$http', function ($http) {
    return {
        getRecommendedProducts: function (requestData) {
            return $http.post('storefrontapi/recommendations', requestData );
        }
    }
}]);

storefrontApp.service('orderService', ['$http', function ($http) {
    return {
        getOrder: function (orderNumber) {
            return $http.get('storefrontapi/orders/' + orderNumber + '?t=' + new Date().getTime());
        }
    }
}]);
var storefrontApp = angular.module('storefrontApp');


storefrontApp.controller('mainController', ['$scope', '$location', '$window', 'customerService', 'storefrontApp.mainContext',
    function ($scope, $location, $window, customerService, mainContext) {

        //Base store url populated in layout and can be used for construction url inside controller
        $scope.baseUrl = {};

        $scope.$watch(function () {
            $scope.currentPath = $location.$$path.replace('/', '');
        });

        $scope.$on('storefrontError', function (event, data) {
            $scope.storefrontNotification = data;
            $scope.storefrontNotification.detailsVisible = false;
        });

        $scope.toggleNotificationDetails = function () {
            $scope.storefrontNotification.detailsVisible = !$scope.storefrontNotification.detailsVisible;
        }

        $scope.closeNotification = function () {
            $scope.storefrontNotification = null;
        }

        //For outside app redirect (To reload the page after changing the URL, use the lower-level API)
        $scope.outerRedirect = function (absUrl) {
            $window.location.href = absUrl;
        };

        //change in the current URL or change the current URL in the browser (for app route)
        $scope.innerRedirect = function (path) {
            $location.path(path);
            $scope.currentPath = $location.$$path.replace('/', '');
        };

        $scope.stringifyAddress = function (address) {
            var stringifiedAddress = address.firstName + ' ' + address.lastName + ', ';
            stringifiedAddress += address.organization ? address.organization + ', ' : '';
            stringifiedAddress += address.countryName + ', ';
            stringifiedAddress += address.regionName ? address.regionName + ', ' : '';
            stringifiedAddress += address.city + ' ';
            stringifiedAddress += address.line1 + ', ';
            stringifiedAddress += address.line2 ? address.line2 : '';
            stringifiedAddress += address.postalCode;
            return stringifiedAddress;
        }

        $scope.getObjectSize = function (obj) {
            var size = 0, key;
            for (key in obj) {
                if (obj.hasOwnProperty(key)) {
                    size++;
                }
            }
            return size;
        }

        mainContext.getCustomer = $scope.getCustomer = function () {
            customerService.getCurrentCustomer().then(function (response) {
                var addressId = 1;
                _.each(response.data.addresses, function (address) {
                    address.id = addressId;
                    addressId++;
                });
                response.data.isContact = response.data.memberType === 'Contact';
                mainContext.customer = $scope.customer = response.data;
            });
        };

        $scope.getCustomer();
    }])

.factory('storefrontApp.mainContext', function () {
    return {};
});
var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('cartController', ['$rootScope', '$scope', '$timeout', 'cartService', 'catalogService', function ($rootScope, $scope, $timeout, cartService, catalogService) {
    var timer;

    initialize();

    $scope.setCartForm = function (form) {
        $scope.formCart = form;
    }

    $scope.changeLineItemQuantity = function (lineItemId, quantity) {
        var lineItem = _.find($scope.cart.items, function (i) { return i.id == lineItemId });
        if (!lineItem || quantity < 1 || $scope.cartIsUpdating || $scope.formCart.$invalid) {
            return;
        }
        var initialQuantity = lineItem.quantity;
        lineItem.quantity = quantity;
        $timeout.cancel(timer);
        timer = $timeout(function () {
            $scope.cartIsUpdating = true;
            cartService.changeLineItemQuantity(lineItemId, quantity).then(function (response) {
                getCart();
                $rootScope.$broadcast('cartItemsChanged');
            }, function (response) {
                lineItem.quantity = initialQuantity;
                $scope.cartIsUpdating = false;
            });
        }, 300);
    }

    $scope.changeLineItemPrice = function (lineItemId, newPrice) {
    	var lineItem = _.find($scope.cart.items, function (i) { return i.id == lineItemId });
    	if (!lineItem || $scope.cartIsUpdating) {
    		return;
    	}
    	$scope.cartIsUpdating = true;
    	cartService.changeLineItemPrice(lineItemId, newPrice).then(function (response) {
    		getCart();
    		$rootScope.$broadcast('cartItemsChanged');
    	}, function (response) {
    		$scope.cart.items = initialItems;
    		$scope.cartIsUpdating = false;
    	});
    };
    $scope.removeLineItem = function (lineItemId) {
        var lineItem = _.find($scope.cart.items, function (i) { return i.id == lineItemId });
        if (!lineItem || $scope.cartIsUpdating) {
            return;
        }
        $scope.cartIsUpdating = true;
        var initialItems = angular.copy($scope.cart.items);
        $scope.recentCartItemModalVisible = false;
        $scope.cart.items = _.without($scope.cart.items, lineItem);
        cartService.removeLineItem(lineItemId).then(function (response) {
            getCart();
            $rootScope.$broadcast('cartItemsChanged');
        }, function (response) {
            $scope.cart.items = initialItems;
            $scope.cartIsUpdating = false;
        });
    }   

    $scope.submitCart = function () {
        $scope.formCart.$setSubmitted();
        if ($scope.formCart.$invalid) {
            return;
        }
        if ($scope.cart.hasPhysicalProducts) {
            $scope.outerRedirect($scope.baseUrl + 'cart/checkout');
        } else {
            $scope.outerRedirect($scope.baseUrl + 'cart/checkout');
        }
    }

    $scope.searchProduct = function () {
        $scope.productSearchResult = null;
        if ($scope.productSkuOrName) {
            $timeout.cancel(timer);
            timer = $timeout(function () {
                $scope.productSearchProcessing = true;
                var criteria = {
                    keyword: $scope.productSkuOrName,
                    start: 0,
                    pageSize: 5
                }
                catalogService.search(criteria).then(function (response) {
                    $scope.productSearchProcessing = false;
                    $scope.productSearchResult = response.data.products;
                }, function (response) {
                    $scope.productSearchProcessing = false;
                });
            }, 300);
        }
    }

    $scope.selectSearchedProduct = function (product) {
        $scope.productSearchResult = null;
        $scope.selectedSearchedProduct = product;
        $scope.productSkuOrName = product.name;
    }

    $scope.addProductToCart = function (product, quantity) {
        $scope.cartIsUpdating = true;
        cartService.addLineItem(product.id, quantity).then(function (response) {
            getCart();
            $scope.productSkuOrName = null;
            $scope.selectedSearchedProduct = null;
            $rootScope.$broadcast('cartItemsChanged');
        });
    }

    function initialize() {
        getCart();
    }

    function getCart() {
        $scope.cartIsUpdating = true;
        cartService.getCart().then(function (response) {
            var cart = response.data;
            cart.hasValidationErrors = _.some(cart.validationErrors) || _.some(cart.items, function (item) { return _.some(item.validationErrors) });
            $scope.cart = cart;
            $scope.cartIsUpdating = false;
        }, function (response) {
            $scope.cartIsUpdating = false;
        });
    }
}]);

storefrontApp.controller('cartBarController', ['$scope', 'cartService', function ($scope, cartService) {
    getCartItemsCount();

    $scope.$on('cartItemsChanged', function (event, data) {
        getCartItemsCount();
    });

    function getCartItemsCount() {
        cartService.getCartItemsCount().then(function (response) {
            $scope.cartItemsCount = response.data;
        });
    }
}]);

storefrontApp.controller('recentlyAddedCartItemDialogController', ['$scope', '$window', '$uibModalInstance', 'dialogData', function ($scope, $window, $uibModalInstance, dialogData) {
    $scope.$on('cartItemsChanged', function (event, data) {
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

var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('quoteRequestController', ['$rootScope', '$scope', '$window', '$location', 'quoteRequestService', 'cartService',
    function ($rootScope, $scope, $window, $location, quoteRequestService, cartService) {
    initialize();

    $scope.setQuoteRequestForm = function (form) {
        $scope.formQuoteRequest = form;
    }

    $scope.displayForStatuses = function (statuses) {
        return _.contains(statuses, $scope.quoteRequest.status);
    }

    $scope.addTierPrice = function (quoteItem) {
        quoteItem.proposalPrices.push({
            id: quoteItem.proposalPrices.length + 1,
            price: quoteItem.salePrice,
            quantity: 1
        });
    }

    $scope.changeTierPriceQuantity = function (tierPrice, quantity) {
        if (quantity < 1 || quantity.isNaN) {
            return;
        }
        tierPrice.quantity = quantity;
    }

    $scope.removeTierPrice = function (quoteItem, tierPrice) {
        quoteItem.proposalPrices = _.without(quoteItem.proposalPrices, tierPrice);
    }

    $scope.removeProductFromQuoteRequest = function (quoteItem) {
        var initialQuoteItems = angular.copy($scope.quoteRequest.items);
        $scope.quoteRequest.items = _.without($scope.quoteRequest.items, quoteItem);
        quoteRequestService.removeProductFromQuoteRequest($scope.quoteRequest.id, quoteItem.id).then(function (response) {
            getQuoteRequest($scope.quoteRequest.id);
            $rootScope.$broadcast('actualQuoteRequestItemsChanged');
        }, function (response) {
            $scope.quoteRequest.items = initialQuoteItems;
        });
    }

    $scope.setCountry = function (addressType, countryName) {
        var country = _.find($scope.countries, function (c) { return c.name == countryName });
        if (!country) {
            return;
        }
        if (addressType == 'Billing') {
            $scope.billingCountry = country;
            $scope.billingCountryRegions = [];
            $scope.quoteRequest.billingAddress.countryCode = country.code3 || country.code2;
            $scope.quoteRequest.billingAddress.regionId = null;
            $scope.quoteRequest.billingAddress.regionName = null;
        }
        if (addressType == 'Shipping') {
            $scope.shippingCountry = country;
            $scope.shippingCountryRegions = [];
            $scope.quoteRequest.shippingAddress.countryCode = country.code3 || country.code2;
            $scope.quoteRequest.shippingAddress.regionId = null;
            $scope.quoteRequest.shippingAddress.regionName = null;
        }
        if (country.code3) {
            getCountryRegions(addressType, country.code3);
        }
    }

    $scope.setCountryRegion = function (addressType) {
        if (addressType == 'Billing') {
            var countryRegion = _.find($scope.billingCountryRegions, function (r) { return r.name == $scope.quoteRequest.billingAddress.regionName });
            if (!countryRegion) {
                return;
            }
            $scope.quoteRequest.billingAddress.regionId = countryRegion.code;
        }
        if (addressType == 'Shipping') {
            var countryRegion = _.find($scope.shippingCountryRegions, function (r) { return r.name == $scope.quoteRequest.shippingAddress.regionName });
            if (!countryRegion) {
                return;
            }
            $scope.quoteRequest.shippingAddress.regionId = countryRegion.code;
        }
    }

    $scope.selectCustomerAddress = function (addressType) {
        if (addressType === 'Billing') {
            var billingAddress = _.find($scope.customer.addresses, function (a) { return a.id === $scope.quoteRequest.billingAddress.id });
            if (billingAddress) {
                billingAddress.type = 'Billing';
                if (billingAddress.countryCode) {
                    getCountryRegions('Billing', billingAddress.countryCode);
                }
                $scope.quoteRequest.billingAddress = angular.copy(billingAddress);
            }
        }
        if (addressType === 'Shipping') {
            var shippingAddress = _.find($scope.customer.addresses, function (a) { return a.id === $scope.quoteRequest.shippingAddress.id });
            if (shippingAddress) {
                shippingAddress.type = 'Shipping';
                if (shippingAddress.countryCode) {
                    getCountryRegions('Shipping', shippingAddress.countryCode);
                }
                $scope.quoteRequest.shippingAddress = angular.copy(shippingAddress);
            }
        }
    }

    $scope.stringifyAddress = function (address) {
        if (!address) {
            return;
        }
        var stringifiedAddress = address.firstName + ' ' + address.lastName + ', ';
        stringifiedAddress += address.organization ? address.organization + ', ' : '';
        stringifiedAddress += address.countryName + ', ';
        stringifiedAddress += address.regionName ? address.regionName + ', ' : '';
        stringifiedAddress += address.city + ' ';
        stringifiedAddress += address.line1 + ', ';
        stringifiedAddress += address.line2 ? address.line2 : '';
        stringifiedAddress += address.postalCode;
        return stringifiedAddress;
    }

    $scope.submitQuoteRequest = function () {
        $scope.formQuoteRequest.$setSubmitted();
        if ($scope.formQuoteRequest.$invalid) {
            return;
        }
        $scope.quoteRequest.billingAddress.email = $scope.quoteRequest.email;
        if ($scope.quoteRequest.shippingAddress) {
            $scope.quoteRequest.shippingAddress.email = $scope.quoteRequest.email;
        }
        quoteRequestService.submitQuoteRequest($scope.quoteRequest.id, toFormModel($scope.quoteRequest)).then(function (response) {
            if ($scope.customer.isRegisteredUser) {
                $scope.outerRedirect($scope.baseUrl + 'account/quoterequests');
            } else {
                $scope.outerRedirect($scope.baseUrl + 'account/login');
            }
        });
    }

    $scope.rejectQuoteRequest = function () {
        quoteRequestService.rejectQuoteRequest($scope.quoteRequest.id).then(function (response) {
            quoteRequestService.getQuoteRequest($scope.quoteRequest.id).then(function (response) {
                $scope.quoteRequest = response.data;
            });
        });
    }

    $scope.selectTierPrice = function () {
        quoteRequestService.getTotals($scope.quoteRequest.id, toFormModel($scope.quoteRequest)).then(function (response) {
            $scope.quoteRequest.totals = response.data;
        });
    }

    $scope.confirmQuoteRequest = function () {
        quoteRequestService.confirmQuoteRequest($scope.quoteRequest.id, toFormModel($scope.quoteRequest)).then(function (response) {
            $scope.outerRedirect($scope.baseUrl + 'cart/checkout/#/shipping-address');
        });
    }

    $scope.setRequestShippingQuote = function () {
        if (!$scope.quoteRequest.requestShippingQuote) {
            $scope.quoteRequest.shippingAddress = null;
        }
    }

    $scope.setShippingAddressEqualsBilling = function () {
        if ($scope.quoteRequest.shippingAddressEqualsBilling) {
            $scope.quoteRequest.shippingAddress = angular.copy($scope.quoteRequest.billingAddress);
            $scope.quoteRequest.shippingAddress.type = 'Shipping';
            if ($scope.quoteRequest.shippingAddress.countryCode) {
                $scope.shippingCountry = $scope.billingCountry;
                getCountryRegions('Shipping', $scope.quoteRequest.shippingAddress.countryCode);
            }
        }
    }

    $scope.tierPricesUnique = function (quoteItem) {
        var quantities = _.map(quoteItem.proposalPrices, function (p) { return p.quantity });
        return _.uniq(quantities).length == quoteItem.proposalPrices.length;
    }

    function initialize() {
        var quoteRequestNumber = $location.url().replace('/', '') || $window.currentQuoteRequestNumber;
        $scope.billingCountry = null;
        $scope.shippingCountry = null;
        getCountries();
        if (quoteRequestNumber) {
            getQuoteRequest(quoteRequestNumber);
        } else {
            $scope.quoteRequest = { itemsCount: 0 };
        }
    }

    function getQuoteRequest(number) {
        quoteRequestService.getQuoteRequest(number).then(function (response) {
            var quoteRequest = response.data;
            if (!quoteRequest.billingAddress) {
                if ($scope.customer.addresses.length) {
                    quoteRequest.billingAddress = angular.copy($scope.customer.addresses[0]);
                    quoteRequest.billingAddress.type = 'Billing';
                    if (quoteRequest.billingAddress.countryCode) {
                        getCountryRegions('Billing', quoteRequest.billingAddress.countryCode);
                    }
                } else {
                    quoteRequest.billingAddress = {
                        firstName: $scope.customer.firstName,
                        lastName: $scope.customer.lastName
                    };
                }
            }
            _.each(quoteRequest.items, function (quoteItem) {
                var i = 1;
                _.each(quoteItem.proposalPrices, function (tierPrice) {
                    tierPrice.id = i;
                    if (quoteItem.selectedTierPrice.quantity == tierPrice.quantity) {
                        quoteItem.selectedTierPrice = tierPrice;
                    }
                    i++;
                });
            });
            quoteRequest.requestShippingQuote = true;
            $scope.quoteRequest = quoteRequest;
        });
    }

    function getCountries() {
        cartService.getCountries().then(function (response) {
            $scope.countries = response.data;
        });
    }

    function getCountryRegions(addressType, countryCode) {
        cartService.getCountryRegions(countryCode).then(function (response) {
            var countryRegions = response.data;
            if (addressType == 'Billing') {
                $scope.billingCountryRegions = countryRegions || [];
            }
            if (addressType == 'Shipping') {
                $scope.shippingCountryRegions = countryRegions || [];
            }
        });
    }

    function toFormModel(quoteRequest) {
        var quoteRequestFormModel = {
            id: quoteRequest.id,
            tag: quoteRequest.tag,
            status: quoteRequest.status,
            comment: quoteRequest.comment,
            billingAddress: quoteRequest.billingAddress,
            shippingAddress: quoteRequest.shippingAddress,
            items: []
        };
        _.each(quoteRequest.items, function (quoteItem) {
            var quoteItemFormModel = {
                id: quoteItem.id,
                comment: quoteItem.comment,
                selectedTierPrice: {
                    price: quoteItem.selectedTierPrice.price.amount,
                    quantity: quoteItem.selectedTierPrice.quantity
                },
                proposalPrices: []
            };
            _.each(quoteItem.proposalPrices, function (tierPrice) {
                quoteItemFormModel.proposalPrices.push({
                    price: tierPrice.price.amount,
                    quantity: tierPrice.quantity
                });
            });
            quoteRequestFormModel.items.push(quoteItemFormModel);
        });

        return quoteRequestFormModel;
    }
}]);

storefrontApp.controller('actualQuoteRequestBarController', ['$scope', 'quoteRequestService', function ($scope, quoteRequestService) {
    getCurrentQuoteRequest();

    $scope.$on('actualQuoteRequestItemsChanged', function (event, data) {
        getCurrentQuoteRequest();
    });

    function getCurrentQuoteRequest() {
        quoteRequestService.getCurrentQuoteRequest().then(function (response) {
            $scope.actualQuoteRequest = response.data;
        });
    }
}]);

storefrontApp.controller('recentlyAddedActualQuoteRequestItemDialogController', ['$scope', '$window', '$uibModalInstance', 'dialogData',
    function ($scope, $window, $uibModalInstance, dialogData) {

    $scope.$on('actualQuoteRequestItemsChanged', function (event, data) {
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
var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('productCompareListController', ['$rootScope', '$scope', '$localStorage', '$window', 'catalogService', 'dialogService',
function ($rootScope, $scope, $localStorage, $window, catalogService, dialogService) {
    if (!$localStorage['productCompareList']) {
        $localStorage['productCompareList'] = [];
    }

    $scope.products = $localStorage['productCompareList'];

    $scope.isInProductCompareList = function (productId) {
        return _.some($localStorage['productCompareList'], function (p) { return p.id == productId });
    }

    $scope.addProductToCompareList = function (productId, event) {
        event.preventDefault();
        var existingProduct = _.find($localStorage['productCompareList'], function (p) { return p.id === productId });
        if (existingProduct) {
            dialogService.showDialog(existingProduct, 'productCompareListDialogController', 'storefront.product-compare-list-dialog.tpl');
            return;
        }
        if ($window.productCompareListCapacity <= $localStorage['productCompareList'].length) {
            dialogService.showDialog({ capacityExceeded: true }, 'productCompareListDialogController', 'storefront.product-compare-list-dialog.tpl');
            return;
        }
        catalogService.getProduct([productId]).then(function (response) {
            if (response.data && response.data.length) {
                var product = response.data[0];
                _.each(product.properties, function (property) {
                    property.productId = product.id;
                    if (property.valueType.toLowerCase() === 'number') {
                        property.value = formatNumber(property.value);
                    }
                });
                $localStorage['productCompareList'].push(product);
                dialogService.showDialog(product, 'productCompareListDialogController', 'storefront.product-compare-list-dialog.tpl');
                $rootScope.$broadcast('productCompareListChanged');
            }
        });
    }

    $scope.getProductProperties = function () {
        var grouped = {};
        var properties = _.flatten(_.map($scope.products, function (product) { return product.properties; }));
        var propertyDisplayNames = _.uniq(_.map(properties, function (property) { return property.displayName; }));
        _.each(propertyDisplayNames, function (displayName) {
            grouped[displayName] = [];
            var props = _.where(properties, { displayName: displayName });
            _.each($scope.products, function (product) {
                var productProperty = _.find(props, function (prop) { return prop.productId === product.id });
                if (productProperty) {
                    grouped[displayName].push(productProperty);
                } else {
                    grouped[displayName].push({ valueType: 'ShortText', value: '-' });
                }
            });
        });
        $scope.properties = grouped;
    }

    $scope.hasValues = function (properties, onlyDifferences) {
        var uniqueValues = _.uniq(_.map(properties, function (p) { return p.value }));
        if (onlyDifferences && properties.length > 1 && uniqueValues.length == 1) {
            return false;
        }
        return true;
    }

    $scope.clearCompareList = function () {
        $localStorage['productCompareList'] = [];
        $rootScope.$broadcast('productCompareListChanged');
        $scope.products = $localStorage['productCompareList'];
    }

    $scope.removeProduct = function (product) {
        $localStorage['productCompareList'] = _.without($localStorage['productCompareList'], product);
        $scope.products = $localStorage['productCompareList'];
        $rootScope.$broadcast('productCompareListChanged');
        $scope.getProductProperties();
    }

    function formatNumber(number) {
        var float = parseFloat(number);
        return !isNaN(float) ? float : number;
    }
}]);

storefrontApp.controller('productCompareListDialogController', ['$scope', '$window', 'dialogData', '$uibModalInstance',
function ($scope, $window, dialogData, $uibModalInstance) {
    $scope.dialogData = dialogData;

    $scope.close = function () {
        $uibModalInstance.close();
    }

    $scope.redirect = function (url) {
        $window.location = url;
    }
}]);

storefrontApp.controller('productCompareListBarController', ['$scope', '$localStorage',
function ($scope, $localStorage) {
    $scope.itemsCount = $localStorage['productCompareList'] ? $localStorage['productCompareList'].length : 0;
    $scope.$on('productCompareListChanged', function (event, data) {
        $scope.itemsCount = $localStorage['productCompareList'].length;
    });
}]);
var storefrontApp = angular.module('storefrontApp');
storefrontApp.controller('searchBarController', ['$scope', '$timeout', '$window', 'catalogService', function ($scope, $timeout, $window, catalogService) {
    var timer;

    $scope.query = $window.searchQuery;

    $scope.getSuggestions = function () {
        if (!$scope.query) {
            return;
        }
        $timeout.cancel(timer);
        timer = $timeout(function () {
            $scope.searching = true;
            $scope.categorySuggestions = [];
            $scope.productSuggestions = [];
            var searchCriteria = {
                keyword: $scope.query,
                skip: 0,
                take: $window.suggestionsLimit
            }
            catalogService.searchCategories(searchCriteria).then(function (response) {
                var categories = response.data.categories;
                if (categories.length > 5) {
                    searchCriteria.take = $window.suggestionsLimit - 5;
                    $scope.categorySuggestions = _.first(categories, 5);
                } else {
                    searchCriteria.take = $window.suggestionsLimit - categories.length;
                    $scope.categorySuggestions = categories;
                }
                catalogService.search(searchCriteria).then(function (response) {
                    var products = response.data.products;
                    $scope.productSuggestions = products;
                    $scope.searching = false;
                });
            });
        }, 300);
    }
}]);
var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcAddress', {
    templateUrl: "themes/assets/js/common-components/address.tpl.html",
    bindings: {
        address: '=',
        addresses: '<',
        countries: '=',
        validationContainer: '=',
        getCountryRegions: '&',
        editMode: '<',
        onUpdate: '&'
    },
    require: {
        checkoutStep: '?^vcCheckoutWizardStep'
    },
    controller: ['$scope', function ($scope) {
        var ctrl = this;
        this.$onInit = function () {
            if (ctrl.validationContainer)
                ctrl.validationContainer.addComponent(this);
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.addComponent(this);
        };

        this.$onDestroy = function () {
            if (ctrl.validationContainer)
                ctrl.validationContainer.removeComponent(this);
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.removeComponent(this);
        };

        function populateRegionalDataForAddress(address) {
            if (address) {
                //Set country object for address
                address.country = _.findWhere(ctrl.countries, { code3: address.countryCode });
                if (address.country != null) {
                    ctrl.address.countryName = ctrl.address.country.name;
                    ctrl.address.countryCode = ctrl.address.country.code3;
                }

                if (address.country) {
                    if (address.country.regions) {
                        setAddressRegion(address, address.country.regions);
                    }
                    else {
                        ctrl.getCountryRegions({ country: address.country }).then(function (regions) {
                            address.country.regions = regions;
                            setAddressRegion(address, regions);
                        });
                    }
                }
            }
        }

        function setAddressRegion(address, regions) {
            address.region = _.findWhere(regions, { code: address.regionId });
            if (address.region) {
                ctrl.address.regionId = ctrl.address.region.code;
                ctrl.address.regionName = ctrl.address.region.name;
            }
            else {
                ctrl.address.regionId = undefined;
                ctrl.address.regionName = undefined;
            }
        }

        ctrl.setForm = function (frm) { ctrl.form = frm; };

        ctrl.validate = function () {
            if (ctrl.form) {
                ctrl.form.$setSubmitted();
                return ctrl.form.$valid;
            }
            return true;
        };

        function stringifyAddress(address) {
            var stringifiedAddress = address.firstName + ' ' + address.lastName + ', ';
            stringifiedAddress += address.organization ? address.organization + ', ' : '';
            stringifiedAddress += address.countryName + ', ';
            stringifiedAddress += address.regionName ? address.regionName + ', ' : '';
            stringifiedAddress += address.city + ' ';
            stringifiedAddress += address.line1 + ', ';
            stringifiedAddress += address.line2 ? address.line2 : '';
            stringifiedAddress += address.postalCode;
            return stringifiedAddress;
        }

        $scope.$watch('$ctrl.address', function () {
            if (ctrl.address) {
                populateRegionalDataForAddress(ctrl.address);
                ctrl.address.name = stringifyAddress(ctrl.address);
            }
            ctrl.onUpdate({ address: ctrl.address });
        }, true);

    }]
});

var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcCreditCard', {
    templateUrl: "themes/assets/js/common-components/creditCard.tpl.html",
    require: {
        checkoutStep: '?^vcCheckoutWizardStep'
    },
    bindings: {
        card: '=',
        validationContainer: '='
    },
    controller: ['$scope', '$filter', function ($scope, $filter) {
        var ctrl = this;

        this.$onInit = function () {
            if(ctrl.validationContainer)
                ctrl.validationContainer.addComponent(this);
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.addComponent(this);
        };

        this.$onDestroy = function () {
            if (ctrl.validationContainer)
                ctrl.validationContainer.removeComponent(this);
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.removeComponent(this);
        };

        $scope.$watch('$ctrl.card.bankCardHolderName', function (val) {
            if (ctrl.card) {
                ctrl.card.bankCardHolderName = $filter('uppercase')(val);
            }
        }, true);

        ctrl.validate = function () {
            ctrl.form.$setSubmitted();
            return !ctrl.form.$invalid;
        }

    }]
});

angular.module('storefrontApp')

.component('vcLabeledInput', {
    templateUrl: "themes/assets/js/common-components/labeled-input.tpl.html",
    bindings: {
        value: '=',
        form: '=',
        name: '@',
        placeholder: '@',
        type: '@?',
        required: '<',
        requiredError: '@?',
        autofocus: '<'
    },
    controller: [function () {
        var $ctrl = this;
        
        $ctrl.validate = function () {
            $ctrl.form.$setSubmitted();
            return $ctrl.form.$valid;
        };

    }]
});
var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcLineItems', {
    templateUrl: "themes/assets/js/common-components/lineItems.tpl.liquid",
    bindings: {
        items: '='
    }
});

var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcPaymentMethods', {
    templateUrl: "themes/assets/js/common-components/paymentMethods.tpl.html",
    require: {
        checkoutStep: '?^vcCheckoutWizardStep'
    },
    bindings: {
        getAvailPaymentMethods: '&',
        onSelectMethod: '&',
        paymentMethod: '=',
        validationContainer: '='
    },
    controller: ['$scope', function ($scope) {
        var ctrl = this;

        this.$onInit = function () {
            ctrl.getAvailPaymentMethods().then(function (methods) {
                ctrl.availPaymentMethods = _.sortBy(methods, function (x) { return x.priority; });
                if (ctrl.paymentMethod) {
                    ctrl.paymentMethod = _.findWhere(ctrl.availPaymentMethods, { code: ctrl.paymentMethod.code });
                }
                if (!ctrl.paymentMethod && ctrl.availPaymentMethods.length > 0) {
                    ctrl.selectMethod(ctrl.availPaymentMethods[0]);
                }
            })
            if (ctrl.validationContainer)
                ctrl.validationContainer.addComponent(this);
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.addComponent(this);
        };

        this.$onDestroy = function () {
            if (ctrl.validationContainer)
                ctrl.validationContainer.removeComponent(this);
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.removeComponent(this);
        };

        ctrl.validate = function () {
            return ctrl.paymentMethod;
        }

        ctrl.selectMethod = function (method) {
            ctrl.paymentMethod = method;
            ctrl.onSelectMethod({ paymentMethod: method });
        };
    }]
});

var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcTotals', {
    templateUrl: "themes/assets/js/common-components/totals.tpl.liquid",
	bindings: {
		order: '<'
	}
});

//Call this to register our module to main application
var moduleName = "storefront.account";

if (storefrontAppDependencies !== undefined) {
    storefrontAppDependencies.push(moduleName);
}
angular.module(moduleName, ['ngResource', 'ngComponentRouter', 'credit-cards', 'pascalprecht.translate', 'ngSanitize', 'storefrontApp'])

.config(['$translateProvider', function ($translateProvider) {
    $translateProvider.useSanitizeValueStrategy('sanitizeParameters');
    $translateProvider.useUrlLoader(BASE_URL + 'themes/localization.json');
    $translateProvider.preferredLanguage('en');
}])

.run(['$templateCache', function ($templateCache) {
    // cache application level templates
    $templateCache.put('pagerTemplate.html', '<uib-pagination boundary-links="true" max-size="$ctrl.pageSettings.numPages" items-per-page="$ctrl.pageSettings.itemsPerPageCount" total-items="$ctrl.pageSettings.totalItems" ng-model="$ctrl.pageSettings.currentPage" ng-change="$ctrl.pageSettings.pageChanged()" class="pagination-sm" previous-text="&lsaquo;" next-text="&rsaquo;" first-text="&laquo;" last-text="&raquo;"></uib-pagination>');
}])

.value('$routerRootComponent', 'vcAccountManager')

.component('vcAccountManager', {
    templateUrl: "account-manager.tpl",
    bindings: {
        baseUrl: '<',
        customer: '<'
    },
    $routeConfig: [
         { path: '/orders/...', name: 'Orders', component: 'vcAccountOrders'},
         { path: '/subscriptions/...', name: 'Subscriptions', component: 'vcAccountSubscriptions' },
         { path: '/quotes', name: 'Quotes', component: 'vcAccountQuotes' },
         { path: '/profile', name: 'Profile', component: 'vcAccountProfileUpdate', useAsDefault: true },
         { path: '/addresses', name: 'Addresses', component: 'vcAccountAddresses' },
         { path: '/changePassword', name: 'PasswordChange', component: 'vcAccountPasswordChange' },
         { path: '/wishlist', name: 'WishList', component: 'vcAccountLists' }
    ],
    controller: ['storefront.accountApi', 'storefrontApp.mainContext', 'loadingIndicatorService', function (accountApi, mainContext, loader) {
        var $ctrl = this;
        $ctrl.loader = loader;

        $ctrl.getQuotes = function (pageNumber, pageSize, sortInfos, callback) {
            loader.wrapLoading(function () {
                return accountApi.getQuotes({ pageNumber: pageNumber, pageSize: pageSize, sortInfos: sortInfos }, callback).$promise;
            });
        };

        $ctrl.updateProfile = function (updateRequest) {
            loader.wrapLoading(function () {
                return accountApi.updateAccount(updateRequest, mainContext.getCustomer).$promise;
            });
        };

        $ctrl.updateAddresses = function (data) {
            return loader.wrapLoading(function () {
                return accountApi.updateAddresses(data, mainContext.getCustomer).$promise;
            });
        };

        $ctrl.availCountries = accountApi.getCountries();

        $ctrl.getCountryRegions = function (country) {
            return accountApi.getCountryRegions(country).$promise;
        };

        $ctrl.changePassword = function (changePasswordData) {
            return loader.wrapLoading(function () {
                return accountApi.changePassword(changePasswordData).$promise;
            });
        };
    }]
})

.service('confirmService', ['$q', function ($q) {
    this.confirm = function (message) {
        return $q.when(window.confirm(message || 'Is it OK?'));
    };
}])

.factory('loadingIndicatorService', function () {
    var retVal = {
        isLoading: false,
        wrapLoading: function (func) {
            retVal.isLoading = true;
            return func().then(function (result) {
                retVal.isLoading = false;
                return result;
            },
            function () { retVal.isLoading = false; });
        }
    };

    return retVal;
});
angular.module('storefront.account')
.component('vcAccountAddresses', {
    templateUrl: "themes/assets/js/account/account-addresses.tpl.liquid",
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefrontApp.mainContext', 'confirmService', '$translate', '$scope', 'loadingIndicatorService', function (mainContext, confirmService, $translate, $scope, loader) {
        var $ctrl = this;
        $ctrl.loader = loader;

        $scope.$watch(
          function () { return mainContext.customer.addresses; },
          function () {
              $ctrl.addresses = mainContext.customer.addresses;
          }
        );

        $ctrl.addNewAddress = function () {
            if (_.last(components).validate()) {
                $ctrl.addresses.push($ctrl.newAddress);
                $ctrl.newAddress = null;
                $ctrl.accountManager.updateAddresses($ctrl.addresses);
            }
        };

        $ctrl.submit = function () {
            if (components[$ctrl.editIdx].validate()) {
                angular.copy($ctrl.editItem, $ctrl.addresses[$ctrl.editIdx]);
                $ctrl.accountManager.updateAddresses($ctrl.addresses).then($ctrl.cancel);
            }
        };

        $ctrl.cancel = function () {
            $ctrl.editIdx = -1;
            $ctrl.editItem = null;
        };

        $ctrl.edit = function ($index) {
            $ctrl.editIdx = $index;
            $ctrl.editItem = angular.copy($ctrl.addresses[$ctrl.editIdx]);
        };

        $ctrl.delete = function ($index) {
            var showDialog = function (text) {
                confirmService.confirm(text).then(function (confirmed) {
                    if (confirmed) {
                        $ctrl.addresses.splice($index, 1);
                        $ctrl.accountManager.updateAddresses($ctrl.addresses);
                    }
                });
            };

            $translate('customer.addresses.delete_confirm').then(showDialog, showDialog);
        };

        var components = [];
        $ctrl.addComponent = function (component) {
            components.push(component);
        };
        $ctrl.removeComponent = function (component) {
            components = _.without(components, component);
        };
    }]
});

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

angular.module('storefront.account')
.component('vcAccountOrders', {
    templateUrl: "themes/assets/js/account/account-orders.tpl.liquid",
    $routeConfig: [
     { path: '/', name: 'OrderList', component: 'vcAccountOrdersList', useAsDefault: true },
     { path: '/:number', name: 'OrderDetail', component: 'vcAccountOrderDetail' }
    ],
    controller: ['orderHelper', function (orderHelper) {
        var $ctrl = this;
        $ctrl.orderHelper = orderHelper;
    }]
})

.component('vcAccountOrdersList', {
    templateUrl: "account-orders-list.tpl",
    controller: ['storefront.orderApi', 'loadingIndicatorService', function (orderApi, loader) {
        var ctrl = this;
        ctrl.loader = loader;
        ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        ctrl.pageSettings.pageChanged = function () {
            loader.wrapLoading(function () {
                return orderApi.search({
                    pageNumber: ctrl.pageSettings.currentPage,
                    pageSize: ctrl.pageSettings.itemsPerPageCount,
                    sortInfos: ctrl.sortInfos
                }, function (data) {
                    ctrl.entries = data.results;
                    ctrl.pageSettings.totalItems = data.totalCount;
                }).$promise;
            });
        };

        this.$routerOnActivate = function (next) {
            ctrl.pageSettings.currentPage = next.params.pageNumber || ctrl.pageSettings.currentPage;
            ctrl.pageSettings.pageChanged();
        };
    }]
})

.component('vcAccountOrderDetail', {
    templateUrl: "account-order-detail.tpl",
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefront.orderApi', '$rootScope', '$window', 'loadingIndicatorService', 'confirmService', 'orderHelper', function (orderApi, $rootScope, $window, loader, confirmService, orderHelper) {
        var $ctrl = this;
        $ctrl.loader = loader;
        $ctrl.hasPhysicalProducts = true;

        function refresh() {
            loader.wrapLoading(function () {
                $ctrl.order = orderApi.get({ number: $ctrl.orderNumber }, function (result) {
                    $ctrl.isShowPayment = false;
                    var lastPayment = _.last(_.sortBy($ctrl.order.inPayments, 'createdDate'));
                    $ctrl.billingAddress = (lastPayment && lastPayment.billingAddress) ||
                            _.findWhere($ctrl.order.addresses, { type: 'billing' }) ||
                            _.first($ctrl.order.addresses);
                    $ctrl.amountToPay = orderHelper.getNewPayment($ctrl.order).sum.amount;

                    if ($ctrl.amountToPay > 0) {
                        $ctrl.billingAddressEqualsShipping = true;
                        loadPromise = orderApi.getNewPaymentData({ number: $ctrl.orderNumber }, function (result) {
                            //$ctrl.order = result.order;
                            configurePayment(result.paymentMethods, result.payment);
                        }).$promise;
                    }
                });
                return $ctrl.order.$promise;
            });
        }

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber || 1;
            $ctrl.orderNumber = next.params.number;

            refresh();
        };

        $ctrl.getInvoicePdf = function () {
            var url = $window.BASE_URL + 'storefrontapi/orders/' + $ctrl.orderNumber + '/invoice';
            $window.open(url, '_blank');
        }

        $ctrl.showPayment = function () {
            loadPromise.then(function (result) {
                $ctrl.isShowPayment = true;
            });
        };

        var loadPromise;
        $ctrl.getAvailPaymentMethods = function () {
            return loadPromise.then(function (result) {
                var preselectedMaymentMethod;
                if ($ctrl.payment.gatewayCode) {
                    preselectedMaymentMethod = _.findWhere(result.paymentMethods, { code: $ctrl.payment.gatewayCode });
                }

                return preselectedMaymentMethod ? [preselectedMaymentMethod] : result.paymentMethods;
            });
        };

        $ctrl.selectPaymentMethod = function (paymentMethod) {
            angular.extend($ctrl.payment, paymentMethod);
            $ctrl.payment.gatewayCode = paymentMethod.code;
            // $ctrl.payment.sum = angular.copy($ctrl.order.total);
            // $ctrl.payment.sum.amount += paymentMethod.totalWithTax.amount;

            $ctrl.validate();
        };

        $ctrl.validate = function () {
            $ctrl.isValid = $ctrl.payment &&
                $ctrl.payment.gatewayCode &&
                $ctrl.payment.sum && $ctrl.payment.sum.amount > 0 &&
                _.every(components, function (x) {
                    return typeof x.validate !== "function" || x.validate();
                });

            return $ctrl.isValid;
        };

        $ctrl.submit = function () {
            if ($ctrl.validate()) {
                loader.wrapLoading(function () {
                    $ctrl.payment.bankCardInfo = $ctrl.paymentMethod.card;
                    return orderApi.addOrUpdatePayment({ number: $ctrl.orderNumber }, $ctrl.payment, function (payment) {
                        orderApi.processPayment({ number: $ctrl.orderNumber, paymentNumber: payment.number }, $ctrl.paymentMethod.card, function (result) {
                            var orderProcessingResult = result.orderProcessingResult;
                            var paymentMethod = result.paymentMethod;

                            if (!orderProcessingResult.isSuccess) {
                                $rootScope.$broadcast('storefrontError', {
                                    type: 'error',
                                    title: ['Error in new payment processing: ', orderProcessingResult.error, 'New Payment status: ' + orderProcessingResult.newPaymentStatus].join(' '),
                                    message: orderProcessingResult.error,
                                });
                                return;
                            }

                            if (paymentMethod.paymentMethodType && paymentMethod.paymentMethodType.toLowerCase() === 'preparedform' && orderProcessingResult.htmlForm) {
                                outerRedirect($ctrl.accountManager.baseUrl + 'cart/checkout/paymentform?orderNumber=' + $ctrl.orderNumber);
                            } else if (paymentMethod.paymentMethodType && paymentMethod.paymentMethodType.toLowerCase() === 'redirection' && orderProcessingResult.redirectUrl) {
                                outerRedirect(orderProcessingResult.redirectUrl);
                            } else {
                                if ($ctrl.accountManager.customer.isRegisteredUser) {
                                    refresh();
                                } else {
                                    outerRedirect($ctrl.accountManager.baseUrl + 'cart/thanks/' + $ctrl.orderNumber);
                                }
                            }
                        })
                    }).$promise;
                });
            }
        };

        $ctrl.cancel = function () {
            confirmService.confirm('Cancel this payment?').then(function (confirmed) {
                if (confirmed) {
                    loader.wrapLoading(function () {
                        return orderApi.cancelPayment({ number: $ctrl.orderNumber, paymentNumber: $ctrl.payment.number }, null, refresh).$promise;
                    });
                }
            });
        };

        var components = [];
        $ctrl.addComponent = function (component) {
            components.push(component);
        };
        $ctrl.removeComponent = function (component) {
            components = _.without(components, component);
        };

        function configurePayment(paymentMethods, newPaymentTemplate) {
            $ctrl.payment = orderHelper.getNewPayment($ctrl.order, paymentMethods, newPaymentTemplate);
            $ctrl.payment.purpose = $ctrl.payment.purpose || 'Repeated payment';
            $ctrl.amountToPay = $ctrl.payment.sum.amount;

            $ctrl.canCancelPayment = $ctrl.payment.id !== newPaymentTemplate.id;
            if ($ctrl.canCancelPayment) {
                $ctrl.selectPaymentMethod(_.findWhere(paymentMethods, { code: $ctrl.payment.gatewayCode }));
            }

            if (!_.some($ctrl.order.shipments)) {
                $ctrl.hasPhysicalProducts = false;
                $ctrl.billingAddressEqualsShipping = false;
            }
        }

        function outerRedirect(absUrl) {
            $window.location.href = absUrl;
        };
    }]
})

.factory('orderHelper', function () {
    var retVal = {
        getNewPayment: function (order, paymentMethods, newPaymentTemplate) {
            var retVal;
            var paidPayments = _.filter(order.inPayments, function (x) {
                return x.status === 'Paid';
            });
            var paidAmount = _.reduce(paidPayments, function (memo, num) { return memo + num.sum.amount; }, 0);
            var amountToPay = order.total.amount - paidAmount;

            var pendingPayments = _.filter(order.inPayments, function (x) {
                return !x.isCancelled &&
                        (x.status === 'New' || x.status === 'Pending') &&
                        x.sum.amount > 0; // && x.sum.amount === amountToPay;
            });
            var pendingPayment = _.last(_.sortBy(pendingPayments, 'createdDate'));
            if (pendingPayment && (!paymentMethods || _.findWhere(paymentMethods, { code: pendingPayment.gatewayCode }))) {
                retVal = pendingPayment;
            } else {
                newPaymentTemplate = newPaymentTemplate || { sum: {} };
                newPaymentTemplate.sum.amount = amountToPay;
                retVal = newPaymentTemplate;
            }

            return retVal;
        }
    };

    return retVal;
})

.filter('orderToSummarizedStatusLabel', ['orderHelper', function (orderHelper) {
    return function (order) {
        var retVal = order.status || 'New';

        var found = _.findWhere(orderHelper.statusLabels, { status: retVal.toLowerCase() });
        if (found) {
            retVal = found.label;
        }

        return retVal;
    };
}])
;

angular.module('storefront.account')
.component('vcAccountPasswordChange', {
    templateUrl: "themes/assets/js/account/account-password-change.tpl.liquid",
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['loadingIndicatorService', function (loader) {
        var ctrl = this;
        ctrl.loader = loader;
        ctrl.passwordChangeData = {};

        ctrl.submit = function () {
            // validation
            ctrl.errors = null;
            ctrl.error = {};
            var hasError = false;
            var errorMsg;

            errorMsg = ctrl.passwordChangeData.oldPassword === ctrl.passwordChangeData.newPassword;
            ctrl.error.newPassword = errorMsg
            hasError = hasError || errorMsg;

            if (!hasError) {
                errorMsg = ctrl.passwordChangeData.newPassword !== ctrl.passwordChangeData.newPassword2;
                ctrl.error.newPassword2 = errorMsg;
                hasError = hasError || errorMsg;
            }

            if (!hasError) {
                ctrl.accountManager.changePassword(ctrl.passwordChangeData).then(function (result) {
                    angular.extend(ctrl, result);
                    ctrl.passwordChangeData = {};
                    ctrl.form.$setPristine();
                });
            }
        };

        ctrl.setForm = function (frm) { ctrl.form = frm; };
    }]
});

angular.module('storefront.account')
.component('vcAccountProfileUpdate', {
    templateUrl: "themes/assets/js/account/account-profile-update.tpl.liquid",
    bindings: {
        $router: '<'
    },
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefrontApp.mainContext', '$scope', 'loadingIndicatorService', function (mainContext, $scope, loader) {
        var $ctrl = this;
        $ctrl.loader = loader;
        
        $scope.$watch(
            function () { return mainContext.customer; },
            function (customer) {
                $ctrl.customer = customer;
                if (customer) {
                    if (customer.isContract) {
                        $ctrl.$router.navigate(['Orders']);
                    }
                    $ctrl.changeData =
                    {
                        firstName: customer.firstName,
                        lastName: customer.lastName,
                        email: customer.email
                    };
                }
            });

        $ctrl.submit = function () {
            // no validation
            $ctrl.accountManager.updateProfile($ctrl.changeData);
        };
    }]
});

angular.module('storefront.account')
.component('vcAccountQuotes', {
    templateUrl: "themes/assets/js/account/account-quotes.tpl.liquid",
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: [function () {
        var ctrl = this;
        ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        ctrl.pageSettings.pageChanged = function () {
            ctrl.accountManager.getQuotes(ctrl.pageSettings.currentPage, ctrl.pageSettings.itemsPerPageCount, ctrl.sortInfos, function (data) {
                ctrl.entries = data.results;
                ctrl.pageSettings.totalItems = data.totalCount;
            });
        };
        
        this.$routerOnActivate = function (next) {
            ctrl.pageSettings.currentPage = next.params.pageNumber || ctrl.pageSettings.currentPage;
            ctrl.pageSettings.pageChanged();
        };
    }]
});

angular.module('storefront.account')
.component('vcAccountSubscriptions', {
    templateUrl: "themes/assets/js/account/account-subscriptions.tpl.liquid",
    $routeConfig: [
     { path: '/', name: 'SubscriptionList', component: 'vcAccountSubscriptionsList', useAsDefault: true },
     { path: '/:number', name: 'SubscriptionDetail', component: 'vcAccountSubscriptionDetail' }
    ]
})

.component('vcAccountSubscriptionsList', {
    templateUrl: "account-subscriptions-list.tpl",
    controller: ['storefront.subscriptionApi', 'confirmService', 'loadingIndicatorService', '$translate', function (subscriptionApi, confirmService, loader, $translate) {
        var $ctrl = this;
        $ctrl.loader = loader;
        $ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        $ctrl.pageSettings.pageChanged = function () {
            loader.wrapLoading(function () {
                return subscriptionApi.search({
                    pageNumber: $ctrl.pageSettings.currentPage,
                    pageSize: $ctrl.pageSettings.itemsPerPageCount,
                    sortInfos: $ctrl.sortInfos
                }, function (data) {
                    $ctrl.entries = data.results;
                    $ctrl.pageSettings.totalItems = data.totalCount;
                }).$promise;
            });
        };

        this.$routerOnActivate = function (next) {
            $ctrl.pageSettings.currentPage = next.params.pageNumber || $ctrl.pageSettings.currentPage;
            $ctrl.pageSettings.pageChanged();
        };
    }]
})

.component('vcAccountSubscriptionDetail', {
    templateUrl: "account-subscription-detail.tpl",
    controller: ['storefront.subscriptionApi', 'confirmService', 'loadingIndicatorService', '$translate', function (subscriptionApi, confirmService, loader, $translate) {
        var $ctrl = this;
        $ctrl.loader = loader;

        function refresh() {
            loader.wrapLoading(function () {
                return subscriptionApi.get({ number: $ctrl.entryNumber }, function (result) {
                    $ctrl.subscription = angular.copy(result);
                }).$promise;
            });
        }

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber || 1;
            $ctrl.entryNumber = next.params.number;

            refresh();
        };

        $ctrl.cancel = function () {
            //var showDialog = function (text) {
            //    confirmService.confirm(text).then(function (confirmed) {
            //        if (confirmed) {
            loader.wrapLoading(function () {
                return subscriptionApi.cancel({ number: $ctrl.entryNumber }, { number: $ctrl.entryNumber, cancelReason: $ctrl.cancelReason }, function (result) {
                    $ctrl.subscription = angular.copy(result);
                    $ctrl.isCancelFormVisible = false;
                }).$promise;
            });
            //        }
            //    });
            //};
            //$translate('customer.subscription.cancel_confirmation').then(showDialog, showDialog);
        };
    }]
})

.filter('toIntervalKey', function () {
    return function (data, data_intervalCount) {
        var retVal = 'customer.subscriptions.intervals.' + data.interval.toLowerCase() + '_' + (data_intervalCount === 1 ? 1 : 'plural');
        //var everyKey = 'customer.subscriptions.intervals.every';

        //$translate([intervalKey, everyKey]).then(function (translations) {
        //var intervalVal = translations[intervalKey];
        //  var everyVal = translations[everyKey];

        //if (data_intervalCount === 1) {
        //    retVal = intervalKey;
        //} else {
        //    retVal = data_intervalCount + intervalVal;
        //}
        //});

        return retVal;
    };
})
;
angular.module('storefront.account')
    .factory('storefront.accountApi', ['$resource', function ($resource) {
        return $resource('storefrontapi/account', null, {
            updateAccount: { url: 'storefrontapi/account', method: 'POST' },
            changePassword: { url: 'storefrontapi/account/password', method: 'POST' },
            getQuotes: { url: 'storefrontapi/account/quotes' },
            updateAddresses: { url: 'storefrontapi/account/addresses', method: 'POST' },
            getCountries: { url: 'storefrontapi/countries', isArray: true },
            getCountryRegions: { url: 'storefrontapi/countries/:code3/regions', isArray: true }
        });
    }])
    .factory('storefront.orderApi', ['$resource', function ($resource) {
        return $resource('storefrontapi/orders/:number', null, {
            search: { url: 'storefrontapi/orders/search', method: 'POST' },
            getNewPaymentData: { url: 'storefrontapi/orders/:number/newpaymentdata' },
            addOrUpdatePayment: { url: 'storefrontapi/orders/:number/payments', method: 'POST' },
            processPayment: { url: 'storefrontapi/orders/:number/payments/:paymentNumber/process', method: 'POST' },
            cancelPayment: { url: 'storefrontapi/orders/:number/payments/:paymentNumber/cancel', method: 'POST' }
        });
    }])
    .factory('storefront.subscriptionApi', ['$resource', function ($resource) {
        return $resource('storefrontapi/subscriptions/:number', null, {
            search: { url: 'storefrontapi/subscriptions/search', method: 'POST' },
            cancel: { url: 'storefrontapi/subscriptions/:number/cancel', method: 'POST' }
        });
    }]);
//# sourceMappingURL=account-scripts.js.map
