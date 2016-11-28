using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Tax;
using VirtoCommerce.Storefront.Model.Tax.Services;
using coreService = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Services
{
    public class TaxEvaluator : ITaxEvaluator
    {
        private readonly ICoreModuleApiClient _coreModuleApiClient;

        public TaxEvaluator(ICoreModuleApiClient coreModuleApiClient)
        {
            _coreModuleApiClient = coreModuleApiClient;
        }

        #region ITaxEvaluator Members
        public virtual async Task EvaluateTaxesAsync(TaxEvaluationContext context, IEnumerable<ITaxable> owners)
        {
            var taxRates = await _coreModuleApiClient.Commerce.EvaluateTaxesAsync(context.StoreId, context.ToTaxEvaluationContextDto());
            InnerEvaluateTaxes(taxRates, owners);
        }

        public virtual void EvaluateTaxes(TaxEvaluationContext context, IEnumerable<ITaxable> owners)
        {
            var taxRates = _coreModuleApiClient.Commerce.EvaluateTaxes(context.StoreId, context.ToTaxEvaluationContextDto());
            InnerEvaluateTaxes(taxRates, owners);
        }

        #endregion

        private static void InnerEvaluateTaxes(IList<coreService.TaxRate> taxRates, IEnumerable<ITaxable> owners)
        {
            if (taxRates == null)
            {
                return;
            }
            var taxRatesMap = owners.Select(x => x.Currency).Distinct().ToDictionary(x => x, x => taxRates.Select(r => r.ToTaxRate(x)).ToArray());

            foreach (var owner in owners)
            {
                owner.ApplyTaxRates(taxRatesMap[owner.Currency]);
            }
        }
    }
}
