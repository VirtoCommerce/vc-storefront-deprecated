var storefrontAppDependencies = [
    'ui.bootstrap',
    'ngStorage',
    'pascalprecht.translate',
    'ngSanitize',
]
var storefrontApp = angular.module('storefrontApp', storefrontAppDependencies);

storefrontApp.factory('httpErrorInterceptor', ['$q', '$rootScope', function ($q, $rootScope) {
    var httpErrorInterceptor = {};

    httpErrorInterceptor.responseError = function (rejection) {
        if (rejection.data && rejection.data.message) {
            $rootScope.$broadcast('storefrontError', {
                type: 'error',
                title: [rejection.config.method, rejection.config.url, rejection.status, rejection.statusText, rejection.data.message].join(' '),
                message: rejection.data.stackTrace,
            });
        }
        return $q.reject(rejection);
    };
    httpErrorInterceptor.requestError = function (rejection) {
        if (rejection.data && rejection.data.message) {
            $rootScope.$broadcast('storefrontError', {
                type: 'error',
                title: [rejection.config.method, rejection.config.url, rejection.status, rejection.statusText, rejection.data.message].join(' '),
                message: rejection.data.stackTrace,
            });
        }
        return $q.reject(rejection);
    };

    return httpErrorInterceptor;
}])

storefrontApp.config(['$httpProvider', '$translateProvider', function ($httpProvider, $translateProvider) {
    $httpProvider.interceptors.push('httpErrorInterceptor');

    $translateProvider.useSanitizeValueStrategy('sanitizeParameters');
    $translateProvider.useUrlLoader(BASE_URL + 'themes/localization.json');
    $translateProvider.preferredLanguage('en');

}]);