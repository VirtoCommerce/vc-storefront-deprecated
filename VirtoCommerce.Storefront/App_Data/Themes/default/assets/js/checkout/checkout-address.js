var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcCheckoutAddress', {
	templateUrl: "themes/assets/js/checkout/checkout-address.tpl.html",
	bindings: {
		address: '=',
		addresses: '<',
		getAvailCountries: '&',
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
			if (ctrl.editMode) {
				ctrl.getAvailCountries().then(function (countries) {
					ctrl.countries = countries;
				});
			}
		};

		this.$onDestroy = function () {
			ctrl.checkoutStep.removeComponent(this);
		};
		
		function prepareAddress(address, countries) {
			//Set country object for address
			address.country = _.find(countries, function (x) { return x.name == address.countryName || x.code2 == address.countryCode || x.code3 == address.countryCode; });

			if (ctrl.address.country) {
				ctrl.address.countryName = ctrl.address.country.name;
				ctrl.address.countryCode = ctrl.address.country.code3;
			}
			else {
				ctrl.address.countryName = undefined;
				ctrl.address.countryCode = undefined;
			}

			if (address.country) {
				loadCountryRegions(address.country).then(function (regions) {
					address.region = _.find(regions, function (x) { return x.code == address.regionId || x.name == address.regionName; });
					if (ctrl.address.region) {
						ctrl.address.regionId = ctrl.address.region.code;
						ctrl.address.regionName = ctrl.address.region.name;
					}
					else {
						ctrl.address.regionId = undefined;
						ctrl.address.regionName = undefined;
					}
				});
			}		
		}

		function loadCountryRegions(country) {
			return cartService.getCountryRegions(country.code3).then(function (result) {
				country.regions = result.data;
				return country.regions;
			});
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

		function stringifyAddress(address) {
			var stringifiedAddress = address.firstName + ' ' + address.lastName + ', ';
			stringifiedAddress += address.organization ? address.organization + ', ' : '';
			stringifiedAddress += address.countryName + ', ';
			stringifiedAddress += address.regionName ? address.regionName + ', ' : '';
			stringifiedAddress += address.city;
			stringifiedAddress += address.line1 + ', '
			stringifiedAddress += address.line2 ? address.line2 : '';
			stringifiedAddress += address.postalCode;
			return stringifiedAddress;
		}

		$scope.$watch('$ctrl.address', function () {
			if (ctrl.form && ctrl.address) {
				if (!ctrl.address.country)
				{
					if (!ctrl.countries) {
						ctrl.getAvailCountries().then(function (countries) {
							prepareAddress(ctrl.address, countries);
						});
					}
					else
					{
						prepareAddress(ctrl.address, ctrl.countries);
					}
				}
				ctrl.address.name = stringifyAddress(ctrl.address);
		
				ctrl.onUpdate({ address: ctrl.address });
			}
		}, true);

	}]
});
