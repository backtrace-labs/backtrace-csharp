# Backtrace

[![Backtrace@release](https://img.shields.io/badge/Backtrace%40master-2.1.9-blue.svg)](https://www.nuget.org/packages/Backtrace)
[![Build status](https://ci.appveyor.com/api/projects/status/o0n9sp0ydgxb3ktu?svg=true)](https://ci.appveyor.com/project/konraddysput/backtrace-csharp)


[Backtrace](http://backtrace.io/)'s integration with C# applications allows customers to capture and report handled and unhandled C# exceptions to their Backtrace instance, instantly offering the ability to prioritize and debug software errors.

[github release]: (https://github.com/backtrace-labs/backtrace-csharp/)

## Usage

```csharp
// replace with your endpoint url and token
var backtraceCredentials =
	new BacktraceCredentials(@"https://myserver.sp.backtrace.io:6097", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0");
var backtraceClient = new BacktraceClient(backtraceCredentials);

try{
    //throw exception here
}
catch(Exception exception){
    await backtraceClient.SendAsync(new BacktraceReport(exception));
}
```

# Table of contents

1. [Features Summary](#features-summary)
2. [Supported .NET Frameworks](#supported-frameworks)
3. [Installation](#installation)
   1. [Prerequisites](#installation-before-start)
   2. [NuGet installation](#installation-nuget)
4. [Running sample application](#sample-app)
   1. [Visual Studio](#sample-app-vs)
   2. [.NET Core CLI](#sample-app-cli)
   3. [Visual Studio for Mac](#sample-app-vs-mac)
5. [Documentation](#documentation)
   1. [Initialize new BacktraceClient](#documentation-initialization)
      - [Database Initialization](#documentation-database-initialization)
   2. [Sending a report](#documentation-sending-report)
   3. [Events](#documentation-events)
   4. [Customization](#documentation-customization)
6. [Architecture](#architecture)
   1. [BacktraceReport](#architecture-BacktraceReport)
   2. [BacktraceClient](#architecture-BacktraceClient)
   3. [BacktraceData](#architecture-BacktraceData)
   4. [BacktraceApi](#architecture-BacktraceApi)
   5. [BacktraceDatabase](#architecture-BacktraceDatabase)
   6. [ReportWatcher](#architecture-ReportWatcher)
7. [Good to know](#good-to-know)
8. [Release Notes](#changelog)

# Features Summary <a name="features-summary"></a>

- Light-weight C# client library that quickly submits C#/.NET exceptions and crashes to your Backtrace dashboard
  - Can include callstack, system metadata, custom metadata, and file attachments (e.g. minidump) if needed.
- Supports a wide range of .NET versions such as .NET Framework, .NET Core, Mono, Xamarin and Unity. Read more [here](#supported-frameworks)
- Supports both CLI and IDE work environments
- Supports asynchronous Tasks in .NET 4.5+
- Supports offline database for error report storage and re-submission in case of network outage
- Fully customizable and extendable event handlers and base classes for custom implementations
- Available as a [NuGet Package](https://www.nuget.org/packages/Backtrace/) as well as a fully open-sourced [Github Release](https://github.com/backtrace-labs/backtrace-csharp/).

# Supported .NET Frameworks <a name="supported-frameworks"></a>

- .NET Framework 3.5 +
- .NET Framework 4.5 +
  - getting information about application thread
  - handling unhandled application exceptions
- .NET Standard:
  - .NET Core 2.0 application
  - Xamarin
  - Universal Windows Platform
- Unity

# Installation <a name="installation"></a>

## Prerequisites <a name="installation-before-start"></a>

### Development Environment

- On **Windows**, we recommend **Visual Studio 2017** or above for IDE. You can download and install **Visual Studio** [here](https://www.visualstudio.com/downloads/). As an alternative to `Visual Studio` you can use .NET Core command line interface, see installation guide [here](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)
- On **Mac OS X**, you can download and install **Visual Studio** [here](https://www.visualstudio.com/downloads/) if you prefer using an IDE. For command line, you should to download and install [.NET Core 2.0 or above](https://www.microsoft.com/net/download/macos).
- On **Linux**, [Visual Studio Code](https://code.visualstudio.com/) is available as a light-weight IDE. Similarly, you can use .NET Core command line interface, see instruction for **Linux** [here](https://docs.microsoft.com/en-US/dotnet/core/linux-prerequisites?tabs=netcore2x)

### NuGet

The [**Backtrace** library](https://www.nuget.org/packages/Backtrace/) is available on NuGet. You can read more about NuGet and how to download the packages [here](https://docs.microsoft.com/en-us/nuget/)

## Installing Backtrace via NuGet <a name="installation-nuget"></a>

You can install Backtrace via NuGet using the following commands:

Windows NuGet CLI:

```
Install-Package Backtrace
```

Linux/Mac OS X .NET Core CLI:

```
dotnet add package Backtrace
```

# Running sample application <a name="sample-app"></a>

## Visual Studio <a name="sample-app-vs"></a>

Visual Studio allows you to build a project and run all available samples (includes support for .NET Core, .NET Framework 4.5, .NET Framework 3.5).

- Double click `.sln` file or **open** project directory in Visual Studio.
- In **Solution Explorer** navigate to directory `Sample` and set preferred project (.NET Core/Framework) as startup project.

![Visual Studio](https://github.com/backtrace-labs/backtrace-csharp/raw/master/Backtrace/Documents/Images/VisualStudio.PNG)

- Open `Program.cs` class in any **Backtrace Sample project** and replace `BacktraceCredential` constructor patemeters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:

```csharp
    var backtraceCredentials = new BacktraceCredentials(@"https://myserver.sp.backtrace.io:6097", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0");
```

- Press `Ctrl+Shift+B` to `build` solution
- Press `F5` to run the project
- You should see new errors in your Backtrace instance. Refresh the Project page or Query Builder to see new details in real-time.

## .NET Core command line <a name="sample-app-cli"></a>

You can use .NET Core's CLI to run sample project on Windows, Mac OS and Linux. To run a sample project using .NET Core CLI:

- While in solution directory, navigate to **Backtrace.Core** sample application:

```
    cd Backtrace.Core
```

- Open `Program.cs` in project **Backtrace.Core** and replace `BacktraceCredential` constructor parameters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:

```csharp
    var backtraceCredentials = new BacktraceCredentials(@"https://myserver.sp.backtrace.io:6097", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0");
```

- Build the project:

```
    dotnet build
```

- When the build completes, run the project:

```
    dotnet run
```

- You should see new errors in your Backtrace instance. Refresh the Project page or Query Builder to see new details in real-time.

## Visual Studio for MacOS <a name="sample-app-vs-mac"></a>

- Open the **Backtrace** solution in Visual Studio, unload all projects except **Backtrace**, **Backtrace.Tests** and **Backtrace.Core**, and set **Backtrace.Core** as your startup project:

![VisualStudioMacOS](https://github.com/backtrace-labs/backtrace-csharp/raw/master/Backtrace/Documents/Images/VisualStudioMacOS.PNG)

- Open `Program.cs` class in project **Backtrace.Core** and replace `BacktraceCredential` constructor parameters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:

```csharp
    var backtraceCredentials = new BacktraceCredentials(@"https://myserver.sp.backtrace.io:6097", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0");
```

- Build the project.
- Upon successful build, run the project.
- You should see new errors in your Backtrace instance. Refresh the Project page or Query Builder to see new details in real-time.

# Documentation <a name="documentation"></a>

## Initialize a new BacktraceClient <a name="documentation-initialization"></a>

First create a `BacktraceCredential` instance with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`, and supply it as a parameter in the `BacktraceClient` constructor:

```csharp
var credentials = new BacktraceCredentials("backtrace_endpoint_url", "token");
var backtraceClient = new BacktraceClient(credentials);
```

For more advanced usage of `BacktraceClient`, you can supply `BacktraceClientConfiguration` as a parameter. See the following example:

```csharp
var credentials = new BacktraceCredentials("backtrace_endpoint_url", "token");
var configuration = new BacktraceClientConfiguration(credentials){
    ClientAttributes = new Dictionary<string, object>() {
        { "attribute_name", "attribute_value" } },
    ReportPerMin = 3,
}
var backtraceClient = new BacktraceClient(configuration);
```

For more information on `BacktraceClientConfiguration` parameters please see <a href="#architecture-BacktraceClient">Architecture</a> section.

Notes:

- If parameter `reportPerMin` is equal to 0, there is no limit on the number of error reports per minute. When the `reportPerMin` cap is reached, `BacktraceClient.Send/BacktraceClient.SendAsync` method will return false,
- If you develop application behind the proxy you can pass `WebProxy` object to `BacktraceCredentials` object. We will try to use `WebProxy` object when user pass it to `Backtrace`. To setup proxy use property `Proxy`,
- `BacktraceClient` allows you to unpack `AggregateExceptions` and send only exceptions that are available in `InnerException` property of `AggregateException`. By default `BacktraceClient` will send `AggregateException` information to Backtrace server. To avoid sending these reports, please override `UnpackAggregateException` and set value to `true`.

#### Database initialization <a name="documentation-database-initialization"></a>

`BacktraceClient` allows you to customize the initialization of `BacktraceDatabase` for local storage of error reports by supplying a `BacktraceDatabaseSettings` parameter, as follows:

```csharp
var dbSettings = new BacktraceDatabaseSettings("databaseDirectory"){
    MaxRecordCount = 100,
    MaxDatabaseSize = 1000,
    AutoSendMode = true,
    RetryBehavior = Backtrace.Types.RetryBehavior.ByInterval
};
var database = new BacktraceDatabase(dbSettings);
var credentials = new BacktraceCredentials("backtrace_endpoint_url", "token");
var configuration = new BacktraceClientConfiguration(credentials);
var backtraceClient = new BacktraceClient(configuration, database);
```

Notes:

- If a valid `databaseDirectory` directory is supplied, the Backtrace library will generate and attach a minidump to each error report automatically. Otherwise, `BacktraceDatabase` will be disabled,
- You can set `backtraceClient.MiniDumpType` to `MiniDumpType.None` if you don't want to generate minidump files.

#### Deduplication

Backtrace C# library allows you to aggregate the same reports. By using Backtrace deduplication mechanism you can aggregate the same reports and send only one message to Backtrace Api. As a developer you can choose deduplication options. Please use `DeduplicationStrategy` enum to setup possible deduplication rules or copy example below to setup deduplication strategy:

```csharp
var dbSettings = new BacktraceDatabaseSettings(path)
{
    DeduplicationStrategy = DeduplicationStrategy.LibraryName | DeduplicationStrategy.Classifier | DeduplicationStrategy.Message,
}
```

Deduplication strategy enum types:

- Ignore - ignore deduplication strategy,
- Default - deduplication strategy will only use current strack trace to find duplicated reports,
- Classifier - deduplication strategy will use stack trace and exception type to find duplicated reports,
- Message - deduplication strategy will use stack trace and exception message to find duplicated reports,
- LibraryName - deduplication strategy will use stack trace and faulting library name to find duplicated reports.

To combine all possible deduplication strategies please use code below:

```csharp
DeduplicationStrategy = DeduplicationStrategy.LibraryName | DeduplicationStrategy.Classifier | DeduplicationStrategy.Message
```

Notes:

- When you aggregate reports via Backtrace C# library, `BacktraceDatabase` will store number of the same reports in `counter` file.
- Deduplication algorithm will include `BacktraceReport` `Fingerprint` and `Factor` properties. `Fingerprint` property will overwrite deduplication algorithm result. `Factor` property will change hash generated by deduplication algorithm.
- By storing data in additional counter file we can read number of the same offline reports on application starts and send them to Backtrace when your internet connection back.
- When C# library aggregate multiple reports into one diagnostic data, application will send only one request, not multiple,
- `BacktraceDatabase` methods allows you to use aggregated diagnostic data together. You can check `Hash` property of `BacktraceDatabaseRecord` to check generated hash for diagnostic data and `Counter` to check how much the same records we detect.
- `BacktraceDatabase` `Count` method will return number of all records stored in database (included deduplicated records),
- `BacktarceDatabase` `Delete` method will remove record (with multiple deduplicated records) at the same time.
- You can override default hash method by using `GenerateHash` delegate available in `BacktraceDatabase` object. When you add your own method implementation, `BacktraceDatabase` won't use default deduplication mechanism.

## Sending an error report <a name="documentation-sending-report"></a>

`BacktraceClient.Send/BacktraceClient.SendAsync` method will send an error report to the Backtrace endpoint specified. There `Send` method is overloaded, see examples below:

### Using BacktraceReport

The `BacktraceReport` class represents a single error report. (Optional) You can also submit custom attributes using the `attributes` parameter, or attach files by supplying an array of file paths in the `attachmentPaths` parameter.

```csharp
try
{
  //throw exception here
}
catch (Exception exception)
{
    var report = new BacktraceReport(
        exception: exception,
        attributes: new Dictionary<string, object>() { { "key", "value" } },
        attachmentPaths: new List<string>() { @"file_path_1", @"file_path_2" }
    );
    var result = backtraceClient.Send(backtraceReport);
}
```

Notes:

- if you initialize `BacktraceClient` with `BacktraceDatabase` and your application is offline or you pass invalid credentials to `BacktraceClient`, reports will be stored in database directory path,
- for .NET 4.5+, we recommend to use `SendAsync` method,
- if you don't want to use reflection to determine valid stack frame method name, you can pass `false` to `reflectionMethodName`. By default this value is equal to `true`,
- `BacktraceReport` allows you to change default fingerprint generation algorithm. You can use `Fingerprint` property if you want to change fingerprint value. Keep in mind - fingerprint should be valid sha256 string,
- `BacktraceReport` allows you to change grouping strategy in Backtrace server. If you want to change how algorithm group your reports in Backtrace server please override `Factor` property,
- `Fingerprint` will overwrite `BacktraceReport` deduplication strategy! In this case two different reports will use the same hash in deduplication algorithm, that could cause data lost,
- `Factor` can change a result from deduplication algorithm. Hash generated by deduplication model properties will include `Factor` value.

If you want to use `Fingerprint` and `Factor` property you have to override default property values. See example below to check how to use these properties:

```
try
{
  //throw exception here
}
catch (Exception exception)
{
    var report = new BacktraceReport(...){
        FingerPrint = "sha256 string",
        Factor = exception.GetType().Name
    };
    ....
}

```

#### Asynchronous Send Support

For developers that use .NET 4.5+ and .NET Standard we recommend using `SendAsync` method, which uses asynchronous Tasks. Both `Send` and `SendAsync` method returns `BacktraceResult`. See example below:

```csharp
try
{
  //throw exception here
}
catch (Exception exception)
{
    var report = new BacktraceReport(
        exception: exception,
        attributes: new Dictionary<string, object>() { { "key", "value" } },
        attachmentPaths: new List<string>() { @"file_path_1", @"file_path_2" }
    );
    var result = await backtraceClient.SendAsync(backtraceReport);
}
```

### Other BacktraceReport Overloads

`BacktraceClient` can also automatically create `BacktraceReport` given an exception or a custom message using the following overloads of the `BacktraceClient.Send` method:

```csharp
try
{
  //throw exception here
}
catch (Exception exception)
{
  //use extension method
  var report = exception.ToBacktraceReport();
  backtraceClient.Send(report);

  //pass exception to Send method
  backtraceClient.Send(exception);

  //pass your custom message to Send method
  await backtraceClient.SendAsync("Message");
}
```

## Attaching custom event handlers <a name="documentation-events"></a>

`BacktraceClient` allows you to attach your custom event handlers. For example, you can trigger actions before the `Send` method:

```csharp

 //Add your own handler to client API

backtraceClient.BeforeSend =
    (Model.BacktraceData model) =>
    {
        var data = model;
        //do something with data for example:
        data.Attributes.Add("eventAtrtibute", "EventAttributeValue");
        if(data.Classifier == null || !data.Classifier.Any())
        {
            data.Attachments.Add("path to attachment");
        }

        return data;
    };
```

`BacktraceClient` currently supports the following events:

- `BeforeSend`
- `AfterSend`
- `RequestHandler`
- `OnReportStart`
- `OnClientReportLimitReached`
- `OnUnhandledApplicationException`
- `OnServerResponse`
- `OnServerError`

## Reporting unhandled application exceptions

`BacktraceClient` also supports reporting of unhandled application exceptions not captured by your try-catch blocks. To enable reporting of unhandled exceptions:

```csharp
backtraceClient.HandleApplicationException();
```

Unhandled application exception handler will store your report in database. In case if you won't see your report in Backtrace, you will have to relaunch your application.

## Custom client and report classes <a name="documentation-customization"></a>

You can extend `BacktraceBase` to create your own Backtrace client and error report implementation. You can refer to `BacktraceClient` for implementation inspirations.

# Architecture <a name="architecture"></a>

## BacktraceReport <a name="architecture-BacktraceReport"></a>

**`BacktraceReport`** is a class that describe a single error report. Keep in mind that `BacktraceClient` uses `CallingAssembly` method to retrieve information about your application.

## BacktraceClient <a name="architecture-BacktraceClient"></a>

**`BacktraceClient`** is a class that allows you to instantiate a client instance that interacts with `BacktraceApi`. This class sets up connection to the Backtrace endpoint and manages error reporting behavior (for example, saving minidump files on your local hard drive and limiting the number of error reports per minute). `BacktraceClient` extends `BacktraceBase` class.

`BacktraceClient` takes a `BacktraceClientConfiguration` parameter, which has the following properties:

- `Credentials` - the `BacktraceCredentials` object to use for connection to the Backtrace server.
- `ClientAttributes` - custom attributes to be submitted to Backtrace alongside the error report.
- `ReportPerMin` - A cap on the number of reports that can be sent per minute. If `ReportPerMin` is equal to zero then there is no cap.

## BacktraceData <a name="architecture-BacktraceData"></a>

**`BacktraceData`** is a serializable class that holds the data to create a diagnostic JSON to be sent to the Backtrace endpoint via `BacktraceApi`. You can add additional pre-processors for `BacktraceData` by attaching an event handler to the `BacktraceClient.BeforeSend` event. `BacktraceData` require `BacktraceReport` and `BacktraceClient` client attributes.

## BacktraceApi <a name="architecture-BacktraceApi"></a>

**`BacktraceApi`** is a class that sends diagnostic JSON to the Backtrace endpoint. `BacktraceApi` is instantiated when the `BacktraceClient` constructor is called. You use the following event handlers in `BacktraceApi` to customize how you want to handle JSON data:

- `RequestHandler` - attach an event handler to this event to override the default `BacktraceApi.Send` method. A `RequestHandler` handler requires 3 parameters - `uri`, `header` and `formdata` bytes. Default `Send` method won't execute when a `RequestHandler` handler is attached.
- `OnServerError` - attach an event handler to be invoked when the server returns with a `400 bad request`, `401 unauthorized` or other HTTP error codes.
- `OnServerResponse` - attach an event handler to be invoked when the server returns with a valid response.

`BacktraceApi` can send synchronous and asynchronous reports to the Backtrace endpoint. To enable asynchronous report (default is synchronous) you have to set `AsynchronousRequest` property to `true`.

## BacktraceResult <a name="architecture-BacktraceResult"></a>

**`BacktraceResult`** is a class that holds response and result from a `Send` or `SendAsync` call. The class contains a `Status` property that indicates whether the call was completed (`OK`), the call returned with an error (`ServerError`), the call was aborted because client reporting limit was reached (`LimitReached`), or the call wasn't needed because developer use `UnpackAggregateException` property with empty `AggregateException` object (`Empty`). Additionally, the class has a `Message` property that contains details about the status. Note that the `Send` call may produce an error report on an inner exception, in this case you can find an additional `BacktraceResult` object in the `InnerExceptionResult` property.

## BacktraceDatabase <a name="architecture-BacktraceDatabase"></a>

**`BacktraceDatabase`** is a class that stores error report data in your local hard drive. If `DatabaseSettings` dones't contain a **valid** `DatabasePath` then `BacktraceDatabase` won't generate minidump files and store error report data.

`BacktraceDatabase` stores error reports that were not sent successfully due to network outage or server unavailability. `BacktraceDatabase` periodically tries to resend reports
cached in the database. In `BacktraceDatabaseSettings` you can set the maximum number of entries (`MaxRecordCount`) to be stored in the database. The database will retry sending
stored reports every `RetryInterval` seconds up to `RetryLimit` times, both customizable in the `BacktraceDatabaseSettings`.

`BacktraceDatabaseSettings` has the following properties:

- `DatabasePath` - the local directory path where `BacktraceDatabase` stores error report data when reports fail to send
- `MaxRecordCount` - Maximum number of stored reports in Database. If value is equal to `0`, then there is no limit.
- `MaxDatabaseSize` - Maximum database size in MB. If value is equal to `0`, there is no limit.
- `AutoSendMode` - if the value is `true`, `BacktraceDatabase` will automatically try to resend stored reports. Default is `false`.
- `RetryBehavior` - - `RetryBehavior.ByInterval` - Default. `BacktraceDatabase` will try to resend the reports every time interval specified by `RetryInterval`. - `RetryBehavior.NoRetry` - Will not attempt to resend reports
- `RetryInterval` - the time interval between retries, in seconds.
- `RetryLimit` - the maximum number of times `BacktraceDatabase` will attempt to resend error report before removing it from the database.

If you want to clear your database or remove all reports after send method you can use `Clear`, `Flush` and `FlushAsync` methods.

## ReportWatcher <a name="architecture-ReportWatcher"></a>

**`ReportWatcher`** is a class that validate send requests to the Backtrace endpoint. If `reportPerMin` is set in the `BacktraceClient` constructor call, `ReportWatcher` will drop error reports that go over the limit. `BacktraceClient` check rate limit before `BacktraceApi` generate diagnostic json.

# Good to know <a name="good-to-know"></a>

## Xamarin

You can use this Backtrace library with Xamarin if you change your `HttpClient` Implementation to `Android`. To change `HttpClient` settings, navigate to `Android Options` under `Project Settings` and click on `Advanced` button.

![Xamarin Android Support][androidsupport]

[androidsupport]: https://github.com/backtrace-labs/backtrace-csharp/raw/master/Backtrace/Documents/Images/AndroidSupport.PNG "Xamarin Android Support"

# Release Notes <a name="changelog"></a>

See release notes [here](./CHANGELOG.md).
