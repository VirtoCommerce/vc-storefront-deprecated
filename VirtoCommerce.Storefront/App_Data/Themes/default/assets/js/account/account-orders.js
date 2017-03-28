angular.module('storefront.account')
.component('vcAccountOrders', {
    templateUrl: "themes/assets/js/account/account-orders.tpl.liquid",
    $routeConfig: [
     { path: '/', name: 'OrderList', component: 'vcAccountOrdersList', useAsDefault: true },
     { path: '/:number', name: 'OrderDetail', component: 'vcAccountOrderDetail' }
    ],
    controller: ['orderHelper', function (orderHelper) {
        var $ctrl = this;
        $ctrl.orderHelper = orderHelper;
    }]
})

.component('vcAccountOrdersList', {
    templateUrl: "account-orders-list.tpl",
    controller: ['storefront.orderApi', 'loadingIndicatorService', function (orderApi, loader) {
        var ctrl = this;
        ctrl.loader = loader;
        ctrl.pageSettings = { currentPage: 1, itemsPerPageCount: 5, numPages: 10 };
        ctrl.pageSettings.pageChanged = function () {
            loader.wrapLoading(function () {
                return orderApi.search({
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
    require: {
        accountManager: '^vcAccountManager'
    },
    controller: ['storefront.orderApi', '$rootScope', '$window', 'loadingIndicatorService', 'confirmService', 'orderHelper', function (orderApi, $rootScope, $window, loader, confirmService, orderHelper) {
        var $ctrl = this;
        $ctrl.loader = loader;
        $ctrl.hasPhysicalProducts = true;

        function refresh() {
            loader.wrapLoading(function () {
                $ctrl.order = orderApi.get({ number: $ctrl.orderNumber }, function (result) {
                    $ctrl.isShowPayment = false;
                    var lastPayment = _.last(_.sortBy($ctrl.order.inPayments, 'createdDate'));
                    $ctrl.billingAddress = (lastPayment && lastPayment.billingAddress) ||
                            _.findWhere($ctrl.order.addresses, { type: 'billing' }) ||
                            _.first($ctrl.order.addresses);
                    $ctrl.amountToPay = orderHelper.getNewPayment($ctrl.order).sum.amount;

                    if ($ctrl.amountToPay > 0) {
                        $ctrl.billingAddressEqualsShipping = true;
                        loadPromise = orderApi.getNewPaymentData({ number: $ctrl.orderNumber }, function (result) {
                            //$ctrl.order = result.order;
                            configurePayment(result.paymentMethods, result.payment);
                        }).$promise;
                    }
                });
                return $ctrl.order.$promise;
            });
        }

        this.$routerOnActivate = function (next) {
            $ctrl.pageNumber = next.params.pageNumber || 1;
            $ctrl.orderNumber = next.params.number;

            refresh();
        };

        $ctrl.getInvoicePdf = function () {
            var url = $window.BASE_URL + 'storefrontapi/orders/' + $ctrl.orderNumber + '/invoice';
            $window.open(url, '_blank');
        }

        $ctrl.showPayment = function () {
            loadPromise.then(function (result) {
                $ctrl.isShowPayment = true;
            });
        };

        var loadPromise;
        $ctrl.getAvailPaymentMethods = function () {
            return loadPromise.then(function (result) {
                var preselectedMaymentMethod;
                if ($ctrl.payment.gatewayCode) {
                    preselectedMaymentMethod = _.findWhere(result.paymentMethods, { code: $ctrl.payment.gatewayCode });
                }

                return preselectedMaymentMethod ? [preselectedMaymentMethod] : result.paymentMethods;
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
                    $ctrl.payment.bankCardInfo = $ctrl.paymentMethod.card;
                    return orderApi.addOrUpdatePayment({ number: $ctrl.orderNumber }, $ctrl.payment, function (payment) {
                        orderApi.processPayment({ number: $ctrl.orderNumber, paymentNumber: payment.number }, $ctrl.paymentMethod.card, function (result) {
                            var orderProcessingResult = result.orderProcessingResult;
                            var paymentMethod = result.paymentMethod;

                            if (!orderProcessingResult.isSuccess) {
                                $rootScope.$broadcast('storefrontError', {
                                    type: 'error',
                                    title: ['Error in new payment processing: ', orderProcessingResult.error, 'New Payment status: ' + orderProcessingResult.newPaymentStatus].join(' '),
                                    message: orderProcessingResult.error,
                                });
                                return;
                            }

                            if (paymentMethod.paymentMethodType && paymentMethod.paymentMethodType.toLowerCase() === 'preparedform' && orderProcessingResult.htmlForm) {
                                outerRedirect($ctrl.accountManager.baseUrl + 'cart/checkout/paymentform?orderNumber=' + $ctrl.orderNumber);
                            } else if (paymentMethod.paymentMethodType && paymentMethod.paymentMethodType.toLowerCase() === 'redirection' && orderProcessingResult.redirectUrl) {
                                outerRedirect(orderProcessingResult.redirectUrl);
                            } else {
                                if ($ctrl.accountManager.customer.isRegisteredUser) {
                                    refresh();
                                } else {
                                    outerRedirect($ctrl.accountManager.baseUrl + 'cart/thanks/' + $ctrl.orderNumber);
                                }
                            }
                        })
                    }).$promise;
                });
            }
        };

        $ctrl.cancel = function () {
            confirmService.confirm('Cancel this payment?').then(function (confirmed) {
                if (confirmed) {
                    loader.wrapLoading(function () {
                        return orderApi.cancelPayment({ number: $ctrl.orderNumber, paymentNumber: $ctrl.payment.number }, null, refresh).$promise;
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

        function configurePayment(paymentMethods, newPaymentTemplate) {
            $ctrl.payment = orderHelper.getNewPayment($ctrl.order, paymentMethods, newPaymentTemplate);
            $ctrl.payment.purpose = $ctrl.payment.purpose || 'Repeated payment';
            $ctrl.amountToPay = $ctrl.payment.sum.amount;

            $ctrl.canCancelPayment = $ctrl.payment.id !== newPaymentTemplate.id;
            if ($ctrl.canCancelPayment) {
                $ctrl.selectPaymentMethod(_.findWhere(paymentMethods, { code: $ctrl.payment.gatewayCode }));
            }

            if (!_.some($ctrl.order.shipments)) {
                $ctrl.hasPhysicalProducts = false;
                $ctrl.billingAddressEqualsShipping = false;
            }
        }

        function outerRedirect(absUrl) {
            $window.location.href = absUrl;
        };
    }]
})

.factory('orderHelper', function () {
    var retVal = {
        getNewPayment: function (order, paymentMethods, newPaymentTemplate) {
            var retVal;
            var paidPayments = _.filter(order.inPayments, function (x) {
                return x.status === 'Paid';
            });
            var paidAmount = _.reduce(paidPayments, function (memo, num) { return memo + num.sum.amount; }, 0);
            var amountToPay = order.total.amount - paidAmount;

            var pendingPayments = _.filter(order.inPayments, function (x) {
                return !x.isCancelled &&
                        (x.status === 'New' || x.status === 'Pending') &&
                        x.sum.amount > 0; // && x.sum.amount === amountToPay;
            });
            var pendingPayment = _.last(_.sortBy(pendingPayments, 'createdDate'));
            if (pendingPayment && (!paymentMethods || _.findWhere(paymentMethods, { code: pendingPayment.gatewayCode }))) {
                retVal = pendingPayment;
            } else {
                newPaymentTemplate = newPaymentTemplate || { sum: {} };
                newPaymentTemplate.sum.amount = amountToPay;
                retVal = newPaymentTemplate;
            }

            return retVal;
        }
    };

    return retVal;
})

.filter('orderToSummarizedStatusLabel', ['orderHelper', function (orderHelper) {
    return function (order) {
        var retVal = order.status || 'New';

        var found = _.findWhere(orderHelper.statusLabels, { status: retVal.toLowerCase() });
        if (found) {
            retVal = found.label;
        }

        return retVal;
    };
}])
;
