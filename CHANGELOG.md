# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [0.1.0] - Unreleased

Initial release. Hand-written port of `xingen-java-sdk`, adapted to .NET idioms (async/`Task`
throughout, `CancellationToken` instead of a cancellation-check callback, `decimal`/`DateOnly`
instead of `BigDecimal`/`LocalDate`, records with `init` properties instead of builders).

- `XingenClient` — entry point; holds one connection-pooled `HttpClient`.
- `InvoicesClient` — submit (JSON/file/IDoc/OData), get, list, list-all (`IAsyncEnumerable`),
  download (PDF/IDoc XML), and `*AndWaitAsync` polling helpers.
- `ApiKeysClient` — create, list, revoke.
- Typed exception hierarchy (`XingenException` → `ApiException` → status-specific subtypes).
