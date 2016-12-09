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
    //],
    //controller: ['storefront.orderApi', 'loadingIndicatorService', function (orderApi, loader) {
    //    var ctrl = this;
        
    //    this.$routerOnActivate = function (next) {
    //        var a = next.params.pageNumber || ctrl.pageSettings.currentPage;
    //    };
    //}
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
            $ctrl.pageNumber = next.params.pageNumber || 1;
            $ctrl.orderNumber = next.params.number;

            loader.wrapLoading(function () {
                $ctrl.order = orderApi.get({ number: $ctrl.orderNumber }, function (result) {
                    $ctrl.billingAddress = _.findWhere($ctrl.order.addresses, { type: 'billing' }) || _.first($ctrl.order.addresses);
                    calculateBalance();


                });
                return $ctrl.order.$promise;
            });
        };

        function calculateBalance() {
            var paidPayments = _.filter($ctrl.order.inPayments, function (x) {
                return x.status === 'Paid';
            });
            var paidAmount = _.reduce(paidPayments, function (memo, num) { return memo + num.sum.amount; }, 0);
            $ctrl.amountToPay = $ctrl.order.total.amount - paidAmount
            $ctrl.canAddPayment = $ctrl.amountToPay > 0;
        }

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
    controller: ['storefront.orderApi', '$rootScope', '$window', 'loadingIndicatorService', 'confirmService', function (orderApi, $rootScope, $window, loader, confirmService) {
        var $ctrl = this;
        $ctrl.hasPhysicalProducts = true;
        $ctrl.billingAddressEqualsShipping = true;

        var loadPromise;
        $ctrl.getAvailPaymentMethods = function () {
            return loadPromise.then(function (result) {
                var preselectedMaymentMethod;
                if ($ctrl.payment.gatewayCode) {
                    preselectedMaymentMethod = _.find(result.paymentMethods, { code: $ctrl.payment.gatewayCode });
                }

                return preselectedMaymentMethod ? [preselectedMaymentMethod] : result.paymentMethods;
            });
        };

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber || 1;
            $ctrl.orderNumber = next.params.number;

            loader.wrapLoading(function () {
                return loadPromise = orderApi.getNewPaymentData({ number: $ctrl.orderNumber }, function (result) {
                    $ctrl.order = result.order;
                    configurePayment(result.paymentMethods, result.payment);
                }).$promise;
            });
        };

        $ctrl.selectPaymentMethod = function (paymentMethod) {
            angular.extend($ctrl.payment, paymentMethod);
            $ctrl.payment.gatewayCode = paymentMethod.code;
            // $ctrl.payment.sum = angular.copy($ctrl.order.total);
            // $ctrl.payment.sum.amount += paymentMethod.totalWithTax.amount;

            $ctrl.validate();
        };

        $ctrl.validate = function () {
            $ctrl.isValid = $ctrl.payment &&
                $ctrl.payment.gatewayCode &&
                $ctrl.payment.sum && $ctrl.payment.sum.amount > 0 &&
                _.every(components, function (x) {
                    return typeof x.validate !== "function" || x.validate();
                });

            return $ctrl.isValid;
        };

        $ctrl.submit = function () {
            if ($ctrl.validate()) {
                loader.wrapLoading(function () {
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

        $ctrl.cancel = function () {
            confirmService.confirm('Cancel this payment?').then(function (confirmed) {
                if (confirmed) {
                    loader.wrapLoading(function () {
                        $ctrl.payment.cancelReason = "Cancelled by customer";
                        $ctrl.payment.cancelledDate = new Date();
                        $ctrl.payment.isCancelled = true;
                        $ctrl.payment.status = 'Cancelled';
                        $ctrl.payment.paymentStatus = 'Cancelled';

                        return orderApi.addOrUpdatePayment({ number: $ctrl.orderNumber }, $ctrl.payment, function (result) {
                            $ctrl.$router.navigate(['OrderDetail', { number: $ctrl.orderNumber, pageNumber: $ctrl.pageNumber }]);
                        }).$promise;
                    });
                }
            });
        };

        var components = [];
        $ctrl.addComponent = function (component) {
            components.push(component);
        };
        $ctrl.removeComponent = function (component) {
            components = _.without(components, component);
        };

        function configurePayment(paymentMethods, newPayment) {
            var paidPayments = _.filter($ctrl.order.inPayments, function (x) {
                return x.status === 'Paid';
            });
            var paidAmount = _.reduce(paidPayments, function (memo, num) { return memo + num.sum.amount; }, 0);
            var amountToPay = $ctrl.order.total.amount - paidAmount;

            var pendingPayments = _.filter($ctrl.order.inPayments, function (x) {
                return (x.status === 'New' || x.status === 'Pending') && x.sum.amount > 0; // && x.sum.amount === amountToPay;
            });
            var pendingPayment = _.last(_.sortBy(pendingPayments, 'createdDate'));
            if (pendingPayment) {
                var found = _.find(paymentMethods, { code: pendingPayment.gatewayCode });
                if (found) {
                    $ctrl.payment = pendingPayment;
                    $ctrl.canCancelPayment = true;
                    $ctrl.selectPaymentMethod(found);
                }
            }
            if (!$ctrl.payment) {
                $ctrl.payment = newPayment;
                $ctrl.payment.sum.amount = amountToPay;
            }

            $ctrl.payment.purpose = $ctrl.payment.purpose || 'Repeated payment';
        }

        function outerRedirect(absUrl) {
            $window.location.href = absUrl;
        };
    }]
})

.filter('orderToSummarizedStatusLabel', function () {
    return function (order) {
        var retVal = order.status;
        // TODO: add status 'state machine'
        
        return retVal;
    };
})

.filter('orderToFinancialStatusLabel', function () {
    return function (order) {
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
    return function (order) {
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
