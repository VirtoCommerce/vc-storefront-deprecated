exports.config = {
    seleniumAddress: 'http://localhost:4444/wd/hub',
    capabilities: {
        browserName: "chrome",
        shardTestFiles: true,
        maxInstances: 1,
        chromeOptions: {
        	args: ['--disable-extensions']
        }
    },

    allScriptsTimeout: 50000,
    getPageTimeout: 50000,

    framework: 'jasmine',

    jasmineNodeOpts: {
        defaultTimeoutInterval: 200000
    },

    baseUrl: 'http://localhost/storefront/Electronics/',

    onPrepare: function () {
        jasmine.DEFAULT_TIMEOUT_INTERVAL = 200000;
    },

    specs: ['home/home.spec.js'],

    params: {
        address: {
            email: 'test@test.com',
            firstName: 'John',
            lastName: 'Smith',
            organization: 'Comcast',
            addressLine1: '1819 Farnam St',
            addressLine2: '',
            city: 'Omaha',
            countryValue: 'United States',
            regionValue: 'Nebraska',
            postalCode: '68183',
            phone: '4024452300'
        }
    }
}