angular.module('storefront.account')
.component('vcAccountAddresses', {
    templateUrl: "themes/assets/js/account/account-addresses.tpl.liquid",
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefrontApp.mainContext', 'confirmService', '$translate', '$scope', 'loadingIndicatorService', function (mainContext, confirmService, $translate, $scope, loader) {
        var $ctrl = this;
        $ctrl.loader = loader;

        $scope.$watch(
          function () { return mainContext.customer.addresses; },
          function () {
              $ctrl.addresses = mainContext.customer.addresses;
          }
        );

        $ctrl.addNewAddress = function () {
            if (_.last(components).validate()) {
                $ctrl.addresses.push($ctrl.newAddress);
                $ctrl.newAddress = null;
                $ctrl.accountManager.updateAddresses($ctrl.addresses);
            }
        };

        $ctrl.submit = function () {
            if (components[$ctrl.editIdx].validate()) {
                angular.copy($ctrl.editItem, $ctrl.addresses[$ctrl.editIdx]);
                $ctrl.accountManager.updateAddresses($ctrl.addresses).then($ctrl.cancel);
            }
        };

        $ctrl.cancel = function () {
            $ctrl.editIdx = -1;
            $ctrl.editItem = null;
        };

        $ctrl.edit = function ($index) {
            $ctrl.editIdx = $index;
            $ctrl.editItem = angular.copy($ctrl.addresses[$ctrl.editIdx]);
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
