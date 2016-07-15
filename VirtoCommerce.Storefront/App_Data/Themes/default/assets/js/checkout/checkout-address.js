var storefrontApp = angular.module('storefrontApp');
storefrontApp.component('vcCheckoutAddress', {
	templateUrl: "themes/assets/js/checkout/checkout-address.tpl.html",
	bindings: {
		address: '=',
		addresses: '<',
		countries: '=',
		getCountryRegions: '&',
		editMode: '<',
		onUpdate: '&',
	},
	require: {
		checkoutStep: '^vcCheckoutWizardStep'
	},
	controller: ['$scope', function ($scope) {
		var ctrl = this;
		this.$onInit = function () {
			ctrl.checkoutStep.addComponent(this);
		};

		this.$onDestroy = function () {
			ctrl.checkoutStep.removeComponent(this);
		};

		function populateRegionalDataForAddress(address) {
			if (address) {
				//Set country object for address
				address.country = _.find(ctrl.countries, function (x) { return x.code3 == address.countryCode; });
				if (address.country != null) {
					ctrl.address.countryName = ctrl.address.country.name;
					ctrl.address.countryCode = ctrl.address.country.code3;
				}

				if (address.country) {
					if (address.country.regions) {
						setAddressRegion(address, address.country.regions);
					}
					else {
						ctrl.getCountryRegions({ country: address.country }).then(function (regions) {
							address.country.regions = regions;
							setAddressRegion(address, regions);
						});
					}
				}
			}
		};

		function setAddressRegion(address, regions)
		{
			address.region = _.find(regions, function (x) { return x.code == address.regionId; });
			if (address.region) {
				ctrl.address.regionId = ctrl.address.region.code;
				ctrl.address.regionName = ctrl.address.region.name;
			}
			else
			{
				ctrl.address.regionId = undefined;
				ctrl.address.regionName = undefined;
			}
		}

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
			if (ctrl.address) {
				populateRegionalDataForAddress(ctrl.address);				
				ctrl.address.name = stringifyAddress(ctrl.address);				
				ctrl.onUpdate({ address: ctrl.address });
			}
		}, true);

	}]
});
