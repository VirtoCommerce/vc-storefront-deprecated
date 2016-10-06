using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;
using customerDTO = VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi.Models;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using VirtoCommerce.Storefront.Model.Customer.Factories;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CustomerConverterExtension
    {
        public static CustomerConverter CustomerConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CustomerConverter>();
            }
        }

        public static CustomerInfo ToCustomerInfo(this Register formModel)
        {           
            return CustomerConverterInstance.ToCustomerInfo(formModel);
        }

        public static CustomerInfo ToCustomerInfo(this customerDTO.Contact contactDTO)
        {
            return CustomerConverterInstance.ToCustomerInfo(contactDTO);
        }

        public static customerDTO.Contact ToCustomerContactDTO(this CustomerInfo customer)
        {
            return CustomerConverterInstance.ToCustomerContactDTO(customer);
        }

        public static coreDTO.Contact ToCoreContactDTO(this CustomerInfo customer)
        {
            return CustomerConverterInstance.ToCoreContactDTO(customer);
        }

        public static Vendor ToVendor(this customerDTO.Vendor vendorDTO, Language currentLanguage, Store store)
        {
            return CustomerConverterInstance.ToVendor(vendorDTO, currentLanguage, store);
        }

        public static Address ToAddress(this customerDTO.Address addressDTO)
        {
            return CustomerConverterInstance.ToAddress(addressDTO);
        }

        public static customerDTO.Address ToCustomerAddressDTO(this Address address)
        {
            return CustomerConverterInstance.ToCustomerAddressDTO(address);
        }

        public static DynamicProperty ToDynamicProperty(this customerDTO.DynamicObjectProperty propertyDTO)
        {
            return CustomerConverterInstance.ToDynamicProperty(propertyDTO);
        }

        public static customerDTO.DynamicObjectProperty ToCustomerDynamicPropertyDTO(this DynamicProperty property)
        {
            return CustomerConverterInstance.ToCustomerDynamicPropertyDTO(property);
        }     
    }

    public class CustomerConverter
    {
        private static readonly char[] _nameSeparator = { ' ' };

    
        public virtual DynamicProperty ToDynamicProperty(customerDTO.DynamicObjectProperty propertyDTO)
        {
            return propertyDTO.JsonConvert<coreDTO.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual customerDTO.DynamicObjectProperty ToCustomerDynamicPropertyDTO(DynamicProperty property)
        {
            return property.ToDynamicPropertyDTO().JsonConvert<customerDTO.DynamicObjectProperty>();
        }

        public virtual Address ToAddress(customerDTO.Address addressDTO)
        {
            return addressDTO.JsonConvert<coreDTO.Address>().ToAddress();
        }

        public virtual customerDTO.Address ToCustomerAddressDTO(Address address)
        {
            return address.ToCoreAddressDTO().JsonConvert<customerDTO.Address>();
        }

        public virtual Vendor ToVendor(customerDTO.Vendor vendorDTO, Language currentLanguage, Store store)
        {
            Vendor result = null;

            if (vendorDTO != null)
            {
                result = ServiceLocator.Current.GetInstance<CustomerFactory>().CreateVendor();
                result.Id = vendorDTO.Id;
                result.Name = vendorDTO.Name;
                result.Description = vendorDTO.Description;
                result.LogoUrl = vendorDTO.LogoUrl;
                result.SiteUrl = vendorDTO.SiteUrl;
                result.GroupName = vendorDTO.GroupName;

                if(!vendorDTO.SeoInfos.IsNullOrEmpty())
                {
                    var seoInfoDTO = vendorDTO.SeoInfos.Select(x => x.JsonConvert<coreDTO.SeoInfo>())
                                                .GetBestMatchedSeoInfo(store, currentLanguage);
                    if(seoInfoDTO != null)
                    {
                        result.SeoInfo = seoInfoDTO.ToSeoInfo();
                    }
                }

                if(result.SeoInfo == null)
                {
                    result.SeoInfo = new SeoInfo
                    {
                        Title = vendorDTO.Name,
                        Slug = string.Concat("/vendor/", result.Id)
                    };
                }

                if (vendorDTO.Addresses != null)
                {
                    result.Addresses = vendorDTO.Addresses.Select(ToAddress).ToList();
                }

                if (vendorDTO.DynamicProperties != null)
                {
                    result.DynamicProperties = vendorDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
                }
            }

            return result;
        }

        public virtual CustomerInfo ToCustomerInfo(Register formModel)
        {
            var result = ServiceLocator.Current.GetInstance<CustomerFactory>().CreateCustomerInfo();
            result.Email = formModel.Email;
            result.FullName = string.Join(" ", formModel.FirstName, formModel.LastName);
            result.FirstName = formModel.FirstName;
            result.LastName = formModel.LastName;
            
            if (string.IsNullOrEmpty(result.FullName) || string.IsNullOrWhiteSpace(result.FullName))
            {
                result.FullName = formModel.Email;
            }
            return result;
        }

        public virtual CustomerInfo ToCustomerInfo(customerDTO.Contact contactDTO)
        {
            var result = ServiceLocator.Current.GetInstance<CustomerFactory>().CreateCustomerInfo(); 
            result.InjectFrom<NullableAndEnumValueInjecter>(contactDTO);

            result.IsRegisteredUser = true;
            if (contactDTO.Addresses != null)
            {
                result.Addresses = contactDTO.Addresses.Select(ToAddress).ToList();
            }

            result.DefaultBillingAddress = result.Addresses.FirstOrDefault(a => (a.Type & AddressType.Billing) == AddressType.Billing);
            result.DefaultShippingAddress = result.Addresses.FirstOrDefault(a => (a.Type & AddressType.Shipping) == AddressType.Shipping);

            // TODO: Need separate properties for first, middle and last name
            if (!string.IsNullOrEmpty(contactDTO.FullName))
            {
                var nameParts = contactDTO.FullName.Split(_nameSeparator, 2);

                if (nameParts.Length > 0)
                {
                    result.FirstName = nameParts[0];
                }

                if (nameParts.Length > 1)
                {
                    result.LastName = nameParts[1];
                }
            }

            if (contactDTO.Emails != null)
            {
                result.Email = contactDTO.Emails.FirstOrDefault();
            }

            if (!contactDTO.DynamicProperties.IsNullOrEmpty())
            {
                result.DynamicProperties = contactDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            return result;
        }

        public virtual customerDTO.Contact ToCustomerContactDTO(CustomerInfo customer)
        {
            var retVal = new customerDTO.Contact();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(customer);
            if (customer.Addresses != null)
            {
                retVal.Addresses = customer.Addresses.Select(ToCustomerAddressDTO).ToList();
            }
            if (!string.IsNullOrEmpty(customer.Email))
            {
                retVal.Emails = new[] { customer.Email }.ToList();
            }
            retVal.FullName = customer.FullName;

            return retVal;
        }

        public virtual coreDTO.Contact ToCoreContactDTO(CustomerInfo customer)
        {
            var retVal = new coreDTO.Contact();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(customer);
            if (customer.Addresses != null)
            {
                retVal.Addresses = customer.Addresses.Select(x => x.ToCoreAddressDTO()).ToList();
            }
            if (!string.IsNullOrEmpty(customer.Email))
            {
                retVal.Emails = new[] { customer.Email }.ToList();
            }
            retVal.FullName = customer.FullName;

            return retVal;
        }
    }
}
