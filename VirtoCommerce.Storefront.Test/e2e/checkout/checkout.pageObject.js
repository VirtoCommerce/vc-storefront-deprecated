var CheckoutAddress = require('../checkout/checkoutAddress.pageObject.js');
var ShippingMethod = require('../checkout/checkoutShippingMethod.pageObject.js');
var PaymentMethod = require('../checkout/checkoutPaymentMethod.pageObject.js');

var checkout = function () {
	this.checkoutAddress = new CheckoutAddress();
	this.shippingMethod = new ShippingMethod();
	this.paymentMethod = new PaymentMethod();

	this.nextButton = element(by.css('button.step__footer__continue-btn.btn'));
}

module.exports = checkout;