# Async Splunk Logger

[![NuGet Version](http://img.shields.io/nuget/v/Agero.Core.SplunkLogger.Async.Core.svg?style=flat)](https://www.nuget.org/packages/Agero.Core.SplunkLogger.Async.Core/) 
[![NuGet Downloads](http://img.shields.io/nuget/dt/Agero.Core.SplunkLogger.Async.Core.svg?style=flat)](https://www.nuget.org/packages/Agero.Core.SplunkLogger.Async.Core/)

Async Splunk Logger is a **.NET Core (>= v2.1)** library for logging to Splunk using HTTP collector, **asynchronously**. It has in-memory queue and runs background threads to pick logs from queue and submit them to Splunk using [Splunk Logger](https://github.com/agero-core/splunk-logger/) library.

The logging is implemented as a background task that uses [hosted services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host) introduced in .NET Core 2.1.

## Usage:

Register [LogProcessingBackgroundService](./Agero.Core.SplunkLogger.Async.Core/LogProcessingBackgroundService.cs) in the app's dependency injection container with [ConfigureServices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-2.1#configureservices). If you require multiple background threads for processing logs, register additional [LogProcessingBackgroundService](./Agero.Core.SplunkLogger.Async.Core/LogProcessingBackgroundService.cs).

In .NET Core application:
```csharp
var builder = new HostBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<LogProcessingBackgroundService>();  
        services.AddHostedService<LogProcessingBackgroundService>(); 
    })
    .Build();

builder.StartAsync();
```

In ASP.NET Core application:
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHostedService<LogProcessingBackgroundService>();
    services.AddHostedService<LogProcessingBackgroundService>();
}
```

Create instance:
```csharp
IAsyncLogger logger = new AsyncLogger(
    collectorUri: new Uri("<Your Splunk Collector Url>"), 
    authorizationToken: "<Your Splunk Access Token>", 
    applicationName: "TestName", 
    applicationVersion: "1.2.3.4", 
    timeout: 3000);
```
Create log:
```csharp
bool result = logger.Log(
    type: "TestInfo", 
    message: "Test message", 
    data: new { test1 = "test1", test2 = "test2" },
    correlationId: "1234567");	
```

This creates the following log in Splunk:

```json
{  
   "type":"TestInfo",
   "name":"TestName",
   "version":"1.2.3.4",
   "message":"Test message",
   "system":{  
      "userName":"DomainName\\user",
      "userDomainName":"DomainName",
      "operatingSystem":"Unix 18.2.0.0",
      "is64BitOperatingSystem":true,
      "processorCount":4,
      "clrVersion":"4.0.30319.42000",
      "is64BitProcess":false,
      "machineName":"MACHINE-NAME",
      "localTime":"2017-08-16T14:43:07.7658829-04:00",
      "utcTime":"2017-08-16T18:43:07.7668831+00:00",
      "hostName":"MACHINE-NAME",
      "ipAddresses":[  
         "123.123.1.0"
      ]
   },
   "app":{  
      "test1":"test1",
      "test2":"test2"
   },
   "correlationId": "1234567"
}
```

## Running Tests:

Create the json file **logger-settings.json** with the below configuration under the projects **Agero.Core.SplunkLogger.Async.Core.Tests** and **Agero.Core.SplunkLogger.Async.Core.Web**.

```json
{
  "splunkCollectorUrl": "<Your Splunk Collector Url>",
  "authenticationToken": "<Your Splunk Access Token>"
}
```
