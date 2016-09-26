var product = function () {
	this.buyButton = element(by.id("addToCart"));
	this.goToCartButton = element(by.buttonText("Go to cart"));
}

module.exports = product;