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
        search: { url: 'storefrontapi/orders/search', method: 'POST' },
        getNewPaymentData: { url: 'storefrontapi/orders/:number/newpaymentdata' },
        addOrUpdatePayment: { url: 'storefrontapi/orders/:number/payments', method: 'POST' },
        processPayment: { url: 'storefrontapi/orders/:number/payments/:paymentNumber/process', method: 'POST' },
        cancelPayment: { url: 'storefrontapi/orders/:number/payments/:paymentNumber/cancel', method: 'POST' }
    });
}])
.factory('storefront.subscriptionApi', ['$resource', function ($resource) {
    return $resource('storefrontapi/subscriptions/:number', null, {
        search: { url: 'storefrontapi/subscriptions/search', method: 'POST' },
        cancel: { url: 'storefrontapi/subscriptions/:number/cancel', method: 'POST' }
    });
}]);