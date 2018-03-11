# backtrace-Csharp

[Backtrace](http://backtrace.io/) error reporting tool for C#.

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
    1. [Before start](#installation-before-start)
    2. [Nuget installation](#installation-nuget)
3. [Running sammple application](#sample-app)
    1. [.NET Core CLI](#sample-app-cli)
    2. [Visual Studio for Mac](#sample-app-vs-mac)
    2. [Visual Studio](#sample-app-vs)
4. [Documentation](#documentation)
    1. [Initialize new BacktraceClient](#documentation-initialization)
    2. [Sending a report](#documentation-sending-report)
    3. [Events](#documentation-events)
    4. [Customization](#documentation-customization)
5. [Good to know](#good-to-know)


# Supported .NET Frameworks <a name="supported-frameworks"></a>
* .NET Framework 3.5 >
* .NET Framework 4.5 > (new cool features) 
* .NET Standard:
  * .NET Core 2.0 application
  * Xamarin
  * Universal Windows Platform
* Unity

# Installation <a name="installation"></a>

## Before start <a name="installation-before-start"></a>

### Development Environment
- On `Windows` we suggest to use `Visual Studio 2017`. You can download and install `Visual Studio` from [link above](https://www.visualstudio.com/downloads/). If you don't want to use `Visual Studio` you can use .NET Core environment from `cmd` or any other terminal. You can check installation guide available [here](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)
- To work with library on `Linux`, you need to prepare your development environment to work with `.NET Core`. You can find installation steps for `.NET Core` on `Linux` environment on [link above](https://docs.microsoft.com/en-US/dotnet/core/linux-prerequisites?tabs=netcore2x)
- In order to use **backtrace-csharp** on Mac OS X, you need to download and install [.NET Core 2.0 or above](https://www.microsoft.com/net/download/macos).  

### Nuget  
`Backtrace-csharp` library is available via nuget. You can read more about nuget and how to download package on [link above](https://docs.microsoft.com/en-us/nuget/)



## Install library via Nuget <a name="installation-nuget"></a>

You can install library via Nuget package

Windows: 
```
Install-Package Backtrace.Csharp
```
Linux/MacOS/.NET CLI:
```
dotnet add package Backtrace.Csharp
```


# Running sammple application <a name="sample-app"></a>

## .NET Core Command line <a name="sample-app-cli"></a>
You can use `CLI` to run sample project on `Linux`, `Windows` and `MacOS`. In order to run a sample project via `.NET CLI` follow guide:

- In the directory where project is installed, go to **Backtrace.Core** sample application: 
``` 
    cd Backtrace.Core  
``` 
- Sample application store credentials to `Backtrace API` in `App.config`. Update the following `Backtrace I/O credentials` entries:
```xml
    <BacktraceCredentials>  
        <add key="HostUrl" value="...backtrace host URL (with port)..."/>  
        <add key="Token" value="...your backtrace submission token"/>  
    </BacktraceCredentials>  
```
- Build the project:  
```
    dotnet build  
``` 
- Upon successful build, run the project  
``` 
    dotnet run  
``` 
- You should see new errors in your Backtrace I/O dashboard.



## Visual Studio for MacOS  <a name="sample-app-vs-mac"></a>

- Open the **Backtrace** solution in Visual Studio, unload all projects excepts **Backtrace**, **Backtrace.Tests** and **Backtrace.Core**, and set **Backtrace.Core** as your startup project:

![VisualStudioMacOS](https://github.com/backtrace-labs/backtrace-csharp/raw/dev/Backtrace/Documents/Images/VisualStudioMacOS.PNG)

- In project **Backtrace.Core**, edit **App.config** and update the following Backtrace I/O credential entries:  
```
    <BacktraceCredentials>  
        <add key="HostUrl" value="...backtrace host URL (with port)..."/>  
        <add key="Token" value="...your backtrace submission token"/>  
    </BacktraceCredentials>  
```
- Build the project. 
- Upon successful build, run the project.
- You should see new errors in your Backtrace I/O dashboard.


## Visual Studio <a name="sample-app-vs"></a>
Visual Studio allows you to build project and run all available samples (prepared for .NET Core, .NET Framework 4.5, .NET Framework 3.5). 
- Double click `.sln` file or navigate to project directory via `Open` dialog in Visual Studio.
- In `Solution Explorer` navigate to directory `Sample` and set preffered project (.NET Core/Framework) as startup project.

![Visual Studio](https://github.com/backtrace-labs/backtrace-csharp/raw/dev/Backtrace/Documents/Images/VisualStudio.PNG)

- Sample application store credentials to `Backtrace API` in `App.config`. Update the following `Backtrace I/O credentials` entries:
```xml
    <BacktraceCredentials>  
        <add key="HostUrl" value="...backtrace host URL (with port)..."/>  
        <add key="Token" value="...your backtrace submission token"/>  
    </BacktraceCredentials>  
```
- press `Ctrl+Shift+B` to `build` solution
- Press `F5` to run the project
- You should see new errors in your Backtrace I/O dashboard.


# Documentation  <a name="documentation"></a>

## Initialize new BacktraceClient <a name="documentation-initialization"></a>

You can initialize `BacktraceClient` instance via `BacktraceCredential` or application configuration where credentials are stored.

You can check Backtrace credential section in `App.config` file in `Backtrace.Examples` project. See example below to check `App.config` file:

```xml
<configuration>
  <configSections>
    <section name="BacktraceCredentials" type="System.Configuration.NameValueSectionHandler"/>
  </configSections>

  <BacktraceCredentials>
    <add key="HostUrl" value="Your host Url"/>
    <add key="Token" value="Your access token"/>
  </BacktraceCredentials>

  ....
</configuration>
```

In C# code you can initialize new BacktraceClient with section :

```csharp
 var backtraceClient = new BacktraceClient("BacktraceCredentials");
 ```

 If you don't pass a section name to `BacktraceClient`, `BacktraceCredentials` section  will be used.

You can use `BacktraceCredential` class to create new instance of `BacktraceClient`. 

```csharp
var credentials = new BacktraceCredentials("backtraceHostUrl", "accessToken");
var backtraceClient = new BacktraceClient(credentials);
```

Additionally `BacktraceClient` constructor accepts report attributes, database directory path and maximum number of report send per minute. These arguments are optional.

```csharp
var backtraceClient = new BacktraceClient(
    sectionName: "BacktraceCredentials",
    attributes: new Dictionary<string, object>() { { "Attribute", "attribute" } },
    databaseDirectory: "pathToDatabaseDirectory",
    reportPerMin: 0
);
```

If parameter `reportPerMin` is equal to 0, there is no limit for number of reports sending per minute. If you want to send more reports than `reportPerMin` value, `Send` method will return false.

`DatabaseDirectory` parameter is optional. Make sure that there are no files in directory passed in `databaseDirectory`. BacktraceClient will use this directory to save additional information about executed program.

## Sending a report <a name="documentation-sending-report"></a>

To send a new report to Backtrace API you have to use instance of `BacktraceClient`. Use `Send` method to send a new report. You can use overrided versions of `Send` method. See examples below to learn how to send a new `BacktraceReport` to server:

### Use BacktraceReport

You can send a report to server by using `BacktraceReport` class. `BacktraceReport` override a generic class `BacktraceReportBase`. `T` argument is used to determinate type of values in attributes dictionary. 
```csharp
try
{
  //throw exception here
}
catch (Exception exception)
{
  var backtraceReport = new BacktraceReport(
        exception: exception
  );
  backtraceClient.Send(backtraceReport);
}
```

### Use overrided methods
`BacktraceClient` can create report for you. You can use overrided `Send` methods and pass string messsage or received exception. 

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

Additionally `BacktraceReport` constructor accepts report attributes and attachment paths. These arguments are optional.
```csharp
 var report = new BacktraceReport(
    exception: exception,
    attributes: new Dictionary<string, string>() { { "AttributeString", "string" } },
    attachmentPaths: new List<string>() { "path to file attachment", "another path" }
);
```
If you add path to files stored on hard drive, send report will attach all files from variable attachmentPaths.


## Events  <a name="documentation-events"></a>

`BacktraceClient` allows you to add your custom events. You can add methods that will trigger event before `Send` method, after `Send` method or to check report information before sending.
 
```csharp
 //Add your own handler to client API
backtraceClient.BeforeSend =
    (Model.BacktraceData<object> model) =>
    {
        var data = model;
        //do something with data
        return data;
    };
```           
`BacktraceClient` allows you to add custom events that will trigger after server response. You can add new events when server is temporary unvailable `WhenServerUnvailable` or when you receive response from server `OnServerResponse`.

## Customization <a name="documentation-customization"></a>

`BacktraceClient` and `BacktraceReport` allows you to use your own generic attributes and your own implementation. You can override `BacktraceReportBase` and `BacktraceBase` to create your own client/report implementation. 

# Architecture

## BacktraceReport
**`BacktraceReport`** - Class that describe single instance of prepared user report. As user you can pass to `Send` method `Exception` or custom message. `BacktraceReport` use base class `BacktraceReportBase`. `BacktraceReportbase` allow you to add your custom logic to any report. You can override `BacktraceReportBase` and create your custom `BacktraceReport`. Keep in mind - `BacktraceClient` use `CallingAssembly` information to send valid data about your application. `BacktraceReportbase` is a generic class. `T` argument is a value type in `Attribute` dictionary.

## BacktraceClient
**`BacktraceClient`** - class that allow you to create single client instance to prepare integration with `Backtrace API`. As user you want to use class to prepare connection with `BacktraceApi`, set client behaviour (saving minidump files on your local hard drive or set rate limiting). `BacktraceClient` use base class `BacktraceBase`. You can use `BacktraceBase` to prepare you custom logic. `BacktraceBase`is a generic class. `T` argument is a value type in `Attribute` dictionary.

## BacktraceData
**`BacktraceData`** - serializable class that store all values necessary to create diagnostic `JSON` send to `Backtrace API`. `BacktraceData` is a generic class. You can check prepared data and change it via `BeforeSend` event. `BacktraceData` require `BacktraceReport` and `BacktraceClient` scoped attributes.

## BacktraceApi
**`BacktraceApi`** - class that allows you to send diagnostic JSON to `Backtrace API`. `BacktraceApi` is initialized on `BacktraceClient` start. You can prepare custom events for `BacktraceApi`:
- `RequestHandler` - allows you to override `Send` method. Event triggers with 3 parameters - uri, header and formdata bytes. If RequestHandler is available, Send method wont be triggered.
- `WhenServerUnvailable` -   Set an event executed when received bad request, unauthorize request or other information from server.
- `OnServerAnswer` - Set an event executed when server return information after sending data to API.

BacktraceAPI allows you to send synchronous and asynchronous reports to server. To prepare asynchronous report (default is synchronous) you have to change `AsynchronousRequest` property to true.

## BacktraceDatabase
** `BacktraceDatabase` - class that allows you to store data in hard drive. `BacktraceDatabase` instance is initialized on `BacktraceClient` start. If `database path` isn't available on `BacktraceClient` constructor, `BacktraceDatabase` won't generate mini dump files. Before start - Make sure that **Backtrace database directory** is **empty**. 

## ReportWatcher
**`ReportWatcher` - class that validate all send request to `Backtrace API`. If rate limiting variable is set on `BacktraceClient` constructor, `Watcher` limit all request to a server per minute.



# Good to know <a name="good-to-know"></a>

## Xamarin

You can use library with Xamarin if you change `HttpClient Implementation` to Android. To change settings navigate to `Android Options` in `Project Settings` and use `Advance` button. 

![Xamarin Android Support][androidSupport]

[androidSupport]: https://github.com/backtrace-labs/backtrace-csharp/raw/dev/Backtrace/Documents/Images/AndroidSupport.PNG "Xamarin Android Support"