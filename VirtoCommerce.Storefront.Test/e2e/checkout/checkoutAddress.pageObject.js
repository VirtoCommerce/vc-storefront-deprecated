require('../helpers/options.helpers');

var checkoutAddress = function () {
	this.email = element(by.id('Email'));
	this.firstName = element(by.id('FirstName'));
	this.lastName = element(by.id('LastName'));
	this.organization = element(by.id('Organization'));
	this.address = element(by.id('Line1'));
	this.app = element(by.id('Line2'));
	this.city = element(by.id('City'));
	this.countrySelect = element(by.id('Country'));
	this.stateSelect = element(by.id('Region'));
	this.postalCode = element(by.id('PostalCode'));
	this.phone = element(by.id('Phone'));

	this.shippmentErrorBlock = element(by.css('.shipment_error'));
	this.nextButton = element(by.id('nextButton'));
}

checkoutAddress.prototype.continue = function () {
	var button = this.continueButton;
	button.isPresent().then(function(present) {
		if (present)
			button.click();
	});
}

checkoutAddress.prototype.fillAddressForm = function (address) {
	this.email.sendKeys(address.email);
	this.firstName.sendKeys(address.firstName);
	this.lastName.sendKeys(address.lastName);
	this.organization.sendKeys(address.organization);
	this.address.sendKeys(address.addressLine1);
	this.app.sendKeys(address.addressLine2);
	this.city.sendKeys(address.city);
	this.countrySelect.setIndex(1);
	this.stateSelect.setIndex(1);
	this.postalCode.sendKeys(address.postalCode);
	this.phone.sendKeys(address.phone);
}

checkoutAddress.prototype.setState = function (value) {
	this.stateSelect.setSelectedValue(value);
}

checkoutAddress.prototype.getState = function () {
	return this.stateSelect.getSelectedValue();
}

checkoutAddress.prototype.getCountry = function () {
	return this.countrySelect.getSelectedValue();
}

module.exports = checkoutAddress;