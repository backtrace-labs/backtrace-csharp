# Backtrace

[Backtrace](http://backtrace.io/)'s integration with C# applications allows customers to capture and report handled and unhandled C# exceptions to their Backtrace instance, instantly offering the ability to prioritize and debug software errors.

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
    backtraceClient.Send(new BacktraceReport(exception));
}
```

# Table of contents
1. [Supported .NET Frameworks](#supported-frameworks)
2. [Installation](#installation)
    1. [Prerequisites](#installation-before-start)
    2. [NuGet installation](#installation-nuget)
3. [Running sample application](#sample-app)
    1. [Visual Studio](#sample-app-vs)
    2. [.NET Core CLI](#sample-app-cli)
    3. [Visual Studio for Mac](#sample-app-vs-mac)
4. [Documentation](#documentation)
    1. [Initialize new BacktraceClient](#documentation-initialization)
    2. [Sending a report](#documentation-sending-report)
    3. [Events](#documentation-events)
    4. [Customization](#documentation-customization)
5. [Architecture](#architecture)
    1. [BacktraceReport](#architecture-BacktraceReport)
    2. [BacktraceClient](#architecture-BacktraceClient)
    3. [BacktraceData](#architecture-BacktraceData)
    4. [BacktraceApi](#architecture-BacktraceApi)
    5. [BacktraceDatabase](#architecture-BacktraceDatabase)
    6. [ReportWatcher](#architecture-ReportWatcher)
6. [Good to know](#good-to-know)
7. [Release Notes](#changelog)

# Supported .NET Frameworks <a name="supported-frameworks"></a>
* .NET Framework 3.5 +
* .NET Framework 4.5 + 
    - getting information about application thread
    - handling unhandled application exceptions
* .NET Standard:
  * .NET Core 2.0 application
  * Xamarin
  * Universal Windows Platform
* Unity

# Installation <a name="installation"></a>

## Prerequisites <a name="installation-before-start"></a>

### Development Environment
- On `Windows`, we recommend `Visual Studio 2017` or above for IDE. You can download and install `Visual Studio` [here](https://www.visualstudio.com/downloads/). As an alternative to `Visual Studio` you can use .NET Core command line interface, see installation guide [here](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)
- On `Mac OS X`, you can download and install `Visual Studio` [here](https://www.visualstudio.com/downloads/) if you prefer using an IDE. For command line, you should to download and install [.NET Core 2.0 or above](https://www.microsoft.com/net/download/macos).  
- On `Linux`, [Visual Studio Code](https://code.visualstudio.com/) is available as a light-weight IDE. Similarly, you can use .NET Core command line interface, see instruction for `Linux` [here](https://docs.microsoft.com/en-US/dotnet/core/linux-prerequisites?tabs=netcore2x)

### NuGet  

The `Backtrace` library is available via NuGet. You can read more about NuGet and how to download the packages [here](https://docs.microsoft.com/en-us/nuget/)

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
- Double click `.sln` file or `open` project directory in Visual Studio.
- In `Solution Explorer` navigate to directory `Sample` and set preffered project (.NET Core/Framework) as startup project.

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
- Open `Program.cs` in project **Backtrace.Core** and replace `BacktraceCredential` constructor patemeters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:
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



## Visual Studio for MacOS  <a name="sample-app-vs-mac"></a>

- Open the **Backtrace** solution in Visual Studio, unload all projects except **Backtrace**, **Backtrace.Tests** and **Backtrace.Core**, and set **Backtrace.Core** as your startup project:

![VisualStudioMacOS](https://github.com/backtrace-labs/backtrace-csharp/raw/master/Backtrace/Documents/Images/VisualStudioMacOS.PNG)

- Open `Program.cs` class in project **Backtrace.Core** and replace `BacktraceCredential` constructor patemeters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:
```csharp
    var backtraceCredentials = new BacktraceCredentials(@"https://myserver.sp.backtrace.io:6097", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0");
```

- Build the project. 
- Upon successful build, run the project.
- You should see new errors in your Backtrace instance. Refresh the Project page or Query Builder to see new details in real-time.



# Documentation  <a name="documentation"></a>

## Initialize a new BacktraceClient <a name="documentation-initialization"></a>

First create a `BacktraceCredential` instance with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`, and supply it as a parameter in the `BacktraceClient` constructor:

```csharp
var credentials = new BacktraceCredentials("backtrace_endpoint_url", "token");
var backtraceClient = new BacktraceClient(credentials);
```

Additionally and optionally, `BacktraceClient` takes a configuration object called `BacktraceClientConfiguration` as a parameter. You can use this class to set all your client options and initialize new `BacktraceClient` 

```csharp
var configuration = new BacktraceClientConfiguration(credentials, "databaseDirectory");
var backtraceClient = new BacktraceClient(configuration);

```

`BacktraceClientConfiguration` constructor also accepts the following parameters: **ClientAttributes**, **TlsLegacySupport** and **ReportPerMin**.


#### TLS/SSL Support

For .NET Standard 2.0 and .NET Framework 4.6+, TLS 1.2 support is built-in.

For .NET Framework 4.5 (and below) as well as .NET Standard 2.0 (and below), TLS 1.2 support may not be available, but you can use still enable lower TLS/SSL support by supplying `tlsLegacySupport` parameter to `BacktraceClient` constructor, like so:
```csharp

var configuration = new BacktraceClientConfiguration(credentials, "databaseDirectory")
{
    TlsLegacySupport = true,
    ReportPerMin = 0
};
var backtraceClient = new BacktraceClient(configuration);

```


Notes:
- `databaseDirectory` parameter is optional. If a `databaseDirectory` path is supplied, the Backtrace library will generate and attach a minidump to each error report automatically. Otherwise, `BacktraceDatabase` will be disabled.
- You can initialize `BacktraceClient` with `BacktraceDatabaseSettings` or you can pass your own implementation of offline database. 
- If parameter `reportPerMin` is equal to 0, there is no limit on the number of error reports per minute. When an error is submitted when the `reportPerMin` cap is reached, `BacktraceClient.Send` method will return false.


## Sending an error report <a name="documentation-sending-report"></a>

`BacktraceClient.Send` method will send an error report to the Backtrace endpoint specified. There `Send` method is overloaded, see examples below:

### Using BacktraceReport

The `BacktraceReport` class extends `BacktraceReportBase` and represents a single error report. (Optional) You can also submit custom attributes using the `attributes` parameter, or attach files by supplying an array of file paths in the `attachmentPaths` parameter.

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
Note:
- if you initialize `BacktraceClient` with `BacktraceDatabase` and your application is offline or you pass invalid credentials to `BacktraceClient`, reports will be stored in database directory path,
- for .NET 4.5> we recommend to use `SendAsync` method

#### Asynchronous Send Support

For developers that use .NET 4.5+ and .NET Standard we recommend using `SendAsync` method, which uses asynchourous Tasks. Both `Send` and `SendAsync` method returns `BacktraceResult`. See example below:

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
    (Model.BacktraceData<object> model) =>
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


## Custom client and report classes <a name="documentation-customization"></a>

You can extend `BacktraceReportBase` and `BacktraceBase` to create your own Backtrace client and error report implementation. You can refer to `BacktraceClient` and `BacktraceReport` for implementation inspirations. 

# Architecture  <a name="architecture"></a>

## BacktraceReport  <a name="architecture-BacktraceReport"></a>
**`BacktraceReport`** is a class that describe a single error report that extends `BacktraceReportBase` generic class. Argument `T` is value type of `Attribute` dictionary. Keep in mind that `BacktraceClient` uses `CallingAssembly` method to retrieve information about your application.  

## BacktraceClient  <a name="architecture-BacktraceClient"></a>
**`BacktraceClient`** is a class that allows you to instantiate a client instance that interacts with `BacktraceApi`. This class sets up connection to the Backtrace endpoint and manages error reporting behavior (for example, saving minidump files on your local hard drive and limiting the number of error reports per minute). `BacktraceClient` extends `BacktraceBase` generic class. `T` argument is a value type in `Attribute` dictionary.

## BacktraceData  <a name="architecture-BacktraceData"></a>
**`BacktraceData`** is a generic, serializable class that holds the data to create a diagnostic JSON to be sent to the Backtrace endpoint via `BacktraceApi`. You can add additional pre-processors for `BacktraceData` by attaching an event handler to the `BacktraceClient.BeforeSend` event. `BacktraceData` require `BacktraceReport` and `BacktraceClient` client attributes.

## BacktraceApi  <a name="architecture-BacktraceApi"></a>
**`BacktraceApi`** is a class that sends diagnostic JSON to the Backtrace endpoint. `BacktraceApi` is instantiated when the `BacktraceClient` constructor is called. You use the following event handlers in `BacktraceApi` to customize how you want to handle JSON data:
- `RequestHandler` - attach an event handler to this event to override the default `BacktraceApi.Send` method. A `RequestHandler` handler requires 3 parameters - `uri`, `header` and `formdata` bytes. Default `Send` method won't execute when a `RequestHandler` handler is attached.
- `OnServerError` - attach an event handler to be invoked when the server returns with a `400 bad request`, `401 unauthorized` or other HTTP error codes.
- `OnServerResponse` - attach an event handler to be invoked when the server returns with a valid response.

`BacktraceApi` can send synchronous and asynchronous reports to the Backtrace endpoint. To enable asynchronous report (default is synchronous) you have to set `AsynchronousRequest` property to `true`.

## BacktraceResult  <a name="architecture-BacktraceResult"></a>
**`BacktraceResult`** is a class that holds response and result from a `Send` or `SendAsync` call. The class contains a `Status` property that indicates whether the call was completed (`OK`), the call returned with an error (`ServerError`), or the call was aborted because client reporting limit was reached (`LimitReached`). Additionally, the class has a `Message` property that contains details about the status. Note that the `Send` call may produce an error report on an inner exception, in this case you can find an additional `BacktraceResult` object in the `InnerExceptionResult` property.

## BacktraceDatabase  <a name="architecture-BacktraceDatabase"></a>
**`BacktraceDatabase`** is a class stores data in your local hard drive. You can intiailize new `BacktraceDatabase` or you can let `BacktraceClient` create new database object when constructor is called. `BacktraceClient` use `BacktraceDatabase` `Start` method by default in  constructor. That means when you pass `BacktraceDatabase` object to client, we will prepare database to work with our API. If `databaseDirectory` isn't set in the `BacktraceClient`  constructor call or `BacktraceDatabaseSettings` object, `BacktraceDatabase` won't generate minidump files and store data. 

`BacktraceDatabase` use timer to periodically send reports stored in memory cache database - `BacktraceDatabaseContext`. If single report fails to send, `BacktraceDatabase` increase retry time variable for all reports in `BacktraceDatabaseContext`. `BacktraceDatabase` generate temporary file for each `Send` or `SendAsync` method invoke. If `BacktraceDatabase` correctly save file on hard drive, we rename file to valid name. 

`BacktraceDatabase` remove all orphaned files from database directory path. Make sure you prepare valid database directory before you start using `BacktraceClient` with `BacktraceDatabase` feature!

If you retrieve `BacktraceDatabaseEntry` from `BacktraceDatabaseContext`, context will mark entry as a locked. In this case `FirstOrDefault` or `LastOrDefault` method will return first or last not locked entry. `BacktraceDatabaseContext` allows you to use FIFO or LIFO data storage.

If you want to clear your database or remove all reports after send method you can use `Clear`, `Flush` and `FlushAsync` methods.

## ReportWatcher  <a name="architecture-ReportWatcher"></a>
**`ReportWatcher`** is a class that validate send requests to the Backtrace endpoint. If `reportPerMin` is set in the `BacktraceClient` constructor call, `ReportWatcher` will drop error reports that go over the limit. `BacktraceClient` check rate limit before `BacktraceApi` generate diagnostic json. 


# Good to know <a name="good-to-know"></a>

## Xamarin

You can use this Backtrace library with Xamarin if you change your `HttpClient` Implementation to `Android`. To change `HttpClient` settings, navigate to `Android Options` under `Project Settings` and click on `Advanced` button. 

![Xamarin Android Support][androidSupport]

[androidSupport]: https://github.com/backtrace-labs/backtrace-csharp/raw/master/Backtrace/Documents/Images/AndroidSupport.PNG "Xamarin Android Support"


# Release Notes <a name="changelog"></a>

See release notes [here](./CHANGELOG.md).
