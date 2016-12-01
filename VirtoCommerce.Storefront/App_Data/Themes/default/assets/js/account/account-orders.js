angular.module('storefront.account')
.component('vcAccountOrders', {
    templateUrl: "themes/assets/js/account/account-orders.tpl.liquid",
    bindings: {
        baseUrl: '<',
        customer: '<',
        countries: '<',
        getCountryRegions: '&'
    },
    $routeConfig: [
     { path: '/', name: 'OrderList', component: 'vcAccountOrdersList', useAsDefault: true },
     { path: '/:number', name: 'OrderDetail', component: 'vcAccountOrderDetail' },
     { path: '/:number/pay', name: 'PayOrder', component: 'vcAccountOrderPay' },
    ]
})

.component('vcAccountOrdersList', {
    templateUrl: "account-orders-list.tpl",
    controller: ['storefront.orderApi', 'loadingIndicatorService', function (orderApi, loader) {
        var ctrl = this;
        ctrl.loader = loader;
        ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        ctrl.pageSettings.pageChanged = function () {
            loader.wrapLoading(function () {
                return orderApi.getOrders({
                    pageNumber: ctrl.pageSettings.currentPage,
                    pageSize: ctrl.pageSettings.itemsPerPageCount,
                    sortInfos: ctrl.sortInfos
                }, function (data) {
                    ctrl.entries = data.results;
                    ctrl.pageSettings.totalItems = data.totalCount;
                }).$promise;
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
    controller: ['storefront.orderApi', 'loadingIndicatorService', function (orderApi, loader) {
        var $ctrl = this;

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber;
            loader.wrapLoading(function () {
                $ctrl.order = orderApi.get({ number: next.params.number }, function (result) {
                    $ctrl.billingAddress = _.findWhere($ctrl.order.addresses, { type: 'billing' }) || _.first($ctrl.order.addresses);
                });
                return $ctrl.order.$promise;
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
    require: {
        parentComponent: '^vcAccountOrders',
    },
    controller: ['storefront.orderApi', '$rootScope', '$window', 'loadingIndicatorService', function (orderApi, $rootScope, $window, loader) {
        var $ctrl = this;
        $ctrl.hasPhysicalProducts = true;
        $ctrl.billingAddressEqualsShipping = true;

        var loadPromise;
        $ctrl.getAvailPaymentMethods = function () {
            return loadPromise.then(function (result) {
                return result.paymentMethods;
            });
        };

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber;
            $ctrl.orderNumber = next.params.number;

            loader.wrapLoading(function () {
                return loadPromise = orderApi.getNewPaymentData({ number: $ctrl.orderNumber }, function (result) {
                    $ctrl.order = result.order;
                    $ctrl.payment = result.payment;
                    $ctrl.payment.sum = $ctrl.order.total;
                    $ctrl.payment.purpose = 'Repeated payment';
                }).$promise;
            });
        };

        $ctrl.selectPaymentMethod = function (paymentMethod) {
            $ctrl.payment.gatewayCode = paymentMethod.code;
            $ctrl.validate();
        };

        $ctrl.validate = function () {
            $ctrl.isValid = $ctrl.payment && $ctrl.payment.gatewayCode && $ctrl.payment.sum && $ctrl.payment.sum.amount > 0;
            if ($ctrl.isValid && !$ctrl.billingAddressEqualsShipping) {
                $ctrl.isValid = angular.isObject($ctrl.payment.billingAddress);
            }
            return $ctrl.isValid;
        };

        $ctrl.submit = function () {
            if ($ctrl.validate()) {
                loader.wrapLoading(function () {
                    //return orderApi.addOrUpdatePayment({ number: $ctrl.orderNumber }, { payment: $ctrl.payment, bankCardInfo: {} }, function (result) {
                    return orderApi.addOrUpdatePayment({ number: $ctrl.orderNumber }, $ctrl.payment, function (result) {
                        var orderProcessingResult = result.orderProcessingResult;
                        var paymentMethod = result.paymentMethod;

                        if (!orderProcessingResult.isSuccess) {
                            $ctrl.loading = false;
                            $rootScope.$broadcast('storefrontError', {
                                type: 'error',
                                title: ['Error in new payment processing: ', orderProcessingResult.error, 'New Payment status: ' + orderProcessingResult.newPaymentStatus].join(' '),
                                message: orderProcessingResult.error,
                            });
                            return;
                        }

                        if (paymentMethod.paymentMethodType && paymentMethod.paymentMethodType.toLowerCase() === 'preparedform' && orderProcessingResult.htmlForm) {
                            outerRedirect($ctrl.parentComponent.baseUrl + 'cart/checkout/paymentform?orderNumber=' + $ctrl.orderNumber);
                        } else if (paymentMethod.paymentMethodType && paymentMethod.paymentMethodType.toLowerCase() === 'redirection' && orderProcessingResult.redirectUrl) {
                            outerRedirect(orderProcessingResult.redirectUrl);
                        } else {
                            if ($ctrl.parentComponent.customer.isRegisteredUser) {
                                $ctrl.$router.navigate(['OrderDetail', { number: $ctrl.orderNumber, pageNumber: $ctrl.pageNumber }]);
                            } else {
                                outerRedirect($ctrl.parentComponent.baseUrl + 'cart/thanks/' + $ctrl.orderNumber);
                            }
                        }
                    }).$promise;
                });
            }
        };

        function outerRedirect(absUrl) {
            $window.location.href = absUrl;
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
