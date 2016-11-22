angular.module('storefront.account')
.component('vcAddressesCrud', {
    templateUrl: "themes/assets/js/account/addresses-crud.tpl.liquid",
    bindings: {
        addresses: '<',
        countries: '<',
        getCountryRegions: '&',
        onUpdate: '&'
    },
    controller: [function () {
        var ctrl = this;
        
        ctrl.addNew = function () {
            if (_.last(components).validate()) {
                ctrl.addresses.push(ctrl.newAddress);
                ctrl.onUpdate()(ctrl.addresses).then(function () {
                    ctrl.newAddress = null;
                });
            }
        };

        ctrl.submit = function ($index, addrCopy) {
            if (components[$index].validate()) {
                angular.copy(addrCopy, ctrl.addresses[$index]);
                ctrl.onUpdate()(ctrl.addresses);
            }
        };
        
        ctrl.cancel = function ($index, addrCopy) {
            angular.copy(ctrl.addresses[$index], addrCopy);
        };

        ctrl.clone = function (x) {
            return angular.copy(x);
        };
        
        ctrl.delete = function ($index) {
            ctrl.addresses.splice($index, 1);
            ctrl.onUpdate()(ctrl.addresses);
        };
        
        var components = [];
        ctrl.addComponent = function (component) {
            components.push(component);
        };
        ctrl.removeComponent = function (component) {
            components = _.without(components, component);
        };
    }]
});
