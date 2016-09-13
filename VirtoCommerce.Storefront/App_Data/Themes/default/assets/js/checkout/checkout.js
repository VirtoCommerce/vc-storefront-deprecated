//Call this to register our module to main application
var moduleName = "storefront.checkout";

if (storefrontAppDependencies != undefined) {
	storefrontAppDependencies.push(moduleName);
}
angular.module(moduleName, ['credit-cards', 'angular.filter'])
.controller('checkoutController', ['$rootScope', '$scope', '$window', 'cartService',
    function ($rootScope, $scope, $window, cartService) {
    	$scope.checkout = {
    		wizard: {},
			email: {},
    		paymentMethod: {},
    		shipment: {},
    		payment: {},
    		coupon: {},
    		availCountries: [],
    		loading: false,
    		isValid: false
    	};

    	$scope.validateCheckout = function (checkout) {
    		checkout.isValid = checkout.payment && angular.isDefined(checkout.payment.paymentGatewayCode);
    		if (checkout.isValid && !checkout.billingAddressEqualsShipping) {
    			checkout.isValid = checkout.payment.billingAddress && angular.isDefined(checkout.payment.billingAddress);
    		}
    		if ($scope.checkout.cart && $scope.checkout.cart.hasPhysicalProducts) {
    			if (checkout.isValid) {
    				checkout.isValid = checkout.shipment && angular.isDefined(checkout.shipment.shipmentMethodCode);
    			}
    			if (checkout.isValid && checkout.cart.hasPhysicalProducts) {
    				checkout.isValid = checkout.shipment.deliveryAddress && angular.isDefined(checkout.shipment.deliveryAddress);
    			}    		
    		}
    	};

    	$scope.reloadCart = function () {
    		return cartService.getCart().then(function (response) {
    			var cart = response.data;
    			if (!cart || !cart.id) {
    				$scope.outerRedirect($scope.baseUrl + 'cart');
    			}
    			else {
    				$scope.checkout.cart = cart;
    				$scope.checkout.email = cart.email;
    				$scope.checkout.coupon = cart.coupon || $scope.checkout.coupon;
    				if (!$scope.checkout.coupon.appliedSuccessfully) {
    					$scope.checkout.coupon.errorCode = 'InvalidCouponCode';
    				}
    				if (cart.payments.length) {
    					$scope.checkout.payment = cart.payments[0];
    					$scope.checkout.paymentMethod.code = $scope.checkout.payment.paymentGatewayCode;
    				}       				
    				if (cart.shipments.length)
    				{
    					$scope.checkout.shipment = cart.shipments[0];
    				}
    				$scope.checkout.billingAddressEqualsShipping = !angular.isDefined($scope.checkout.payment.billingAddress);
    				if(!cart.hasPhysicalProducts)
    				{
    					$scope.checkout.billingAddressEqualsShipping = false;
    				}
    			}
    			$scope.validateCheckout($scope.checkout);
    			return cart;
    		});
    	};

    	$scope.applyCoupon = function (coupon) {
    		coupon.processing = true;
    		cartService.addCoupon(coupon.code).then(function (response) {
    			var coupon = response.data;
    			coupon.processing = false;
    			$scope.checkout.coupon = coupon;
    			if (!coupon.appliedSuccessfully) {
    				coupon.errorCode = 'InvalidCouponCode';
    			}
    			$scope.reloadCart();
    		}, function (response) {
    			coupon.processing = false;
    		});
    	}

    	$scope.removeCoupon = function (coupon) {
    		coupon.processing = true;
    		cartService.removeCoupon().then(function (response) {
    			coupon.processing = false;
    			$scope.checkout.coupon = null;
    			$scope.reloadCart();
    		}, function (response) {
    			coupon.processing = false;
    		});
    	}

    	$scope.selectPaymentMethod = function (paymentMethod) {
    		$scope.checkout.payment.paymentGatewayCode = paymentMethod.code;
    		$scope.checkout.payment.amount = $scope.checkout.cart.total;
    		$scope.validateCheckout($scope.checkout);
    	};    

    	function getAvailCountries() {
    		//Load avail countries
    		return cartService.getCountries().then(function (response) {
    			return response.data;
    		});
    	};

    	$scope.getCountryRegions = function(country) {
    		return cartService.getCountryRegions(country.code3).then(function (response) {
    			return response.data;
    		});
    	};

    	$scope.getAvailShippingMethods = function (shipment) {
    		return wrapLoading(function () {
    			return cartService.getAvailableShippingMethods(shipment.id).then(function (response) {
    				return response.data;
    			});
    		});
    	}

    	$scope.getAvailPaymentMethods = function () {
    		return wrapLoading(function () {
    			return cartService.getAvailablePaymentMethods().then(function (response) {
    				return response.data;
    			});
    		});
    	};

    	$scope.selectShippingMethod = function (shippingMethod) {
    		if(shippingMethod)
    		{
    			$scope.checkout.shipment.shipmentMethodCode = shippingMethod.shipmentMethodCode;
    			$scope.checkout.shipment.shipmentMethodOption = shippingMethod.optionName;
    		}
    		else
    		{
    			$scope.checkout.shipment.shipmentMethodCode = undefined;
    			$scope.checkout.shipment.shipmentMethodOption = undefined;
    		}
    		$scope.updateShipment($scope.checkout.shipment);
    	};

    	$scope.updateShipment = function (shipment) {
    		if (shipment.deliveryAddress) {
    			$scope.checkout.shipment.deliveryAddress.email = $scope.checkout.email;
    			$scope.checkout.shipment.deliveryAddress.type = 'Shipping';
    		};
    		return wrapLoading(function () {
    			return cartService.addOrUpdateShipment(shipment).then($scope.reloadCart);
    		});
    	};

    	$scope.createOrder = function () {
    		$scope.checkout.loading = true;
		    updatePayment($scope.checkout.payment).then(function () {
    				return cartService.createOrder($scope.checkout.bankCardInfo);
    			}).then(function (response) {
    				var order = response.data.order;
    				var orderProcessingResult = response.data.orderProcessingResult;
    				var paymentMethod = response.data.paymentMethod;
    				handlePostPaymentResult(order, orderProcessingResult, paymentMethod);
    			});
    	}

    	function updatePayment(payment) {
    		if ($scope.checkout.billingAddressEqualsShipping) {
    			payment.billingAddress = undefined;
    		}

    		if (payment.billingAddress) {
    			payment.billingAddress.email = $scope.checkout.email;
    			payment.billingAddress.type = 'Billing';
    		}
    		return cartService.addOrUpdatePayment(payment)
    	}

    	function handlePostPaymentResult(order, orderProcessingResult, paymentMethod) {
    		if (!orderProcessingResult.isSuccess) {
    			$rootScope.$broadcast('storefrontError', {
    				type: 'error',
    				title: ['Error in new order processing: ', orderProcessingResult.error, 'New Payment status: ' + orderProcessingResult.newPaymentStatus].join(' '),
    				message: orderProcessingResult.error,
    			});
    			return;
    		}
    		if (paymentMethod.paymentMethodType == 'PreparedForm' && orderProcessingResult.htmlForm) {
    			$scope.outerRedirect($scope.baseUrl + 'cart/checkout/paymentform?orderNumber=' + order.number);
    		}
    		if (paymentMethod.paymentMethodType == 'Standard' || paymentMethod.paymentMethodType == 'Unknown') {
    			if (!$scope.customer.HasAccount) {
    				$scope.outerRedirect($scope.baseUrl + 'cart/thanks/' + order.number);
    			} else {
    				$scope.outerRedirect($scope.baseUrl + 'account/order/' + order.number);
    			}
    		}
    		if (paymentMethod.paymentMethodType == 'Redirection' && orderProcessingResult.redirectUrl) {
    			$window.location.href = orderProcessingResult.redirectUrl;
    		}
    	}

    	function wrapLoading(func) {
    		$scope.checkout.loading = true;
    		return func().then(function (result) {
    			$scope.checkout.loading = false;
    			return result;
    		},
				function () {
					$scope.checkout.loading = false;
				});
    	}

    	$scope.initialize = function () {

    		$scope.reloadCart().then(function (cart) {
    			$scope.checkout.wizard.goToStep(cart.hasPhysicalProducts ? 'shipping-address' : 'payment-method');
    		});
    	};    

    	getAvailCountries().then(function (countries) {
    		$scope.checkout.availCountries = countries;
    	});

    }]);
