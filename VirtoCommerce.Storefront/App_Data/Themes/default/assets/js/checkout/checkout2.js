var storefrontApp = angular.module('storefrontApp');

storefrontApp.controller('checkoutController2', ['$rootScope', '$scope', '$window', 'cartService',
    function ($rootScope, $scope, $window, cartService) {
    	$scope.checkout = {};

    	$scope.reloadCart = function () {
    		cartService.getCart().then(function (response) {
    			var cart = response.data;
    			$scope.checkout.cart = cart;
    			$scope.checkout.shipment = cart.shipments.length ? cart.shipments[0] : {};
    		});
    	};

    	$scope.updateShipment = function (shipment) {
    		cartService.addOrUpdateShipment(shipment).then(function (response) {
    			$scope.reloadCart();
    		})
    	};

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
