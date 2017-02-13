1. Open Tools > NuGet Package Manager > Package Manager Console
2. Run the following command to generate API clients:

$modules = @('Cache','Cart','Catalog','Content','Core','Customer','Inventory','Marketing','Orders','Platform','Pricing','Quote','Search','SearchApi','Sitemaps','Store','Subscription')
$modules.ForEach( { AutoRest.exe -Input http://localhost/admin/docs/VirtoCommerce.$_/v1  -OutputFileName $_`ModuleApi.cs -Namespace VirtoCommerce.Storefront.AutoRestClients.$_`ModuleApi -ClientName $_`ModuleApiClient -OutputDirectory VirtoCommerce.Storefront\AutoRestClients -AddCredentials true -UseDateTimeOffset false })
