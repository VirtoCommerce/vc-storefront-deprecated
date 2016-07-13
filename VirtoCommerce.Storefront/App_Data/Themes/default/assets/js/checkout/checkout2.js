var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('checkoutController2', ['$rootScope', '$scope', '$window', 'cartService',
    function ($rootScope, $scope, $window, cartService) {
    	$scope.checkout = {
    		paymentMethod: {},
    		shipment: {},
    		payment: {}
    	};
    	$scope.reloadCart = function () {

    		cartService.getCart().then(function (response) {
    			var cart = response.data;
    			$scope.checkout.cart = cart;
    			$scope.checkout.payment = cart.payments.length ? cart.payments[0] : $scope.checkout.payment;
    			$scope.checkout.paymentMethod.gatewayCode = cart.payments.length ? cart.payments[0].paymentGatewayCode : $scope.checkout.paymentMethod.gatewayCode;
    			$scope.checkout.shipment = cart.shipments.length ? cart.shipments[0] : $scope.checkout.shipment;
    			$scope.checkout.billingAddressEqualsShipping = angular.isDefined($scope.checkout.payment.billingAddress);
    		});
    	};

    	$scope.selectPaymentMethod = function(paymentMethod)
    	{
    		$scope.checkout.payment.paymentGatewayCode = paymentMethod.gatewayCode;
    	};

    	$scope.updatePayment = function () {
    		if ($scope.checkout.billingAddressEqualsShipping)
    		{
    			$scope.checkout.payment.billingAddress = undefined;
    		}

    		if ($scope.checkout.payment.billingAddress) {
    			$scope.checkout.payment.billingAddress.email = $scope.checkout.email;
    			$scope.checkout.payment.billingAddress.type = 'Billing';

    		}
    		cartService.addOrUpdatePayment($scope.checkout.payment);
    	}

    	$scope.getAvailCountries = function () {
    		//Load avail countries
    		return cartService.getCountries().then(function (response) {
    			return response.data;
    		});
    	};
    	$scope.getAvailShippingMethods = function (shipment) {
    		return cartService.getAvailableShippingMethods(shipment.id);
    	}

    	$scope.getAvailPaymentMethods = function () {
    		return cartService.getAvailablePaymentMethods();
    	};

    	$scope.updateShipment = function (shipment) {
    		if (shipment.deliveryAddress) {
    			$scope.checkout.shipment.deliveryAddress.email = $scope.checkout.email;
    			$scope.checkout.shipment.deliveryAddress.type = 'Shipping';
    		};

    		cartService.addOrUpdateShipment(shipment).then(function (response) {
    			$scope.reloadCart();
    		})
    	};

    	$scope.createOrder = function() {
    		cartService.createOrder($scope.checkout.bankCardInfo).then(function (response) {
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

    	$scope.reloadCart();
    }]);

storefrontApp.component('vcCheckout', {
	transclude: true,
	templateUrl: 'themes/assets/js/checkout/checkout2.tpl.html',
	bindings: {
	},
	controller: ['$scope', function ($scope) {
		var ctrl = this;	
		ctrl.address = {};
		ctrl.steps = [];
		ctrl.currentStep = {};

		this.$onInit = function () {
		};

		ctrl.selectStep = function (step) {
			if (ctrl.currentStep != step) {
				step.isActive = true;
				ctrl.currentStep.isActive = false;
				ctrl.currentStep = step;
			}
		};

		ctrl.nextStep = function () {
			if (ctrl.currentStep.validate()) {
				if (ctrl.currentStep.onNextStep) {
					ctrl.currentStep.onNextStep();
				}
				ctrl.selectStep(ctrl.currentStep.nextStep);
			}
		};

		ctrl.prevStep = function () {
			ctrl.selectStep(ctrl.currentStep.prevStep);
		};

		ctrl.addStep = function (step) {
			step.isActive = false;
			if (ctrl.steps.length === 0) {
				ctrl.selectStep(step);
			}
			step.prevStep = ctrl.steps[ctrl.steps.length - 1];
			if (step.prevStep)
			{
				step.prevStep.nextStep = step;
			}
			ctrl.steps.push(step);
		};

	}]
});
