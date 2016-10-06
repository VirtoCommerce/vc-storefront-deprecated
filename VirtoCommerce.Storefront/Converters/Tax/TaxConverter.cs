using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Tax;
using VirtoCommerce.Storefront.Model.Tax.Factories;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class TaxConverterExtension
    {
        public static TaxConverter TaxConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TaxConverter>();
            }
        }

        public static coreDTO.TaxEvaluationContext ToTaxEvaluationContextDTO(this TaxEvaluationContext taxContext)
        {
            return TaxConverterInstance.ToTaxEvaluationContextDTO(taxContext);
        }

        public static TaxEvaluationContext ToTaxEvaluationContext(this WorkContext workContext, IEnumerable<Product> products = null)
        {
            return TaxConverterInstance.ToTaxEvaluationContext(workContext, products);
        }

        public static TaxRate ToTaxRate(this coreDTO.TaxRate taxRateDTO, Currency currency)
        {
            return TaxConverterInstance.ToTaxRate(taxRateDTO, currency);
        }
    }

    public class TaxConverter
    {
        public virtual TaxRate ToTaxRate(coreDTO.TaxRate taxRateDTO, Currency currency)
        {
            var result = new TaxRate(currency)
            {
                Rate = new Money(taxRateDTO.Rate.Value, currency)
            };

            if (taxRateDTO.Line != null)
            {
                result.Line = new TaxLine(currency);
                result.Line.InjectFrom(taxRateDTO.Line);
                result.Line.Amount = new Money(taxRateDTO.Line.Amount.Value, currency);
                result.Line.Price = new Money(taxRateDTO.Line.Price.Value, currency);
            }

            return result;
        }

        public virtual coreDTO.TaxEvaluationContext ToTaxEvaluationContextDTO(TaxEvaluationContext taxContext)
        {
            var retVal = new coreDTO.TaxEvaluationContext();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(taxContext);
            if (taxContext.Address != null)
            {
                retVal.Address = taxContext.Address.ToCoreAddressDTO();
            }
            if (taxContext.Customer != null)
            {
                retVal.Customer = taxContext.Customer.ToCoreContactDTO();
            }
            if (taxContext.Currency != null)
            {
                retVal.Currency = taxContext.Currency.Code;
            }

            retVal.Lines = new List<coreDTO.TaxLine>();
            if (!taxContext.Lines.IsNullOrEmpty())
            {
                foreach(var line in taxContext.Lines)
                {
                    var serviceModelLine = new coreDTO.TaxLine();
                    serviceModelLine.InjectFrom<NullableAndEnumValueInjecter>(line);
                    serviceModelLine.Amount = (double)line.Amount.Amount;
                    serviceModelLine.Price = (double)line.Price.Amount;

                    retVal.Lines.Add(serviceModelLine);
                }
            }
            return retVal;
        }

      
        public virtual TaxEvaluationContext ToTaxEvaluationContext(WorkContext workContext, IEnumerable<Product> products = null)
        {
            var result = ServiceLocator.Current.GetInstance<TaxFactory>().CreateTaxEvaluationContext(workContext.CurrentStore.Id);
            result.Id = workContext.CurrentStore.Id;
            result.Currency = workContext.CurrentCurrency;
            result.Type = "";
            result.Address = workContext.CurrentCustomer.DefaultBillingAddress;
            result.Customer = workContext.CurrentCustomer;
           
            if (products != null)
            {
                result.Lines = products.SelectMany(x => x.ToTaxLines()).ToList();
            }
            return result;
        }

     
    }
}
