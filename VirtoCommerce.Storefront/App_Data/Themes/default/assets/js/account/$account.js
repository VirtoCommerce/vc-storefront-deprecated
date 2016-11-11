//Call this to register our module to main application
var moduleName = "storefront.account";

if (storefrontAppDependencies != undefined) {
    storefrontAppDependencies.push(moduleName);
}
angular.module(moduleName, [])
.controller('accountController', ['$rootScope', '$scope', '$window', 'cartService',
    function ($rootScope, $scope, $window, cartService) {
        $scope.account = {
            changePassword: function (data) {
                console.log(data);
            }
        };
        //$scope.account = {
        //    wizard: {},
        //    paymentMethod: {},
        //    shipment: {},
        //    payment: {},
        //    coupon: {},
        //    availCountries: [],
        //    loading: false,
        //    isValid: false
        //};


    }]);
