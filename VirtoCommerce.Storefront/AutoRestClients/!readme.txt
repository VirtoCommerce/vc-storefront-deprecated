1. Open Tools > NuGet Package Manager > Package Manager Console
2. Run the following commands to generate API clients:

$modules = @('Cache','Cart','Catalog','Content','Core','Customer','Inventory','Marketing','Orders','Platform','Pricing','Quote','Sitemaps','Store','Subscription')
$modules.ForEach( { AutoRest.exe -Input http://localhost/admin/docs/VirtoCommerce.$_/v1  -OutputFileName $_`ModuleApi.cs -Namespace VirtoCommerce.Storefront.AutoRestClients.$_`ModuleApi -ClientName $_`ModuleApiClient -OutputDirectory VirtoCommerce.Storefront\AutoRestClients -AddCredentials true -UseDateTimeOffset false })


Troubleshooting

If you get strong name validation errors, you should disable strong name validation on your dev box:
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\sn.exe" -Vr *
or
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\sn.exe" -Vr *

See AutoRest guide here:
https://github.com/Azure/autorest/blob/master/docs/developer/guide/building-code.md#strong-name-validation-errors
