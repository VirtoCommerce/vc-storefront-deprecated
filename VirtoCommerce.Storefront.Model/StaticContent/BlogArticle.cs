using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.Storefront.Model.StaticContent
{
    /// <summary>
    /// TODO: Comments and author user info
    /// </summary>
    public class BlogArticle : ContentItem
    {
        private static string _excerpToken = "<!--excerpt-->";

        public override string Type { get { return "post"; } }

        public string Excerpt { get; set; }

        public string BlogName { get; set; }

        public string ImageUrl { get; set; }

        public override void LoadContent(string content, IDictionary<string, IEnumerable<string>> metaInfoMap, IDictionary themeSettings)
        {
            var parts = content.Split(new[] { _excerpToken }, StringSplitOptions.None);

            if (parts.Length > 1)
            {
                Excerpt = parts[0];
                content.Replace(_excerpToken, string.Empty);
            }

            if (metaInfoMap.ContainsKey("main-image"))
            {
                ImageUrl = metaInfoMap["main-image"].FirstOrDefault();
            }

            if (metaInfoMap.ContainsKey("excerpt"))
            {
                Excerpt = metaInfoMap["excerpt"].FirstOrDefault();
            }

            base.LoadContent(content, metaInfoMap, themeSettings);
        }
    }
}
