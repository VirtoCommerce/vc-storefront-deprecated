using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using catalogModel = VirtoCommerce.CatalogModule.Client.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AssociationConverter
    {
        public static Association ToWebModel(this catalogModel.ProductAssociation association)
        {
            Association retVal = null;
            if(association.AssociatedObjectType.EqualsInvariant("product"))
            {
                retVal = new ProductAssociation
                {
                    ProductId = association.AssociatedObjectId            
                };
            }
            else if(association.AssociatedObjectType.EqualsInvariant("category"))
            {
                retVal = new CategoryAssociation
                {
                    CategoryId = association.AssociatedObjectId
                };
            }

            if (retVal != null)
            {
                retVal.InjectFrom<NullableAndEnumValueInjecter>(association);
                retVal.Image = new Image { Url = association.AssociatedObjectImg };
            }
       
            return retVal;
        }
    }
}
