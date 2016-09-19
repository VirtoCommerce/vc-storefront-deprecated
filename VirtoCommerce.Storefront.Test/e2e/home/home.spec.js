var Home = require('./home.pageObject.js');
var Collections = require('../collections/collections.pageObject.js');
var Product = require('../product/product.pageObject.js');
var Cart = require('../cart/cart.pageObject.js');
var Checkout = require('../checkout/checkout.pageObject.js');
var Thanks = require('../thanks/thanks.pageObject.js');

var self = this;

self.home = new Home();

describe('Home', function () {

    beforeAll(function () {
        browser.get(browser.baseUrl);
    });

    it('should create order', function () {

    	browser.get(browser.baseUrl + 'en-US/collections');

    	var collections = new Collections();
    	collections.firstProduct.click();

    	var product = new Product();
    	product.buyButton.click();
    	product.goToCartButton.click();

    	var cart = new Cart();
    	cart.checkoutButton.click();

    	var checkout = new Checkout();

    	checkout.checkoutAddress.fillAddressForm(browser.params.address);
    	checkout.nextButton.click();

    	checkout.shippingMethod.setShippmentByIndex(0);
    	checkout.nextButton.click();

    	checkout.nextButton.click();

    	var thanks = new Thanks();

    	expect(thanks.orderNumber.isPresent()).toBeTruthy();
    	expect(thanks.orderNumber.getText()).not.toBe('');

    	//browser.pause();
    	
    });
})