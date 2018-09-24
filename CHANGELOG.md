# Backtrace C# Release Notes

## Version 2.0.4 - 23.09.2018
- `BacktraceClient` allows developer to unpack `AggregateException` and send only exceptions available in `InnerExceptions` property.
- `BacktraceReport` accepts two new properties: `Factor` and `Fingerprint`. These properties allows you to change server side algorithms. 
- `BacktraceData` now include informations about `Exception` properties. You can check detailed `Exception` properties in Annotations.
- `BacktraceDatabase` doesn't throw exception when developer can't add new record. This situation exists when database was full or database hasn't enough disk space for exceptions.
- `BacktraceResult` can use new `Status`. In case when developer want to unpack `AggregateException` and `InnerExceptions` property is empty, `BacktraceClient` return `BacktraceResult` with status `Empty`,

## Version 2.0.3 - 04.09.2018
- Thread data condition for Unity on .NET Framework 4.5+

## Version 2.0.2 - 28.08.2018
- Nullable environment variables fix,
- Fix for invalid database tests that use real minidump files,
- Fix nullable GetEnvironmentValue results.

## Version 2.0.1 - 17.07.2018
- `BacktraceClient` use reflection to generate better method names for async state machine stack frames,
- `BacktraceReport` allows to disable reflection feature by using additional constructor parameter,
- `BacktraceClient` use `BacktraceStackTrace` and `BacktraceStackFrame` instead of `DiagnosticStack`.

## Version 2.0.0 - 10.07.2018
- `BacktraceDatabase` use `BacktraceRecord` instead of `BacktraceEntry`,
- `BacktraceDatabase` new parameters - size limit and maximum number of record in database,
- New information about current assembly in `BacktraceReport` attributes,
- New directory for sample `Backtrace` projects,
- `BacktraceReport` don't use anymore `BacktraceReportBase`. All `BacktraceClient` and `BacktraceDatabase` events use `BacktraceReport` instead of `BacktraceReportBase`.

## Version 1.3.2 - 03.07.2018
- Fixed a invalid type for process.age attribute 

## Version 1.3.1 - 28.06.2018
- Fixed a double-dispose bug in BacktraceApi

## Version 1.3 - 26.06.2018
- Attributes dictionary now use an `object` type instead of a generic type for better flexibility,
- `OnClientReportLimitReached` use `BacktraceReportBase` instead of `BacktraceReport`,
- `BacktraceResult` store `BacktraceReportBase` instead of `BacktraceReport`. 

## Version 1.2.3 - 21.06.2018
- Enum is now available as a primitive value in BacktraceAttributes.

## Version 1.2.2 - 19.06.2018
- Ignore exception object in BacktraceReport in serialization. Change exception conditions in BacktraceReport.

## Version 1.2.1 - 14.06.2018
- Invalid serialization support.

## Version 1.2.0 - 09.05.2018
- `BacktraceDatabase` - offline error report storage and auto re-submission support in the event of network outage and server unavailability,
- `BacktraceClient.Send` now works properly with TLS 1.2 under .NET 4.6+ and .NET Core 2.0. However, `BacktraceClient.SendAsync` is strongly recommended whenever possible,
- Removed TlsLegacySupport flag in BacktraceClient.

## Version 1.1.4 - 27.04.2018
- Fix: A bug where casing of some fields is changed after JSON serialization.

## Version 1.1.3 - 13.04.2018
- Improved `async Task` sample applications.

## Version 1.1.2 - 09.04.2018
- Fix: Clean Backtrace client’s data storage on startup.

## Version 1.1.1 - 09.04.2018
- Error reports now include debug attributes,
- `BacktraceClient.OnClientReportLimitReached` event handlers will now take `BacktraceReport` as a parameter,
- Refactoring of JSON Data code,
- .NET 4.5 example with `async task` usage,
- Better stack trace analysis,
- Fix: Reporting unhandled application exceptions now uses `SendAsync` and proper TLS 1.2 support.


## Version 1.1.0 - 30.03.2018
- BacktraceClient now supports an asynchronously `SendAsync` method that works with `async task`,
- For .NET Framework 4.5 and .NET Standard 2.0, `BacktraceClient` now streams file attachment content directly from disk via `SendAsync` method,
- `AfterSend` event parameter changed. Now `AfterSend` event require `BacktraceResult` parameter, not `BacktraceReport`,
- `Send` and `SendAsync` method now returns `BacktraceResult` with information about report state,
- `OnServerResponse` now require `BacktraceResult` as a parameter. 

## Version 1.0.0 - 19.03.2018
- First release.
