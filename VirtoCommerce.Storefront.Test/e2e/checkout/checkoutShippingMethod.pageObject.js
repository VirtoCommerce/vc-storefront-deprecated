require('../helpers/options.helpers.js');

var shippingMethod = function () {
	this.shippmentOptions = element.all(by.repeater('method in value'));
}

shippingMethod.prototype.setShippmentByIndex = function (index) {
	this.shippmentOptions.get(index).click();
}

module.exports = shippingMethod;