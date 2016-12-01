angular.module('storefront.account')
.factory('storefront.accountApi', ['$resource', function ($resource) {
    return $resource('storefrontapi/account', null, {
        updateAccount: { url: 'storefrontapi/account', method: 'POST' },
        changePassword: { url: 'storefrontapi/account/password', method: 'POST' },
        getQuotes: { url: 'storefrontapi/account/quotes' },
        updateAddresses: { url: 'storefrontapi/account/addresses', method: 'POST' },
        getCountries: { url: 'storefrontapi/countries', isArray: true },
        getCountryRegions: { url: 'storefrontapi/countries/:code3/regions', isArray: true }
    });
}])
.factory('storefront.orderApi', ['$resource', function ($resource) {
    return $resource('storefrontapi/orders/:number', null, {
        getOrders: {},
        getNewPaymentData: { url: 'storefrontapi/orders/:number/newpaymentdata' },
        addOrUpdatePayment: { url: 'storefrontapi/orders/:number/payments', method: 'POST' },
    });
}]);