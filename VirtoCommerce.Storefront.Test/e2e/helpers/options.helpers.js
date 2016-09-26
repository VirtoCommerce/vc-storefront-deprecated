var ElementFinder = $('').constructor;
ElementFinder.prototype.setSelectedValue = function (value) {
	element.all(by.css('option[value="' + value + '"]')).click();
};
ElementFinder.prototype.getSelectedValue = function () {
	return this.$('option:checked').getAttribute('value');
}
ElementFinder.prototype.setIndex = function (index) {
	this.all(by.css('option')).get(index).click();
}

var ElementArrayFinder = $$('').constructor;
ElementArrayFinder.prototype.setSelectedValue = function (value) {
	this.filter(function (elem, index) {
		return elem.getAttribute('value').then(function (value) {
			return value === searchValue;
		});
	}).first().click();
};

ElementArrayFinder.prototype.setSelectedIndex = function (index) {
	this.get(index).click();
};

ElementArrayFinder.prototype.getSelectedValue = function (value) {
	return this.filter(function (elem, index) {
		return elem.getAttribute('checked').then(function (value) {
			return value === 'true';
		});
	}).first().getAttribute('value');
}