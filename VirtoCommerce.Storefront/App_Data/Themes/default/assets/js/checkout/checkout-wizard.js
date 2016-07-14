var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcCheckoutWizard', {
	transclude: true,
	templateUrl: 'themes/assets/js/checkout/checkout-wizard.tpl.html',
	bindings: {
		wizard: '=',
	},
	controller: ['$scope', function ($scope) {
		var ctrl = this;
		ctrl.wizard = ctrl;
		ctrl.steps = [];
		ctrl.currentStep = {};

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
