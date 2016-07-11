var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcCheckoutAddress', {
	templateUrl: "themes/assets/js/checkout/checkout-address.tpl.html",
	bindings: {
		address: '=',
		addresses: '<',
		editMode: '<',
		onUpdate: '&'
	},
	require: {
		checkoutStep: '^vcCheckoutStep'
	},

	controller: ['$scope', 'cartService', function ($scope, cartService) {
		var ctrl = this;
		this.$onInit = function () {
			ctrl.checkoutStep.addComponent(this);
			//Load all countries 
			cartService.getCountries().then(function (result) {
				ctrl.countries = result.data;
				if (ctrl.addresses) {
					prepareAddresses(ctrl.addresses);
				}
			});
		};
		this.$onDestroy = function () {
			ctrl.checkoutStep.removeComponent(this);
		};
		this.$onChanges = function (onChanges) {
			//Load country and regions for all resent addresses
			if (onChanges.addresses && onChanges.addresses.currentValue) {
				prepareAddresses(ctrl.addresses);
			}
		};

		function prepareAddresses(addresses) {
			if (ctrl.countries) {
				//Set country object for each recent address
				_.each(addresses, function (address) {
					address.country = _.find(ctrl.countries, function (x) { return x.name == address.countryName || x.code2 == address.countryCode || x.code3 == address.countryCode; });
				});
				//Get distinct countries for recent addresses
				var countries = _.map(_.groupBy(addresses, function (x) { return x.country.code3; }), function (x) { return x[0].country });
				//load regions for recent addresses countries
				_.each(countries, function (country) {
					loadCountryRegions(country).then(function (regions) {
						//Set region for each resent address
						_.each(_.filter(addresses, function (x) { return x.country == country; }), function (address) {
							address.region = _.find(regions, function (x) { return x.code == address.regionId || x.name == address.regionName; });
						});
					});
				});
			}
		}

		function loadCountryRegions(country) {
			if (country && !country.regions) {
				return cartService.getCountryRegions(country.code3).then(function (result) {
					country.regions = result.data;
					return country.regions;
				});
			}
		}

		ctrl.selectCountry = function (country) {		
			loadCountryRegions(country);
		};	

		ctrl.validate = function () {
			if (ctrl.form) {
				ctrl.form.$setSubmitted();
				return !ctrl.form.$invalid;
			}
			return true;
		}

		$scope.$watch('$ctrl.address', function () {
			if (ctrl.form && ctrl.address) {
				if (ctrl.address.country) {
					ctrl.address.countryName = ctrl.address.country.name;
					ctrl.address.countryCode = ctrl.address.country.code3;
				}
				else {
					ctrl.address.countryName = undefined;
					ctrl.address.countryCode = undefined;
				}
				if (ctrl.address.region) {
					ctrl.address.regionId = ctrl.address.region.code;
					ctrl.address.regionName = ctrl.address.region.name;
				}
				else {
					ctrl.address.regionId = undefined;
					ctrl.address.regionName = undefined;
				}
		
				ctrl.onUpdate({ address: ctrl.address });
			}
		}, true);

	}]
});
