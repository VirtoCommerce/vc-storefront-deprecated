var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('checkoutController2', ['$rootScope', '$scope', '$window', 'cartService',
    function ($rootScope, $scope, $window, cartService) {
    	$scope.checkout = {
    		wizard : {},
    		paymentMethod: {},
    		shipment: {},
    		payment: {},
    		coupon: {},
    		availCountries: [],
    		loading: false,
    		isValid: false
    	};

    	function validateCheckout(checkout) {
    		checkout.isValid = checkout.payment && angular.isDefined(checkout.payment.paymentGatewayCode);
    		if (checkout.isValid) {
    			checkout.isValid = checkout.shipment && angular.isDefined(checkout.shipment.shipmentMethodCode);
    		}
    		if (checkout.isValid && checkout.cart.hasPhysicalProducts) {
    			checkout.isValid = checkout.shipment.deliveryAddress && angular.isDefined(checkout.shipment.deliveryAddress);
    		}
    		if (checkout.isValid && !checkout.billingAddressEqualsShipping) {
    			checkout.isValid = checkout.payment.billingAddress && angular.isDefined(checkout.payment.billingAddress);
    		}
    	};

    	$scope.reloadCart = function () {
    		return wrapLoading(function() {
    			return cartService.getCart();
    		}).then(function (response) {
    			var cart = response.data;
    			if (!cart || !cart.id) {
    				$scope.outerRedirect($scope.baseUrl + 'cart');
    			}
    			else {
    				$scope.checkout.cart = cart;
    				$scope.checkout.coupon = cart.coupon || $scope.checkout.coupon;
    				$scope.checkout.payment = cart.payments.length ? cart.payments[0] : $scope.checkout.payment;
    				$scope.checkout.paymentMethod.gatewayCode = cart.payments.length ? cart.payments[0].paymentGatewayCode : $scope.checkout.paymentMethod.gatewayCode;
    				$scope.checkout.shipment = cart.shipments.length ? cart.shipments[0] : $scope.checkout.shipment;
    				$scope.checkout.billingAddressEqualsShipping = !angular.isDefined($scope.checkout.payment.billingAddress);
    			}
    			validateCheckout($scope.checkout);
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
    		$scope.checkout.payment.paymentGatewayCode = paymentMethod.gatewayCode;
    		validateCheckout($scope.checkout);
    	};

    	$scope.updatePayment = function () {
    		if ($scope.checkout.billingAddressEqualsShipping) {
    			$scope.checkout.payment.billingAddress = undefined;
    		}

    		if ($scope.checkout.payment.billingAddress) {
    			$scope.checkout.payment.billingAddress.email = $scope.checkout.email;
    			$scope.checkout.payment.billingAddress.type = 'Billing';

    		}
    		return wrapLoading(function() {
    			return cartService.addOrUpdatePayment($scope.checkout.payment)
    		});
    	}

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
    			return cartService.getAvailableShippingMethods(shipment.id);
    		}).then(function (response) {
    			return response.data;
    		});
    	}

    	$scope.getAvailPaymentMethods = function () {
    		return wrapLoading(function () {
    			return cartService.getAvailablePaymentMethods();
    		}).then(function (response) {
    			return response.data;
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
    			return cartService.addOrUpdateShipment(shipment)
    		}).then(function (response) {
    			$scope.reloadCart();
    		});
    	};

    	$scope.createOrder = function () {
    		return wrapLoading(function () {
    			return cartService.createOrder($scope.checkout.bankCardInfo);
    		}).then(function (response) {
    			var order = response.data.order;
    			var orderProcessingResult = response.data.orderProcessingResult;
    			handlePostPaymentResult(order, orderProcessingResult);
    		});
    	}

    	function handlePostPaymentResult(order, orderProcessingResult) {
    		if (!orderProcessingResult.isSuccess) {
    			$rootScope.$broadcast('storefrontError', {
    				type: 'error',
    				title: ['Error in new order processing: ', orderProcessingResult.error, 'New Payment status: ' + orderProcessingResult.newPaymentStatus].join(' '),
    				message: orderProcessingResult.error,
    			});
    			return;
    		}
    		if (orderProcessingResult.paymentMethodType == 'PreparedForm' && orderProcessingResult.htmlForm) {
    			$scope.outerRedirect($scope.baseUrl + 'cart/checkout/paymentform?orderNumber=' + order.number);
    		}
    		if (orderProcessingResult.paymentMethodType == 'Standard' || orderProcessingResult.paymentMethodType == 'Unknown') {
    			if (!$scope.customer.HasAccount) {
    				$scope.outerRedirect($scope.baseUrl + 'cart/thanks/' + order.number);
    			} else {
    				$scope.outerRedirect($scope.baseUrl + 'account/order/' + order.number);
    			}
    		}
    		if (orderProcessingResult.paymentMethodType == 'Redirection' && orderProcessingResult.redirectUrl) {
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
    	$scope.reloadCart();

    	getAvailCountries().then(function (countries) {
    		$scope.checkout.availCountries = countries;
    	});

    }]);
