var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcCheckoutWizardStep', {
	templateUrl: "themes/assets/js/checkout/checkout-wizard-step.tpl.html",
	transclude: true,
	require: {
		wizard: '^vcCheckoutWizard'
	},
	bindings: {
		name: '@',
		title: '@',
		disabled: '=?',
		onNextStep: '&?',
		canEnter: '=?',
		final: '<?'
	},
	controller: ['$scope', 'cartService', function ($scope, cartService) {
		var ctrl = this;
		ctrl.components = [];
		ctrl.canEnter = true;

		this.$onInit = function () {
			ctrl.wizard.addStep(this);
		};

		ctrl.addComponent = function (component) {		
			ctrl.components.push(component);
		};
		ctrl.removeComponent = function (component) {
			ctrl.components = _.without(ctrl.components, _.find(ctrl.components, function (x) { return x == component; }));
		};
		ctrl.validate = function () {
			return _.every(ctrl.components, function (x) { return typeof x.validate !== "function" || x.validate(); });
		}
	}]
});
