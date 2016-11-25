angular.module('storefront.account')
.component('vcAccountPasswordChange', {
    templateUrl: "themes/assets/js/account/account-password-change.tpl.liquid",
    bindings: {
        onPasswordChange: '&'
    },
    controller: [function () {
        var ctrl = this;
        ctrl.error = {};
        ctrl.passwordChange = { changeData: {} };

        ctrl.submit = function () {
            ctrl.passwordChange.errors = null;
            // validation
            var hasError = false;
            var errorMsg;

            errorMsg = ctrl.passwordChange.changeData.oldPassword === ctrl.passwordChange.changeData.newPassword;
            ctrl.error.newPassword = errorMsg
            hasError = hasError || errorMsg;

            if (!hasError) {
                errorMsg = ctrl.passwordChange.changeData.newPassword !== ctrl.passwordChange.changeData.newPassword2;
                ctrl.error.newPassword2 = errorMsg;
                hasError = hasError || errorMsg;
            }

            if (!hasError) {
                ctrl.passwordChange.changeData = {
                    oldPassword: ctrl.passwordChange.changeData.oldPassword,
                    newPassword: ctrl.passwordChange.changeData.newPassword
                };
                ctrl.onPasswordChange()(ctrl.passwordChange);
            }
        };
    }]
});
