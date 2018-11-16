# Our.Umbraco.HealthChecks

[![Build status](https://ci.appveyor.com/api/projects/status/okgo4pkpogij6a8g?svg=true)](https://ci.appveyor.com/project/prjseal/our-umbraco-healthchecks)

This repo is a collection of community written [Health Checks](https://our.umbraco.com/Documentation/Extending/Health-Check/) for Umbraco CMS.

Please follow the naming of the folders and checks which are in the [Umbraco Core Health Checks](https://github.com/umbraco/Umbraco-CMS/tree/dev-v7/src/Umbraco.Web/HealthCheck/Checks)

*Don't forget to do a NuGet restore.*

You can login to the website and test the health checks. Here are the login details:

<strong>username:</strong> admin@admin.com<br/>
<strong>password:</strong> 1234567890

## Lorem Ipsum Check

The first check is for checking if there is any Lorem Ipsum Content in the website. It goes under the SEO category. It doesn't have any actions to fix it, it just alerts you to it so you can check on your site.

![](/images/loremipsumcheck.png)


## Suggested Checks

If you are looking to contribute a HealthCheck - here are a few suggestions! Feel free to contribute suggestions too.

### Check for TLS 1.2 or later

### Check whether examine rebuild on start is off

For sites with lots of content, having Examine rebuild enabled on start can really slow down boot time.

### Check for an XML Sitemap

### FCN Mode 

Check that FCN Mode is set to Single or disabled

### Check Image Processor Post processor is installed

Image processor post processor gives potentially huge savings in terms of image download size - by just installing the nuget package.

### Check HSTS is enabled

Release 7.11.0 now includes this - U4-9066 - Three new Security Health Checks

### Check urlCompression is enabled

### Check HTTP Only and Secure cookies
