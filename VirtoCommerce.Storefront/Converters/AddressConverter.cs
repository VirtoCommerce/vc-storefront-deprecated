using System;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using coreModel = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using customerModel = VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi.Models;
using shopifyModel = VirtoCommerce.LiquidThemeEngine.Objects;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AddressConverter
    {
        public static Address ToWebModel(this shopifyModel.Address address, Country[] countries)
        {
            var result = new Address();
            result.CopyFrom(address, countries);
            return result;
        }

        public static customerModel.Address ToServiceModel(this Address address)
        {
            var retVal = new customerModel.Address();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(address);
            retVal.AddressType = address.Type.ToString();

            return retVal;
        }

        public static customerModel.Address ToCustomerModel(this OrderModule.Client.Model.Address orderAddress)
        {
            var customerAddress = new customerModel.Address();

            customerAddress.InjectFrom<NullableAndEnumValueInjecter>(orderAddress);
            customerAddress.AddressType = orderAddress.AddressType;
            customerAddress.Name = string.Join(" ", orderAddress.FirstName, orderAddress.LastName);

            return customerAddress;
        }

        public static Address CopyFrom(this Address result, shopifyModel.Address address, Country[] countries)
        {
            result.InjectFrom<NullableAndEnumValueInjecter>(address);

            result.Organization = address.Company;
            result.CountryName = address.Country;
            result.PostalCode = address.Zip;
            result.Line1 = address.Address1;
            result.Line2 = address.Address2;
            result.RegionName = address.Province;

            result.Name = result.ToString();

            var country = countries.FirstOrDefault(c => string.Equals(c.Name, address.Country, StringComparison.OrdinalIgnoreCase));
            if (country != null)
            {
                result.CountryCode = country.Code3;

                if (address.Province != null && country.Regions != null)
                {
                    var region = country.Regions.FirstOrDefault(r => string.Equals(r.Name, address.Province, StringComparison.OrdinalIgnoreCase));

                    if (region != null)
                    {
                        result.RegionId = region.Code;
                    }
                }
            }

            return result;
        }

        public static cartModel.Address ToCartServiceModel(this Address address)
        {
            var addressServiceModel = new cartModel.Address();

            addressServiceModel.InjectFrom(address);
            addressServiceModel.Type = address.Type.ToString();

            return addressServiceModel;
        }

        public static Address ToWebModel(this cartModel.Address address)
        {
            var addressWebModel = new Address();

            addressWebModel.InjectFrom(address);
            addressWebModel.Type = (AddressType)Enum.Parse(typeof(AddressType), address.Type, true);

            return addressWebModel;
        }

        public static Address ToWebModel(this OrderModule.Client.Model.Address address)
        {
            var result = new Address();

            result.InjectFrom(address);
            result.Type = EnumUtility.SafeParse(address.AddressType, AddressType.BillingAndShipping);

            return result;
        }

        public static Address ToWebModel(this QuoteModule.Client.Model.Address address)
        {
            var result = new Address();

            result.InjectFrom<NullableAndEnumValueInjecter>(address);
            result.Type = EnumUtility.SafeParse(address.AddressType, AddressType.BillingAndShipping);

            return result;
        }

        public static Address ToWebModel(this customerModel.Address serviceModel)
        {
            var result = new Address();

            result.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);
            result.Type = EnumUtility.SafeParse(serviceModel.AddressType, AddressType.BillingAndShipping);

            return result;
        }

        public static QuoteModule.Client.Model.Address ToQuoteServiceModel(this Address webModel)
        {
            var serviceModel = new QuoteModule.Client.Model.Address();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            serviceModel.AddressType = webModel.Type.ToString();

            return serviceModel;
        }

        public static coreModel.Address ToCoreServiceModel(this Address webModel)
        {
            coreModel.Address serviceModel = null;
            if (webModel != null)
            {
                serviceModel = new coreModel.Address();
                serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);
                serviceModel.AddressType = webModel.Type.ToString();
            }
            return serviceModel;
        }
    }
}
