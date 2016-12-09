var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcLabeledSelect', {
    templateUrl: "themes/assets/js/common-components/labeled-select.tpl.html",
    //require: {
    //	checkoutStep: '^vcCheckoutWizardStep'
    //},
    bindings: {
        value: '=',
        form: '=',
        placeholder: '@',
        noSelectionLabel: '@',
        options: '<'
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
