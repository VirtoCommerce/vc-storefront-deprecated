angular.module('storefront.account')
.component('vcAccountOrders', {
    templateUrl: "themes/assets/js/account/account-orders.tpl.liquid",
    bindings: {
        loading: '<',
        getOrders: '&'
    },
    controller: [function () {
        var ctrl = this;
        ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        ctrl.pageSettings.pageChanged = function () {
            ctrl.getOrders()(ctrl.pageSettings.currentPage, ctrl.pageSettings.itemsPerPageCount, ctrl.sortInfos, function (data) {
                ctrl.entries = data.results;
                ctrl.pageSettings.totalItems = data.totalCount;
            });
        };
        ctrl.pageSettings.pageChanged();
    }]
})
.filter('orderToFinancialStatusLabel', function () {
    return function (order, trueValue, falseValue) {
        var retVal;
        if (_.any(order.inPayments)) {
            var inPayment = _.last(_.sortBy(order.inPayments, 'createdDate'));
            if (inPayment.status) {
                retVal = inPayment.status;
            }
            else {
                retVal = inPayment.isApproved ? "Paid" : "Pending";
            }
        }
        return retVal;
    };
})
.filter('orderToFulfillmentStatusLabel', function () {
    return function (order, trueValue, falseValue) {
        var retVal;

        var orderShipment = _.first(order.shipments);
        if (orderShipment) {
            if (orderShipment.status) {
                retVal = orderShipment.status;
            }
            else {
                retVal = orderShipment.isApproved ? "Sent" : "Not sent";
            }
        }
        return retVal;
    };
});
