var storefrontApp = angular.module('storefrontApp');

storefrontApp.filter('imgurl', function () {
    return function (input, type) {
        if (!type)
            return input;

        var extention = '.' + input.split('.').pop();
        var suffix = "_" + type;
        var result = input.replace(extention, suffix+extention);
        return result;
    };
});