using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using catalogModel = VirtoCommerce.CatalogModule.Client.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ProductAssociationConverter
    {
        public static ProductAssociation ToWebModel(this catalogModel.ProductAssociation association)
        {
            var retVal = new ProductAssociation();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(association);
            retVal.ProductImage = new Image() { Url = association.ProductImg };
            retVal.ProductName = association.ProductName;
            retVal.ProductSku = association.ProductCode;
            return retVal;
        }
    }
}
