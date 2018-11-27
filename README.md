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

## Current Checks

### Azure

#### Azure Examine Compatibility Check

Checks that examine settings are appropriate for the Azure platform.

#### Azure File Change Notification Config Check

Checks that fcnMode config is appropriate for the Azure platform.

#### Azure Logging Check

Checks that logging patterns are appropriate for the Azure platform.

#### Azure Temp Storage Config Check

Checks that temp storage config is appropriate for the Azure platform.

### Config

#### Examine Rebuild On Startup

Check whether examine rebuild on start is off

#### Umbraco Path Check

Checks to see if you have changed the umbraco path.

### Security

#### Admin User Check

Check the admin user isn't called 'admin'

#### TLS Check

Check the TLS protocol being used

## Suggest Checks

If you would like to suggest checks please raise it as an issue.

## Contribute Checks

If you would like to contribute any checks, please either choose one from the existing issues list, or create an issue first and link to it in the PR using a hashtag and the issue number i.e. #1234
