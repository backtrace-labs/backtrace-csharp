# Backtrace

[Backtrace](http://backtrace.io/)'s integration with C# applications allows customers to capture and report handled and unhandled C# exceptions to their Backtrace instance, instantly offering the ability to prioritize and debug software errors.

## Usage

```csharp
var backtraceClient = new BacktraceClient(credentials);

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


# Supported .NET Frameworks <a name="supported-frameworks"></a>
* .NET Framework 3.5 +
* .NET Framework 4.5 + 
    - getting information about application thread,
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
- On Mac OS X, you can download and install `Visual Studio` [here](https://www.visualstudio.com/downloads/) if you prefer using an IDE. For command line, you should to download and install [.NET Core 2.0 or above](https://www.microsoft.com/net/download/macos).  
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

Visual Studio allows you to build a project and run all available samples (prepared for .NET Core, .NET Framework 4.5, .NET Framework 3.5). 
- Double click `.sln` file or `open` project directory in Visual Studio.
- In `Solution Explorer` navigate to directory `Sample` and set preffered project (.NET Core/Framework) as startup project.

![Visual Studio](https://github.com/backtrace-labs/backtrace-csharp/raw/dev/Backtrace/Documents/Images/VisualStudio.PNG)

- Open `Program.cs` class in any **Backtrace Sample project** and replace `BacktraceCredential` constructor patemeters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:
```csharp
    var backtraceCredentials = new BacktraceCredentials(@"backtrace_endpoint_url", "token");
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
    var backtraceCredentials = new BacktraceCredentials(@"backtrace_endpoint_url", "token");
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

![VisualStudioMacOS](https://github.com/backtrace-labs/backtrace-csharp/raw/dev/Backtrace/Documents/Images/VisualStudioMacOS.PNG)

- Open `Program.cs` class in project **Backtrace.Core** and replace `BacktraceCredential` constructor patemeters with with your `Backtrace endpoint URL` (e.g. https://xxx.sp.backtrace.io:6098) and `submission token`:
```csharp
    var backtraceCredentials = new BacktraceCredentials(@"backtrace_endpoint_url", "token");
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

Additionally and optionally, `BacktraceClient` constructor also accepts the following parameters: **custom attributes**, **database directory path** and **maximum number of error reports per minute**.

```csharp
var backtraceClient = new BacktraceClient(
    sectionName: "BacktraceCredentials",
    attributes: new Dictionary<string, object>() { { "Attribute", "value" } },
    databaseDirectory: "pathToDatabaseDirectory",
    reportPerMin: 0
);
```

Note:
- `databaseDirectory` parameter is optional. Make sure the directory designated is empty. BacktraceClient will use this directory to save additional information relating to program execution. If a `databaseDirectory` path is supplied, the Backtrace library will generate and attach a minidump to each error report automatically.
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
    backtraceClient.Send(backtraceReport);
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
  backtraceClient.Send(exception);
  backtraceClient.Send("Message");
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
- `WhenServerUnavailable` - attach an event handler to be invoked when the server returns with a `400 bad request`, `401 unauthorized` or other HTTP error codes.
- `OnServerAnswer` - attach an event handler to be invoked when the server returns with a valid response.

`BacktraceApi` can send synchronous and asynchronous reports to the Backtrace endpoint. To prepare asynchronous report (default is synchronous) you have to set `AsynchronousRequest` property to `true`.

## BacktraceDatabase  <a name="architecture-BacktraceDatabase"></a>
**`BacktraceDatabase`** is a class stores data in your local harddrive. An `BacktraceDatabase` instance is instantiated when the `BacktraceClient` constructor is called. If `databaseDirectory` isn't set in the `BacktraceClient` constructor call, `BacktraceDatabase` won't generate minidump files. Before start - make sure that the directory designed in **BacktraceClient.databaseDirectory** is **empty**. 

## ReportWatcher  <a name="architecture-ReportWatcher"></a>
**`ReportWatcher`** is a class that validate send requests to the Backtrace endpoint. If `reportPerMin` is set in the `BacktraceClient` constructor call, `ReportWatcher` will drop error reports that go over the limit.



# Good to know <a name="good-to-know"></a>

## Xamarin

You can use this Backtrace library with Xamarin if you change your `HttpClient` Implementation to `Android`. To change `HttpClient` settings, navigate to `Android Options` under `Project Settings` and click on `Advanced` button. 

![Xamarin Android Support][androidSupport]

[androidSupport]: https://github.com/backtrace-labs/backtrace-csharp/raw/dev/Backtrace/Documents/Images/AndroidSupport.PNG "Xamarin Android Support"
