Open Tools > NuGet Package Manager > Package Manager Console and use the following command to generate API client for search module:

AutoRest.exe -Input http://localhost/admin/docs/VirtoCommerce.Search/v1 -OutputFileName SearchModuleApi.cs -Namespace VirtoCommerce.Storefront.AutoRestClients.SearchModuleApi -ClientName SearchModuleApiClient -OutputDirectory VirtoCommerce.Storefront\AutoRestClients -AddCredentials true -UseDateTimeOffset false

Then include the generated file in the project.