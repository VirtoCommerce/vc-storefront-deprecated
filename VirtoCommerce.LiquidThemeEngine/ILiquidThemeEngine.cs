using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.LiquidThemeEngine
{
    public interface ILiquidThemeEngine
    {
        IEnumerable<string> DiscoveryPaths { get; }
        string ResolveTemplatePath(string templateName, bool searchInGlobalThemeOnly = false);
        string RenderTemplateByName(string templateName, Dictionary<string, object> parameters);
        string RenderTemplate(string templateContent, Dictionary<string, object> parameters);
        IDictionary GetSettings(string defaultValue = null);
        JObject ReadLocalization();
        Stream GetAssetStream(string fileName, bool searchInGlobalThemeOnly = false);
        string GetAssetAbsoluteUrl(string assetName);
        string GetGlobalAssetAbsoluteUrl(string assetName);
    }
}
