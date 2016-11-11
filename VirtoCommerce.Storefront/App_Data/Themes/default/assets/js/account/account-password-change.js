angular.module('storefront.account')
.component('vcAccountPasswordChange', {
    templateUrl: "themes/assets/js/account/account-password-change.tpl.liquid",
    bindings: {
        coupon: '=',
        onApplyCoupon: '&',
        onPasswordChange: '&'
    },
    controller: [function () {
        var ctrl = this;
        console.log(this);
    }]
});
