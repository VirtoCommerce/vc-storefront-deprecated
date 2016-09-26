using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coreService = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Tax;
using VirtoCommerce.Storefront.Model.Tax.Services;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;

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
        public async Task EvaluateTaxesAsync(TaxEvaluationContext context, IEnumerable<ITaxable> owners)
        {
            var taxRates = await _coreModuleApiClient.Commerce.EvaluateTaxesAsync(context.StoreId, context.ToServiceModel());
            InnerEvaluateTaxes(taxRates, owners);
        }

        public void EvaluateTaxes(TaxEvaluationContext context, IEnumerable<ITaxable> owners)
        {
            var taxRates = _coreModuleApiClient.Commerce.EvaluateTaxes(context.StoreId, context.ToServiceModel());
            InnerEvaluateTaxes(taxRates, owners);
        }

        #endregion

        private static void InnerEvaluateTaxes(IList<coreService.TaxRate> taxRates, IEnumerable<ITaxable> owners)
        {
            if (taxRates == null)
            {
                return;
            }

            foreach (var owner in owners)
            {
                owner.ApplyTaxRates(taxRates.Select(x => x.ToWebModel(owner.Currency)));
            }
        }
    }
}
