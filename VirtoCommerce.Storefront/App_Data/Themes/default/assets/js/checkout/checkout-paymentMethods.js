var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcCheckoutPaymentMethods', {
    templateUrl: "themes/assets/js/checkout/checkout-paymentMethods.tpl.html",
    require: {
        checkoutStep: '?^vcCheckoutWizardStep'
    },
    bindings: {
        getAvailPaymentMethods: '&',
        onSelectMethod: '&',
        paymentMethod: '='
    },
    controller: ['$scope', function ($scope) {
        var ctrl = this;

        this.$onInit = function () {
            ctrl.getAvailPaymentMethods().then(function (methods) {
                ctrl.availPaymentMethods = _.sortBy(methods, function (x) { return x.priority; });
                if (ctrl.paymentMethod) {
                    ctrl.paymentMethod = _.find(ctrl.availPaymentMethods, function (x) { return x.code === ctrl.paymentMethod.code; })
                }
                if (!ctrl.paymentMethod && ctrl.availPaymentMethods.length > 0) {
                    ctrl.selectMethod(ctrl.availPaymentMethods[0]);
                }
            })
            if (ctrl.checkoutStep)
                ctrl.checkoutStep.addComponent(this);
        };

        this.$onDestroy = function () {
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
