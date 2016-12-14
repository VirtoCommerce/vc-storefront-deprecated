angular.module('storefront.account')
.component('vcAccountPasswordChange', {
    templateUrl: "themes/assets/js/account/account-password-change.tpl.liquid",
    bindings: {
        loading: '<',
        onPasswordChange: '&'
    },
    controller: [function () {
        var ctrl = this;
        ctrl.passwordChangeData = {};

        ctrl.submit = function () {
            // validation
            ctrl.errors = null;
            ctrl.error = {};
            var hasError = false;
            var errorMsg;

            errorMsg = ctrl.passwordChangeData.oldPassword === ctrl.passwordChangeData.newPassword;
            ctrl.error.newPassword = errorMsg
            hasError = hasError || errorMsg;

            if (!hasError) {
                errorMsg = ctrl.passwordChangeData.newPassword !== ctrl.passwordChangeData.newPassword2;
                ctrl.error.newPassword2 = errorMsg;
                hasError = hasError || errorMsg;
            }

            if (!hasError) {
                ctrl.onPasswordChange()(ctrl.passwordChangeData).then(function (result) {
                    angular.extend(ctrl, result);
                    ctrl.passwordChangeData = {};
                    ctrl.form.$setPristine();
                });
            }
        };

        ctrl.setForm = function (frm) { ctrl.form = frm; };
    }]
});
