var storefrontAppDependencies = [
'ui.bootstrap',
'ngStorage'
]
var storefrontApp = angular.module('storefrontApp', storefrontAppDependencies);

storefrontApp.factory('httpErrorInterceptor', ['$q', '$rootScope', function ($q, $rootScope) {
    var httpErrorInterceptor = {};

    httpErrorInterceptor.responseError = function (rejection) {
        $rootScope.$broadcast('storefrontError', {
            type: 'error',
            title: [rejection.config.method, rejection.config.url, rejection.status, rejection.statusText, rejection.data.message].join(' '),
            message: rejection.data.stackTrace,
        });
        return $q.reject(rejection);
    };
    httpErrorInterceptor.requestError = function (rejection) {
        $rootScope.$broadcast('storefrontError', {
            type: 'error',
            title: [rejection.config.method, rejection.config.url, rejection.status, rejection.statusText, rejection.data.message].join(' '),
            message: rejection.data.stackTrace,
        });
        return $q.reject(rejection);
    };

    return httpErrorInterceptor;
}])

storefrontApp.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('httpErrorInterceptor');

}]);