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

