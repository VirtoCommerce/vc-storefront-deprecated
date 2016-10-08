using System;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using shopifyModel = VirtoCommerce.LiquidThemeEngine.Objects;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AddressConverter
    {
        public static Address ToAddress(this coreDto.Address addressDto)
        {
            var retVal = new Address();

            retVal.InjectFrom(addressDto);
            retVal.Type = (AddressType)Enum.Parse(typeof(AddressType), addressDto.AddressType, true);
            return retVal;
        }

        public static coreDto.Address ToCoreAddressDto(this Address address)
        {
            var retVal = new coreDto.Address();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(address);
            retVal.AddressType = address.Type.ToString();

            return retVal;
        }


        public static Address ToWebModel(this shopifyModel.Address address, Country[] countries)
        {
            var result = new Address();
            result.CopyFrom(address, countries);
            return result;
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




    }
}
