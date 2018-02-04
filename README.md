# backtrace-dotnet

[Backtrace](http://backtrace.io/) error reporting tool for C#.

## Usage

```csharp
var backtraceClient = new BacktraceClient(credentials);
try{
	int i = 0;
	var result = 1/i;
}
catch(Exception exception){
	backtraceClient.Send(new BacktraceReport(exception));
}
```
