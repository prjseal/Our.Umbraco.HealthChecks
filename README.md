# Our.Umbraco.HealthChecks

![Our.Umbraco.HealthChecks](/images/health-check.png)

[![Build status](https://ci.appveyor.com/api/projects/status/okgo4pkpogij6a8g?svg=true)](https://ci.appveyor.com/project/prjseal/our-umbraco-healthchecks)


[![NuGet](https://img.shields.io/nuget/dt/Our.Umbraco.HealthChecks.svg)](https://www.nuget.org/packages/Our.Umbraco.HealthChecks/)

This repo is a collection of community written [Health Checks](https://our.umbraco.com/Documentation/Extending/Health-Check/) for Umbraco CMS.

Please follow the naming of the folders and checks which are in the [Umbraco Core Health Checks](https://github.com/umbraco/Umbraco-CMS/tree/v8/dev/src/Umbraco.Web/HealthCheck/Checks)

*Don't forget to do a NuGet restore.*

You can login to the website and test the health checks. Here are the login details:

<strong>username:</strong> admin@admin.com<br/>
<strong>password:</strong> 1234567890

This package has been ported over from Umbraco v7 to Umbraco v8.
If you are looking for the Umbraco v7 version then switch to the dev/v7 branch

## Checks ported to the Umbraco v8 version:

| Check                 | Description                                                            | Id  |
| ---------------------------- |:---------------------------------------------------------------------- | --- |
| **Azure**                    |                                                                        |     |
| AzureFcnModeCheck            | Checks that fcnMode config is appropriate for the Azure platform.      |EA9619FE-1DF4-4399-A4E5-32F2CF0CDC1F|
| AzureTempStorageCheck        | Checks that temp storage config is appropriate for the Azure platform. |F9088377-103A-4712-B428-D4AB6E5B2A67|
| **Config**                   |                                                                        |     |
| PostProcessorCheck           | Check if ImageProcessor.Web.PostProcessor is installed                 |CA765D50-85D9-4346-BBC4-8DEEBB7EBAE2|
| UmbracoPathCheck             | Checks to see if you have changed the umbraco path.                    |467EFE42-E37D-47FE-A75F-E2D7D2D98438|
| **Security**                 |                                                                        |     |
| AdminUserCheck               | Check the admin user isn't called 'admin'                              |42A3A15F-C2F0-48E7-AE5A-1237C5AF5E35|
| ClientDependencyVersionCheck | Check the version number of ClientDepency.Core.dll                     |C6D425DF-47A6-4526-A915-AAA39192634D|

## The following Umbraco v7 checks were removed from the Umbraco v8 version:

| Check                        | Reason                                                            |
| ---------------------------- |:------------------------------------------------------------------|
| **Azure**                    |                                                                   |
| AzureExamineCheck            | Removed because Examine config has been removed.                  |
| AzureLoggingCheck            | Removed now we have moved to serilog                              |
| **Config**                   |                                                                   |
| ExamineRebuildOnStartCheck   | Removed because Examine config has been removed                   |
| **Data Integrity**           |                                                                   |
| ContentVersionsCheck         | Removed until we understand how to query the content versions     |
| **SEO**                      |                                                                   |
| LorumIpsumCheck              | Removed until we understand how to search using Examine           |
| XmlSitemapCheck              | Removed until we understand how to get the httpcontext            |
| **Security**                 |                                                                   |
| HstsCheck                    | Exists in Core                                                    |
| TlsCheck                     | Exists in Core                                                    |

## Suggest Checks

If you would like to suggest checks please raise it as an issue.

## Contribute Checks

If you would like to contribute any checks, please either choose one from the existing issues list, or create an issue first and link to it in the PR using a hashtag and the issue number i.e. #1234
