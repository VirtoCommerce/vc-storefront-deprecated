var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcCheckoutCreditCard', {
	templateUrl: "themes/assets/js/checkout/checkout-creditCard.tpl.html",
	require: {
		checkoutStep: '^vcCheckoutWizardStep'
	},
	bindings: {
		card: '='
	},
	controller: ['$scope', '$filter', function ($scope, $filter) {
		var ctrl = this;		
	
		this.$onInit = function () {
			ctrl.checkoutStep.addComponent(this);
		};

		this.$onDestroy = function () {
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
