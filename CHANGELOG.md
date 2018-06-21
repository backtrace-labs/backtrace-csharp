# Backtrace C# Release Notes

## Version 1.2.3 - 21.06.2018
- Enum is now available as a primitive value in BacktraceAttributes

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