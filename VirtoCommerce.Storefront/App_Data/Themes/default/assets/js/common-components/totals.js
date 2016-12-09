var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcTotals', {
    templateUrl: "themes/assets/js/common-components/totals.tpl.liquid",
	bindings: {
		order: '<'
	}
});
