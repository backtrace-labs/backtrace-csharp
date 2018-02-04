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

For more advanced usage, see `BacktraceReport` API below.

## Documentation

### bt.initialize([options])

This is intended to be one of the first things your application does during
initialization. It registers a handler for `uncaughtException` which will
spawn a detached child process to perform the error report and then crash
in the same way that your application would have crashed without the handler.

#### Options

##### `endpoint`

Required.

Example: `https://backtrace.example.com:1234`.

Sets the HTTP/HTTPS endpoint that error reports will be sent to.

##### `token`

Required.

Example: `51cc8e69c5b62fa8c72dc963e730f1e8eacbd243aeafc35d08d05ded9a024121`.

Sets the token that will be used for authentication when sending an error
report.

##### `handlePromises`

Optional. Set to `true` to listen to the `unhandledRejection` global event and
report those errors in addition to `uncaughtException` events.

Defaults to `false` because an application can technically add a promise
rejection handler after an event loop iteration, which would cause the
`unhandledRejection` event to fire, followed by the `rejectionHandled` event
when the handler was added later. This would make the error report a false
positive. However, most applications will add rejection handlers before an
event loop iteration, in which case `handlePromises` should be set to `true`.

##### `attributes`

Optional. Object that contains additional attributes to be sent along with
every error report. These can be overridden on an individual report with
`report.addAttribute`.

Example:

```
{
  application: "ApplicationName",
  serverId: "foo",
}
```

##### `timeout`

Defaults to `1000`. Maximum amount of milliseconds to wait for child process
to process error report and schedule sending the report to Backtrace.

##### `debugBacktrace`

Defaults to `false`. Set to `true` to cause process to wait for the report to
Backtrace to complete before exiting.

##### `allowMultipleUncaughtExceptionListeners`

Defaults to `false`. Set to `true` to not crash when another `uncaughtException`
listener is detected.

##### `disableGlobalHandler`

Defaults to `false`. If this is `false`, this module will attach an
`uncaughtException` handler and report those errors automatically before
re-throwing the exception.

Set to `true` to disable this. Note that in this case the only way errors
will be reported is if you call `bt.report(error)`.

##### `contextLineCount`

Defaults to `200`. When an error is reported, this many lines above and below
each stack function are included in the report.

##### `tabWidth`

Defaults to `8`. If there are any hard tabs in the source code, it is unclear
how many spaces they should be indented to correctly display the source code.
Therefore the error report can override this number to specify how many spaces
a hard tab should be represented by when viewing source code.

### bt.report([error], [attributes], [callback])

Sends an error report to the endpoint specified in `initialize`.

 * `error` - optional. An `Error` object created with `new Error("message")`.
   If this parameter is not an instance of `Error` then backtrace-node will
   print a warning message to stderr.
 * `attributes` - optional. An object which contains key-value pairs to add
   to the report.
 * `callback(err)` - optional. Called when the report is finished sending.

Calling this function is the same as doing:

```js
var report = createReport();
report.setError(error);
report.addObjectAttributes(attributes);
report.send(callback);
```

Though the callstack generation is synchronous, note that actually sending
the report is an asynchronous process. If you wish to synchronously generate
and send the error report, use `reportSync` as specified below. Note that
unlike `report`, the `reportSync` function is also safe to use for
`uncaughtException` handlers.

### bt.reportSync([error], [attributes])

Same as `bt.report`, but blocks until finished.

Calling this function is the same as doing:

```js
var report = createReport();
report.setError(error);
report.addObjectAttributes(attributes);
report.sendSync();
```

### bt.errorHandlerMiddleware(err, req, res, next)

This is a connect/express middleware function that will automatically send
error reports. Use it like this:

```js
app.use(bt.errorHandlerMiddleware);
```

This middleware is read-only; it kicks off an error report and then passes the
error down the middleware chain.

For more details see
[Express error handling](https://expressjs.com/en/guide/error-handling.html)


### bt.createReport()

Create a report object that you can later choose whether or not to send.

This may be useful to track something like a request.

Returns a `BacktraceReport`.

### bt.BacktraceReport

Create a `BacktraceReport` object with `bt.createReport`.

Example:

```js
http.createServer(function(request, response) {
  var report = createReport();
  report.addObjectAttributes(request);

  // ...later...
  report.setError(new Error("something broke"));
  report.send();
});
```

#### report.addAttribute(key, value)

Adds an attribute to a specific report. Valid types for `value` are
`string`, `number`, and `boolean`.

Attributes are indexed and searchable. See also `addAnnotation`.

#### report.addObjectAttributes(object, [options])

Adds all key-value pairs of `object` into the report recursively. For example:

```js
http.createServer(function(request, response) {
    report.addObjectAttributes(request);
});
```

In this example, the list of attributes added is:

```
readable = true
socket.readable = true
socket.writable = true
socket.allowHalfOpen = true
socket.destroyed = false
socket.bytesRead = 0
server.allowHalfOpen = true
server.pauseOnConnect = false
server.httpAllowHalfOpen = false
server.timeout = 120000
parser.maxHeaderPairs = 2000
socket.remoteAddress = "::ffff:127.0.0.1"
socket.remoteFamily = "IPv6"
socket.remotePort = 32958
socket.localAddress = "::ffff:127.0.0.1"
socket.localPort = 12345
socket.bytesWritten = 0
httpVersionMajor = 1
httpVersionMinor = 1
httpVersion = "1.1"
complete = false
headers.host = "localhost:12345"
headers.user-agent = "curl/7.47.0"
headers.accept = "*/*"
upgrade = false
url = "/"
method = "GET"
```

Available options:

 * `allowPrivateProps` Boolean. By default, keys that start with an underscore
   are ignored. If you pass `true` for `allowPrivateProps` then these keys are
   added.
 * `prefix` String. Defaults to `""`. You might consider passing `"foo."` to
   namespace the added attributes with `"foo."`.

#### report.addAnnotation(key, value)

Adds an annotation to a specific report. Annotations, unlike attributes, are
not indexed and searchable. However, they are available for inspection when
you view a specific report.

 * `key` - String which is the name of the annotation.
 * `value` - Any type which is JSON-serializable.

See also `addAttribute`.

#### report.setError(error)

`error` is an Error object. Backtrace will extract information from this object
such as the error message and stack trace and send this information along with
the report.

#### report.trace()

This function captures a stack trace at the current location. Due to the event
loop, errors in Node.js sometimes are missing part of the stack trace.

Call this function before every asynchronous function call, and your stack
trace will be complete.

Note that it is safe to call trace multiple times; if you call trace
redundantly, backtrace-node will recognize that the second trace call supercedes
the first, and only the latter will be included in the report.

trace is automatically called when you call createReport and when you call
setError.

#### report.log(...)

Adds a timestamped log message to the report. Log output is available when you
view a report. The arguments to report.log are the same as the arguments to
`console.log`.

#### report.send([callback])

Sends the error report to the endpoint specified in `initialize`.

 * `callback(err)` - optional. Called when the report is finished sending.

#### report.sendSync()

Same as `report.send`, but blocks until finished.
