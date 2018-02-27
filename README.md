# backtrace-dotnet

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

## Documentation

### Initialize new BacktraceClient

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

 If you don't pass a section name to `BacktraceClient` section `BacktraceCredentials section` will be used.

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
    databaseDirectory: "pathToDirectoryPath",
    reportPerMin: 0
);
```

If parameter `reportPerMin` is equal to 0, there is no limit for report send per minute. If you want to send more reports than `reportPerMin` value, `Send` method will return false.

`DatabaseDirectory` parameter is optional. Make sure that there are no files in directory passed in `databaseDirectory`. BacktraceClient will use this directory to save additional information about executed program.

### Sending a report

To send a new report to Backtrace API you have to use instance of `BacktraceClient`. Use `Send` method to send a new report. You can use overrided versions of `Send` method. See examples below to learn how to send a new `BacktraceReport` to server:

Use BacktraceReport

You can send a report to server by using `BacktraceReport` class. `BacktraceReport` is override a generic class `BacktraceReportBase`. `T` argument is used to determinate type of values in attributes dictionary. 
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
 var report = new BacktraceReport<string>(
    exception: exception,
    attributes: new Dictionary<string, string>() { { "AttributeString", "string" } },
    attachmentPaths: new List<string>() { "path to file attachment", "another path" }
);
```
If you add path to files stored on hard drive, send report will attach all files from variable attachmentPaths.


### Events

`BacktraceClient` allows you to add your custom events. You can add methods that will trigger event before `Send` method, after `Send` method or to check report information before sending.
 
```csharp
 //Add your own handler to client API
backtraceClient.BeforeSend =
    (Model.BacktraceData<object> model) =>
    {
        var data = model;
    };
```           