var storefrontApp = angular.module('storefrontApp');

storefrontApp.component('vcCheckoutLineItems', {
	templateUrl: "themes/assets/js/checkout/checkout-lineItems.tpl.liquid",
	bindings: {
		items: '=',
	},
	controller: [function () {
		var ctrl = this;	
	}]
});
