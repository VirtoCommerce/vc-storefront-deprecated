angular.module('storefront.account')
.component('vcAccountProfileUpdate', {
    templateUrl: "themes/assets/js/account/account-profile-update.tpl.liquid",
    bindings: {
        customer: '<',
        loading: '<',
        onUpdate: '&'
    },
    controller: [function () {
        var ctrl = this;
        ctrl.data = {};

        ctrl.$onChanges = function (changesObj) {
            if (changesObj.customer && changesObj.customer.currentValue) {
                ctrl.data.changeData =
                    {
                        firstName: ctrl.customer.firstName,
                        lastName: ctrl.customer.lastName,
                        email: ctrl.customer.email
                    }
            }
        };

        ctrl.submit = function () {
            // no validation
            ctrl.onUpdate()(ctrl.data);
        };
    }]
});
