var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcCheckoutTotals', {
	templateUrl: "themes/assets/js/checkout/checkout-totals.tpl.liquid",
	bindings: {
		cart: '=',
		displayOnlyTotal: '<'
	},
	controller: [function () {
		var ctrl = this;
	
	}]
});
