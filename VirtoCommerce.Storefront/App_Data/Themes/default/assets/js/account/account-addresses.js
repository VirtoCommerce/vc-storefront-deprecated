angular.module('storefront.account')
.component('vcAccountAddresses', {
    templateUrl: "themes/assets/js/account/account-addresses.tpl.liquid",
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefrontApp.mainContext', 'confirmService', '$translate', '$scope', function (mainContext, confirmService, $translate, $scope) {
        var $ctrl = this;

        $scope.$watch(
          function () { return mainContext.customer.addresses; },
          function () {
              $ctrl.addresses = mainContext.customer.addresses;
          }
        );

        $ctrl.addNewAddress = function () {
            if (_.last(components).validate()) {
                $ctrl.addresses.push($ctrl.newAddress);
                $ctrl.accountManager.updateAddresses($ctrl.addresses).then(function () {
                    $ctrl.newAddress = null;
                });
            }
        };

        $ctrl.submit = function ($index, addrCopy) {
            if (components[$index].validate()) {
                angular.copy(addrCopy, $ctrl.addresses[$index]);
                $ctrl.accountManager.updateAddresses($ctrl.addresses);
            }
        };

        $ctrl.cancel = function ($index, addrCopy) {
            angular.copy($ctrl.addresses[$index], addrCopy);
        };

        $ctrl.clone = function (x) {
            return angular.copy(x);
        };

        $ctrl.delete = function ($index) {
            var showDialog = function (text) {
                confirmService.confirm(text).then(function (confirmed) {
                    if (confirmed) {
                        $ctrl.addresses.splice($index, 1);
                        $ctrl.accountManager.updateAddresses($ctrl.addresses);
                    }
                });
            };

            $translate('customer.addresses.delete_confirm').then(showDialog, showDialog);
        };

        var components = [];
        $ctrl.addComponent = function (component) {
            components.push(component);
        };
        $ctrl.removeComponent = function (component) {
            components = _.without(components, component);
        };
    }]
});
