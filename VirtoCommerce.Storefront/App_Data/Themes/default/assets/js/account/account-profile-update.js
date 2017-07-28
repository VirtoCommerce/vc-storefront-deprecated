angular.module('storefront.account')
.component('vcAccountProfileUpdate', {
    templateUrl: "themes/assets/js/account/account-profile-update.tpl.liquid",
    bindings: {
        $router: '<'
    },
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefrontApp.mainContext', '$scope', 'loadingIndicatorService', function (mainContext, $scope, loader) {
        var $ctrl = this;
        $ctrl.loader = loader;
        
        $scope.$watch(
            function () { return mainContext.customer; },
            function (customer) {
                $ctrl.customer = customer;
                if (customer) {
                    if (customer.isContract) {
                        $ctrl.$router.navigate(['Orders']);
                    }
                    $ctrl.changeData =
                    {
                        firstName: customer.firstName,
                        lastName: customer.lastName,
                        email: customer.email
                    };
                }
            });

        $ctrl.submit = function () {
            // no validation
            $ctrl.accountManager.updateProfile($ctrl.changeData);
        };
    }]
});
