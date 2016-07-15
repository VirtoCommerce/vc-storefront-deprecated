var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcCheckoutWizard', {
	transclude: true,
	templateUrl: 'themes/assets/js/checkout/checkout-wizard.tpl.html',
	bindings: {
		wizard: '=',
		loading: '=',
		onFinish: '&?'
	},
	controller: ['$scope', function ($scope) {
		var ctrl = this;
		ctrl.wizard = ctrl;
		ctrl.steps = [];
		ctrl.currentStep = {};

		ctrl.goToStep = function (step) {
			if (step && ctrl.currentStep != step && step.canEnter) {
				if (!step.final) {
					step.isActive = true;
					ctrl.currentStep.isActive = false;
					ctrl.currentStep = step;
				}
				else if (ctrl.onFinish)
				{
					ctrl.onFinish();
				}
			}
		};

		ctrl.nextStep = function () {
			if (!ctrl.currentStep.validate || ctrl.currentStep.validate()) {
				if (ctrl.currentStep.nextStep) {
					if (ctrl.currentStep.onNextStep) {
						ctrl.currentStep.onNextStep();
					}
					ctrl.goToStep(ctrl.currentStep.nextStep);
				}			
			}
		};

		ctrl.prevStep = function () {
			ctrl.goToStep(ctrl.currentStep.prevStep);
		};

		function rebuildStepsLinkedList(steps) {
			var nextStep = undefined;
			for (var i = steps.length; i-- > 0;) {
				steps[i].prevStep = undefined;
				steps[i].nextStep = undefined;
				if (nextStep && !steps[i].disabled) {
					nextStep.prevStep = steps[i]
				};				
				if (!steps[i].disabled) {
					steps[i].nextStep = nextStep;
					nextStep = steps[i];
				}
			}
		};
		
		ctrl.addStep = function (step) {
			if (!ctrl.currentStep || !ctrl.currentStep.isActive) {
				ctrl.goToStep(step);
			}		
			ctrl.steps.push(step);
			$scope.$watch(function () { return step.disabled; }, function () {
				rebuildStepsLinkedList(ctrl.steps);
			});
		};

	}]
});
