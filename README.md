# Our.Umbraco.HealthChecks

![Our.Umbraco.HealthChecks](/images/health-check.png)

[![Build status](https://ci.appveyor.com/api/projects/status/okgo4pkpogij6a8g?svg=true)](https://ci.appveyor.com/project/prjseal/our-umbraco-healthchecks)


[![NuGet](https://img.shields.io/nuget/dt/Our.Umbraco.HealthChecks.svg)](https://www.nuget.org/packages/Our.Umbraco.HealthChecks/)

This repo is a collection of community written [Health Checks](https://our.umbraco.com/Documentation/Extending/Health-Check/) for Umbraco CMS.

Please follow the naming of the folders and checks which are in the [Umbraco Core Health Checks](https://github.com/umbraco/Umbraco-CMS/tree/dev-v7/src/Umbraco.Web/HealthCheck/Checks)

*Don't forget to do a NuGet restore.*

You can login to the website and test the health checks. Here are the login details:

<strong>username:</strong> admin@admin.com<br/>
<strong>password:</strong> 1234567890

This package has been ported over from Umbraco v7 to Umbraco v8.
If you are looking for the Umbraco v7 version then switch to the dev/v7 branch

## Checks ported to the Umbraco v8 version:

| Check                 | Description                                                            |
| --------------------- |:---------------------------------------------------------------------- |
| **Azure**             |                                                                        |
| AzureFcnModeCheck     | Checks that fcnMode config is appropriate for the Azure platform.      |
| AzureTempStorageCheck | Checks that temp storage config is appropriate for the Azure platform. |
| **Config**            |                                                                        |
| PostProcessorCheck    | Check if ImageProcessor.Web.PostProcessor is installed                 |
| UmbracoPathCheck      | Checks to see if you have changed the umbraco path.                    |
| **Security**          |                                                                        |
| AdminUserCheck        | Check the admin user isn't called 'admin'                              |

## The following Umbraco v7 checks were removed from the Umbraco v8 version:

| Check                        | Reason                                                        |
| ---------------------------- |:--------------------------------------------------------------|
| **Config**                   |                                                               |
| ExamineRebuildOnStartCheck   | Removed to start with as Examine config has been removed      |
| **Data Integrity**           |                                                               |
| ContentVersionsCheck         | Removed until we understand how to query the content versions |
| **SEO**                      |                                                               |
| LorumIpsumCheck              | Removed until we understand how to search using Examine       |
| XmlSitemapCheck              | Removed until we understand how to get the httpcontext        |
| **Security**                 |                                                               |
| HstsCheck                    | Exists in Core                                                |
| TlsCheck                     | Exists in Core                                                |
| ClientDependencyVersionCheck | Removed because of no current vulnerabilities in Umbraco v8   |

## Suggest Checks

If you would like to suggest checks please raise it as an issue.

## Contribute Checks

If you would like to contribute any checks, please either choose one from the existing issues list, or create an issue first and link to it in the PR using a hashtag and the issue number i.e. #1234
