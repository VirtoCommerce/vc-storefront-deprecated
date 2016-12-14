var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcLabeledInput', {
    templateUrl: "themes/assets/js/common-components/labeled-input.tpl.html",
    //require: {
    //	checkoutStep: '^vcCheckoutWizardStep'
    //},
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

        this.$onInit = function () {
            var a = $ctrl.form;
        };

        //this.$onDestroy = function () {
        //    $ctrl.checkoutStep.removeComponent(this);
        //};

        $ctrl.validate = function () {
            $ctrl.form.$setSubmitted();
            return $ctrl.form.$valid;
        }

    }]
});
