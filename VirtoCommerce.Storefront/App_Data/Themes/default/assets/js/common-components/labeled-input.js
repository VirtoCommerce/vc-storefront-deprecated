angular.module('storefrontApp')

.component('vcLabeledInput', {
    templateUrl: "themes/assets/js/common-components/labeled-input.tpl.html",
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
        
        $ctrl.validate = function () {
            $ctrl.form.$setSubmitted();
            return $ctrl.form.$valid;
        };

    }]
});