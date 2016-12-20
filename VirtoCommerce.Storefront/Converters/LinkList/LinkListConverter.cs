using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using contentDto = VirtoCommerce.Storefront.AutoRestClients.ContentModuleApi.Models;

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

        public static MenuLinkList ToMenuLinkList(this contentDto.MenuLinkList menuLinkListDto)
        {
            return LinkListConverterInstance.ToMenuLinkList(menuLinkListDto);
        }

        public static MenuLink ToMenuLink(this contentDto.MenuLink menuLinkDto)
        {
            return LinkListConverterInstance.ToMenuLink(menuLinkDto);
        }
    }

    public class LinkListConverter
    {
        public virtual MenuLinkList ToMenuLinkList(contentDto.MenuLinkList menuLinkListDto)
        {
            var result = new MenuLinkList();

            result.InjectFrom<NullableAndEnumValueInjecter>(menuLinkListDto);

            result.Language = string.IsNullOrEmpty(menuLinkListDto.Language) ? Language.InvariantLanguage : new Language(menuLinkListDto.Language);

            if (menuLinkListDto.MenuLinks != null)
            {
                result.MenuLinks = menuLinkListDto.MenuLinks.Select(ToMenuLink).ToList();
            }

            return result;
        }

        public virtual MenuLink ToMenuLink(contentDto.MenuLink menuLinkDto)
        {
            var result = new MenuLink();
            if (menuLinkDto.AssociatedObjectType != null)
            {
                if ("product" == menuLinkDto.AssociatedObjectType.ToLowerInvariant())
                {
                    result = new ProductMenuLink();
                }
                else if ("category" == menuLinkDto.AssociatedObjectType.ToLowerInvariant())
                {
                    result = new CategoryMenuLink();
                }
            }         
            result.InjectFrom<NullableAndEnumValueInjecter>(menuLinkDto);

            return result;
        }
    }
}
