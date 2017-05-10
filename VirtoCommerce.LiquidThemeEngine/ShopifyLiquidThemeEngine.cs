using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using DotLiquid.ViewEngine.Exceptions;
using LibSassNetProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.LiquidThemeEngine.Converters;
using VirtoCommerce.LiquidThemeEngine.Extensions;
using VirtoCommerce.LiquidThemeEngine.Filters;
using VirtoCommerce.LiquidThemeEngine.Operators;
using VirtoCommerce.LiquidThemeEngine.Tags;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.LiquidThemeEngine
{
    /// <summary>
    /// Shopify compliant theme folder structure and all methods for rendering
    /// assets - storages for css, images and other assets
    /// config - contains theme configuration
    /// layout - master pages and layouts
    /// locales - localization resources
    /// snippets - snippets - partial views
    /// templates - view templates
    /// </summary>
    public class ShopifyLiquidThemeEngine : IFileSystem, ILiquidThemeEngine
    {
        private static readonly Regex _isLiquid = new Regex("[{}|]", RegexOptions.Compiled);
        private const string _globalThemeName = "default";
        private const string _defaultMasterView = "theme";
        private const string _liquidTemplateFormat = "{0}.liquid";
        private static readonly string[] _templatesDiscoveryFolders = { "templates", "snippets", "layout", "assets" };
        private readonly string _themesAssetsRelativeUrl;
        private readonly string _globalThemeAssetsRelativeUrl;
        private readonly Func<WorkContext> _workContextFactory;
        private readonly Func<IStorefrontUrlBuilder> _storeFrontUrlBuilderFactory;
        private readonly ILocalCacheManager _cacheManager;
        private readonly SassCompilerProxy _saasCompiler = new SassCompilerProxy();
        private readonly IContentBlobProvider _themeBlobProvider;
        private readonly IContentBlobProvider _globalThemeBlobProvider;

        public ShopifyLiquidThemeEngine(ILocalCacheManager cacheManager, Func<WorkContext> workContextFactory, Func<IStorefrontUrlBuilder> storeFrontUrlBuilderFactory, IContentBlobProvider themeBlobProvider, IContentBlobProvider globalThemeBlobProvider, string themesAssetsRelativeUrl, string globalThemeAssetsRelativeUrl)
        {
            _workContextFactory = workContextFactory;
            _storeFrontUrlBuilderFactory = storeFrontUrlBuilderFactory;
            _themesAssetsRelativeUrl = themesAssetsRelativeUrl;
            _globalThemeAssetsRelativeUrl = globalThemeAssetsRelativeUrl;
            _cacheManager = cacheManager;
            _themeBlobProvider = themeBlobProvider;
            _globalThemeBlobProvider = globalThemeBlobProvider;

            Liquid.UseRubyDateFormat = true;
            // Register custom tags (Only need to do this once)
            Template.RegisterFilter(typeof(CommonFilters));
            Template.RegisterFilter(typeof(CommerceFilters));
            Template.RegisterFilter(typeof(TranslationFilter));
            Template.RegisterFilter(typeof(UrlFilters));
            Template.RegisterFilter(typeof(DateFilters));
            Template.RegisterFilter(typeof(MoneyFilters));
            Template.RegisterFilter(typeof(HtmlFilters));
            Template.RegisterFilter(typeof(StringFilters));
            Template.RegisterFilter(typeof(ArrayFilters));
            Template.RegisterFilter(typeof(BundleFilters));
            Template.RegisterFilter(typeof(MathFilters));

            Condition.Operators["contains"] = CommonOperators.ContainsMethod;

            Template.RegisterTag<LayoutTag>("layout");
            Template.RegisterTag<FormTag>("form");
            Template.RegisterTag<PaginateTag>("paginate");

            //Observe themes content system changes to invalidate cache if changes occur
            _themeBlobProvider.Changed += (sender, args) =>
            {
                _cacheManager.Clear();
            };
            _themeBlobProvider.Renamed += (sender, args) =>
            {
                _cacheManager.Clear();
            };
            _globalThemeBlobProvider.Changed += (sender, args) =>
            {
                _cacheManager.Clear();
            };
            _globalThemeBlobProvider.Renamed += (sender, args) =>
            {
                _cacheManager.Clear();
            };
        }

        /// <summary>
        /// Main work context
        /// </summary>
        public WorkContext WorkContext => _workContextFactory();

        /// <summary>
        /// Store url builder
        /// </summary>
        public IStorefrontUrlBuilder UrlBuilder => _storeFrontUrlBuilderFactory();

        /// <summary>
        /// Default master view name
        /// </summary>
        public string MasterViewName => _defaultMasterView;

        /// <summary>
        /// Current theme name
        /// </summary>
        public string CurrentThemeName => string.IsNullOrEmpty(WorkContext.CurrentStore.ThemeName) ? _globalThemeName : WorkContext.CurrentStore.ThemeName;


        /// <summary>
        /// Current theme base path
        /// </summary>
        private string CurrentThemePath => Path.Combine(WorkContext.CurrentStore.Id, CurrentThemeName);

        #region IFileSystem members
        public string ReadTemplateFile(Context context, string templateName)
        {
            return ReadTemplateByName(templateName);
        }
        #endregion

        #region ILiquidThemeEngine Members
        public IEnumerable<string> DiscoveryPaths
        {
            get
            {
                IEnumerable<string> retVal = _templatesDiscoveryFolders;
                if (WorkContext.CurrentStore != null)
                {
                    retVal = retVal.Select(x => Path.Combine(CurrentThemePath, x)).Concat(retVal);
                }
                return retVal;
            }
        }

        /// <summary>
        /// Return stream for requested  asset file  (used for search current and base themes assets)
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="searchInGlobalThemeOnly"></param>
        /// <returns></returns>
        public Stream GetAssetStream(string filePath, bool searchInGlobalThemeOnly = false)
        {
            Stream retVal = null;
            var filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)).Replace("\\", "/");
            //file.ext => file.ext || file || file.liquid || file.ext.liquid        
            var searchPatterns = new[] { filePath, filePathWithoutExtension, string.Format(_liquidTemplateFormat, filePathWithoutExtension), string.Format(_liquidTemplateFormat, filePath) };

            string currentThemeFilePath = null;
            //search in global theme first 
            var globalThemeFilePath = searchPatterns.SelectMany(x => _globalThemeBlobProvider.Search("assets", x, true)).FirstOrDefault();
            if (!searchInGlobalThemeOnly)
            {
                //try to search in current store theme 
                if (_themeBlobProvider.PathExists(CurrentThemePath + "\\assets"))
                {
                    currentThemeFilePath = searchPatterns.SelectMany(x => _themeBlobProvider.Search(CurrentThemePath + "\\assets", x, true)).FirstOrDefault();
                }
            }

            if (currentThemeFilePath != null)
            {
                retVal = _themeBlobProvider.OpenRead(currentThemeFilePath);
                filePath = currentThemeFilePath;
            }
            else if (globalThemeFilePath != null)
            {
                retVal = _globalThemeBlobProvider.OpenRead(globalThemeFilePath);
                filePath = globalThemeFilePath;
            }

            if (retVal != null && filePath.EndsWith(".liquid"))
            {
                var shopifyContext = WorkContext.ToShopifyModel(UrlBuilder);
                var parameters = shopifyContext.ToLiquid() as Dictionary<string, object>;
                var settings = GetSettings("''");
                parameters.Add("settings", settings);

                var templateContent = retVal.ReadToString();
                retVal.Dispose();

                var template = RenderTemplate(templateContent, parameters);
                retVal = new MemoryStream(Encoding.UTF8.GetBytes(template));
            }

            if (retVal != null && (filePath.Contains(".scss.") || filePath.EndsWith(".scss")))
            {
                var content = retVal.ReadToString();
                retVal.Dispose();

                try
                {
                    //handle scss resources
                    content = _saasCompiler.Compile(content);
                    retVal = new MemoryStream(Encoding.UTF8.GetBytes(content));
                }
                catch (Exception ex)
                {
                    throw new SaasCompileException(filePath, content, ex);
                }
            }

            return retVal;
        }

        /// <summary>
        /// resolve  template path by it name
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="searchInGlobalThemeOnly"></param>
        /// <returns></returns>
        public string ResolveTemplatePath(string templateName, bool searchInGlobalThemeOnly = false)
        {
            if (WorkContext.CurrentStore == null)
                return null;

            var liquidTemplateFileName = string.Format(_liquidTemplateFormat, templateName);
            var curentThemediscoveryPaths = _templatesDiscoveryFolders.Select(x => Path.Combine(CurrentThemePath, x, liquidTemplateFileName));

            //First try to find template in current theme folder
            var retVal = curentThemediscoveryPaths.FirstOrDefault(x => _themeBlobProvider.PathExists(x));
            if (searchInGlobalThemeOnly || retVal == null)
            {
                //Then try to find in global theme
                var globalThemeDiscoveyPaths = _templatesDiscoveryFolders.Select(x => Path.Combine(x, liquidTemplateFileName));
                retVal = globalThemeDiscoveyPaths.FirstOrDefault(x => _globalThemeBlobProvider.PathExists(x));
            }

            return retVal;
        }

        /// <summary>
        /// Render template by name and with passed context (parameters)
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string RenderTemplateByName(string templateName, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException(nameof(templateName));

            var templateContent = ReadTemplateByName(templateName);
            var retVal = RenderTemplate(templateContent, parameters);
            return retVal;
        }

        /// <summary>
        /// Render template by content and parameters
        /// </summary>
        /// <param name="templateContent"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string RenderTemplate(string templateContent, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(templateContent))
            {
                return templateContent;
            }

            var isLiquidTemplate = _isLiquid.Match(templateContent);
            if (!isLiquidTemplate.Success)
            {
                return templateContent;
            }

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            Template.FileSystem = this;

            var renderParams = new RenderParameters
            {
                LocalVariables = Hash.FromDictionary(parameters)
            };

            var parsedTemplate = Template.Parse(templateContent);

            var retVal = parsedTemplate.RenderWithTracing(renderParams);

            //Copy key values which were generated in rendering to out parameters
            if (parsedTemplate.Registers != null)
            {
                foreach (var registerPair in parsedTemplate.Registers)
                {
                    parameters[registerPair.Key] = registerPair.Value;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Read shopify theme settings from 'config' folder
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public IDictionary GetSettings(string defaultValue = null)
        {
            return _cacheManager.Get(GetCacheKey("GetSettings", defaultValue), "LiquidThemeRegion", () =>
            {
                var retVal = new DefaultableDictionary(defaultValue);

                //Load all data from current theme config
                var currentThemeSettings = InnerGetAllSettings(_themeBlobProvider, CurrentThemePath);

                //Load all data from global theme config
                var resultSettings = InnerGetAllSettings(_globalThemeBlobProvider, string.Empty);

                if (currentThemeSettings != null)
                {
                    //Merge two configs
                    resultSettings.Merge(currentThemeSettings, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge });
                }

                //Get actual preset from merged config
                var currentPreset = resultSettings.GetValue("current");
                if(currentPreset is JValue)
                {
                    string currentPresetName = ((JValue)currentPreset).Value.ToString();
                    var presets = resultSettings.GetValue("presets") as JObject;
                    if (presets == null || !presets.Children().Any())
                    {
                        throw new StorefrontException("Setting presets not defined");
                    }

                    IList<JProperty> allPresets = presets.Children().Cast<JProperty>().ToList();
                    resultSettings = allPresets.FirstOrDefault(p => p.Name == currentPresetName).Value as JObject;
                    if(resultSettings == null)
                    {
                        throw new StorefrontException($"Setting preset with name '{currentPresetName}' not found");
                    }
                }
                if (currentPreset is JObject)
                {
                    resultSettings = (JObject)currentPreset;
                }

                var dict = resultSettings.ToObject<Dictionary<string, object>>().ToDictionary(x => x.Key, x => x.Value);
                retVal = new DefaultableDictionary(dict, defaultValue);

                return retVal;
            });
        }


        /// <summary>
        /// Read localization resources 
        /// </summary>
        /// <returns></returns>
        public JObject ReadLocalization()
        {
            return _cacheManager.Get(GetCacheKey("ReadLocalization"), "LiquidThemeRegion", () =>
            {
                //Load first localization from global theme
                var retVal = InnerReadLocalization(_globalThemeBlobProvider, "", WorkContext.CurrentLanguage) ?? new JObject();

                //Next need merge current theme localization with default
                var currentThemeLocalization = InnerReadLocalization(_themeBlobProvider, CurrentThemePath, WorkContext.CurrentLanguage);
                if (currentThemeLocalization != null)
                {
                    retVal.Merge(currentThemeLocalization, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge });
                }

                return retVal;
            });
        }

        /// <summary>
        /// Get relative url for assets (assets folder)
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetAssetAbsoluteUrl(string assetName)
        {
            return UrlBuilder.ToAppAbsolute(_themesAssetsRelativeUrl.TrimEnd('/') + "/" + assetName.TrimStart('/'), WorkContext.CurrentStore, WorkContext.CurrentLanguage);
        }

        /// <summary>
        /// Get relative url for global assets
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetGlobalAssetAbsoluteUrl(string assetName)
        {
            return UrlBuilder.ToAppAbsolute(_globalThemeAssetsRelativeUrl.TrimEnd('/') + "/" + assetName.TrimStart('/'), WorkContext.CurrentStore, WorkContext.CurrentLanguage);
        }
        #endregion


        private static JObject InnerReadLocalization(IContentBlobProvider themeBlobProvider, string themePath, Language language)
        {
            JObject retVal = null;
            var localeFolderPath = Path.Combine(themePath, "locales");
            if (themeBlobProvider.PathExists(localeFolderPath))
            {
                var currentLocalePath = Path.Combine(localeFolderPath, string.Concat(language.TwoLetterLanguageName, ".json"));
                var localeDefaultPath = themeBlobProvider.Search(localeFolderPath, "*.default.json", false).FirstOrDefault();

                JObject localeJson = null;
                JObject defaultJson = null;

                if (themeBlobProvider.PathExists(currentLocalePath))
                {
                    using (var stream = themeBlobProvider.OpenRead(currentLocalePath))
                    {
                        localeJson = JsonConvert.DeserializeObject<dynamic>(stream.ReadToString());
                    }
                }

                if (localeDefaultPath != null && themeBlobProvider.PathExists(localeDefaultPath))
                {
                    using (var stream = themeBlobProvider.OpenRead(localeDefaultPath))
                    {
                        defaultJson = JsonConvert.DeserializeObject<dynamic>(stream.ReadToString());
                    }
                }

                //Need merge default and requested localization json to resulting object
                retVal = defaultJson ?? localeJson;
                if (defaultJson != null && localeJson != null)
                {
                    retVal.Merge(localeJson, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge });
                }
            }
            return retVal;
        }
        
        private static JObject InnerGetAllSettings(IContentBlobProvider themeBlobProvider, string themePath)
        {
            JObject retVal = null;
            var settingsPath = Path.Combine(themePath, "config\\settings_data.json");
            if (themeBlobProvider.PathExists(settingsPath))
            {
                using (var stream = themeBlobProvider.OpenRead(settingsPath))
                {
                    retVal = JsonConvert.DeserializeObject<JObject>(stream.ReadToString());
                }
            }
            return retVal;
        }

        private string ReadTemplateByName(string templateName)
        {
            var templatePath = ResolveTemplatePath(templateName);
            if (string.IsNullOrEmpty(templatePath))
            {
                throw new FileSystemException($"The template '{templateName}' was not found. The following locations were searched:<br/>{string.Join("<br/>", DiscoveryPaths)}");
            }

            return _cacheManager.Get(GetCacheKey("ReadTemplateByName", templatePath), "LiquidThemeRegion", () =>
            {
                // At first, try to find content in current store theme
                var blobProvider = _themeBlobProvider;
                if (!blobProvider.PathExists(templatePath))
                {
                    // Then search in global theme
                    blobProvider = _globalThemeBlobProvider;
                }

                using (var stream = blobProvider.OpenRead(templatePath))
                {
                    return stream.ReadToString();
                }
            });
        }

        private string GetCacheKey(params string[] parts)
        {
            var retVal = new[] { CurrentThemePath, WorkContext.CurrentLanguage.CultureName, WorkContext.CurrentCurrency.Code };
            if (parts != null)
            {
                retVal = retVal.Concat(parts.Select(x => x ?? string.Empty)).ToArray();
            }
            return string.Join(":", retVal).GetHashCode().ToString();
        }

    }
}
