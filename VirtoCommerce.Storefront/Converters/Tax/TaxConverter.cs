using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Tax;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

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

        public static coreDto.TaxEvaluationContext ToTaxEvaluationContextDto(this TaxEvaluationContext taxContext)
        {
            return TaxConverterInstance.ToTaxEvaluationContextDto(taxContext);
        }

        public static TaxEvaluationContext ToTaxEvaluationContext(this WorkContext workContext, IEnumerable<Product> products = null)
        {
            return TaxConverterInstance.ToTaxEvaluationContext(workContext, products);
        }

        public static TaxRate ToTaxRate(this coreDto.TaxRate taxRateDto, Currency currency)
        {
            return TaxConverterInstance.ToTaxRate(taxRateDto, currency);
        }
    }

    public class TaxConverter
    {
        public virtual TaxRate ToTaxRate(coreDto.TaxRate taxRateDto, Currency currency)
        {
            var result = new TaxRate(currency)
            {
                Rate = new Money(taxRateDto.Rate.Value, currency)
            };

            if (taxRateDto.Line != null)
            {
                result.Line = new TaxLine(currency);
                result.Line.InjectFrom(taxRateDto.Line);
                result.Line.Amount = new Money(taxRateDto.Line.Amount.Value, currency);
                result.Line.Price = new Money(taxRateDto.Line.Price.Value, currency);
            }

            return result;
        }

        public virtual coreDto.TaxEvaluationContext ToTaxEvaluationContextDto(TaxEvaluationContext taxContext)
        {
            var retVal = new coreDto.TaxEvaluationContext();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(taxContext);
            if (taxContext.Address != null)
            {
                retVal.Address = taxContext.Address.ToCoreAddressDto();
            }
            if (taxContext.Customer != null)
            {
                retVal.Customer = taxContext.Customer.ToCoreContactDto();
            }
            if (taxContext.Currency != null)
            {
                retVal.Currency = taxContext.Currency.Code;
            }

            retVal.Lines = new List<coreDto.TaxLine>();
            if (!taxContext.Lines.IsNullOrEmpty())
            {
                foreach (var line in taxContext.Lines)
                {
                    var serviceModelLine = new coreDto.TaxLine();
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
            var result = new TaxEvaluationContext(workContext.CurrentStore.Id);
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
