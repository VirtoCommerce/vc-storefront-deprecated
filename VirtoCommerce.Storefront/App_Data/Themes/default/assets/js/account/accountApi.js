angular.module('storefront.account')
.factory('storefront.accountApi', ['$resource', function ($resource) {
    return $resource('storefrontapi/account', null, {
        updateAccount: { url: 'storefrontapi/account', method: 'POST' },
        changePassword: { url: 'storefrontapi/account/password', method: 'POST' },
        getOrders: { url: 'storefrontapi/account/orders' },
        getQuotes: { url: 'storefrontapi/account/quotes' },
        updateAddresses: { url: 'storefrontapi/account/addresses', method: 'POST' },
        getCountries: { url: 'storefrontapi/countries', isArray: true },
        getCountryRegions: { url: 'storefrontapi/countries/:code3/regions', isArray: true }
    });
}]);