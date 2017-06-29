var storefrontApp = angular.module('storefrontApp');

storefrontApp.directive('vcContentPlace', ['marketingService', function (marketingService) {
    return {
        restrict: 'E',
        link: function (scope, element, attrs) {
            marketingService.getDynamicContent(attrs.id).then(function (response) {
                element.html(response.data);
            });
        },
        replace: true
    }
}]);

storefrontApp.directive('vcFallbackImage', function () {
    return {
        link: function (scope, element, attrs) {
            element.on('error', function () {
                element.attr('src', attrs.fallbackSrc);
            });
        }
    };
});