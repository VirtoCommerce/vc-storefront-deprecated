angular.module('storefront.account')
.component('vcAccountOrders', {
    templateUrl: "themes/assets/js/account/account-orders.tpl.liquid",
    bindings: {
        loading: '<'
    },
    $routeConfig: [
     { path: '/', name: 'OrderList', component: 'vcAccountOrdersList', useAsDefault: true },
     { path: '/:number', name: 'OrderDetail', component: 'vcAccountOrderDetail' },
     { path: '/:number/pay', name: 'PayOrder', component: 'vcAccountOrderPay' },
    ]
})

.component('vcAccountOrdersList', {
    templateUrl: "account-orders-list.tpl",
    bindings: {
        loading: '<'
    },
    controller: ['storefront.orderApi', function (orderApi) {
        var ctrl = this;
        ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        ctrl.pageSettings.pageChanged = function () {
            orderApi.getOrders({
                pageNumber: ctrl.pageSettings.currentPage,
                pageSize: ctrl.pageSettings.itemsPerPageCount,
                sortInfos: ctrl.sortInfos
            }, function (data) {
                ctrl.entries = data.results;
                ctrl.pageSettings.totalItems = data.totalCount;
            });
        };

        this.$routerOnActivate = function (next) {
            ctrl.pageSettings.currentPage = next.params.pageNumber || ctrl.pageSettings.currentPage;
            ctrl.pageSettings.pageChanged();
        };
    }]
})

.component('vcAccountOrderDetail', {
    templateUrl: "account-order-detail.tpl",
    bindings: { $router: '<' },
    controller: ['storefront.orderApi', function (orderApi) {
        var $ctrl = this;

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber;
            $ctrl.order = orderApi.get({ number: next.params.number }, function (result) {
                $ctrl.billingAddress = _.findWhere($ctrl.order.addresses, { type: 'billing' }) || _.first($ctrl.order.addresses);
            });
        };

        //this.gotoPayment = function () {
        //    this.$router.navigate(['PayOrder', { number: $ctrl.order.number, order: { asd: $ctrl.order }, pageNumber: $ctrl.pageNumber }]);
        //};
    }]
})

.component('vcAccountOrderPay', {
    templateUrl: "account-order-pay.tpl",
    bindings: { $router: '<' },
    controller: ['storefront.orderApi', function (orderApi) {
        var $ctrl = this;
        $ctrl.payment = {};
        $ctrl.hasPhysicalProducts = true;
        $ctrl.billingAddressEqualsShipping = true;

        $ctrl.getAvailPaymentMethods = function () {
            return orderApi.getAvailablePaymentMethods({ number: $ctrl.orderNumber }).$promise;
        };

        $ctrl.selectPaymentMethod = function (paymentMethod) {
            $ctrl.payment.gatewayCode = paymentMethod.code;
            $ctrl.validate();
        };

        $ctrl.validate = function () {
            $ctrl.isValid = $ctrl.payment.gatewayCode && $ctrl.payment.sum && $ctrl.payment.sum.amount > 0;
            if ($ctrl.isValid && !$ctrl.billingAddressEqualsShipping) {
                $ctrl.isValid = angular.isObject($ctrl.payment.billingAddress);
            }
            return $ctrl.isValid;
        };

        $ctrl.submit = function () {
            if ($ctrl.validate()) {
                orderApi.addOrUpdatePayment({ number: $ctrl.orderNumber }, $ctrl.payment, function () {
                    // todo
                });
            }
        };

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber;
            $ctrl.orderNumber = next.params.number;
            $ctrl.order = orderApi.get({ number: next.params.number }, function (result) {
                //$ctrl.payment.amount = result.total.amount;
                $ctrl.payment.sum = result.total;
                $ctrl.payment.currency = result.currency;
                $ctrl.payment.purpose = 'Repeated payment';
            });
        };
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
