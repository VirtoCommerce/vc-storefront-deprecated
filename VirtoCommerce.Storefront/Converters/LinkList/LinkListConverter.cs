using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.LinkList.Factories;
using contentDTO = VirtoCommerce.Storefront.AutoRestClients.ContentModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class LinkListConverterExtension
    {
        public static LinkListConverter LinkListConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LinkListConverter>();
            }
        }

        public static MenuLinkList ToMenuLinkList(this contentDTO.MenuLinkList menuLinkListDTO)
        {
            return LinkListConverterInstance.ToMenuLinkList(menuLinkListDTO);
        }

        public static MenuLink ToMenuLink(this contentDTO.MenuLink menuLinkDTO)
        {
            return LinkListConverterInstance.ToMenuLink(menuLinkDTO);
        }
    }

    public class LinkListConverter
    {
        public virtual MenuLinkList ToMenuLinkList(contentDTO.MenuLinkList menuLinkListDTO)
        {
            var result = ServiceLocator.Current.GetInstance<LinkListFactory>().CreateMenuLinkList();

            result.InjectFrom<NullableAndEnumValueInjecter>(menuLinkListDTO);

            result.Language = string.IsNullOrEmpty(menuLinkListDTO.Language) ? Language.InvariantLanguage : new Language(menuLinkListDTO.Language);

            if (menuLinkListDTO.MenuLinks != null)
            {
                result.MenuLinks = menuLinkListDTO.MenuLinks.Select(ToMenuLink).ToList();
            }

            return result;
        }

        public virtual MenuLink ToMenuLink(contentDTO.MenuLink menuLinkDTO)
        {
            var result = ServiceLocator.Current.GetInstance<LinkListFactory>().CreateMenuLink(menuLinkDTO.AssociatedObjectType);
            result.InjectFrom<NullableAndEnumValueInjecter>(menuLinkDTO);

            return result;
        }
    }
}
