# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [0.2.0] - Unreleased

- AI-based PDF invoice extraction: `ExtractInvoiceAsync`/`ExtractInvoiceAndWaitAsync`
  (`POST /v1/invoices/extract`), new `ExtractionModelTier` enum (`FAST`/`ACCURATE`), and
  `InvoiceRecord.ExtractionTier`.
- `PatchInvoiceAsync` — correct invoice fields via JSON merge-patch and re-validate synchronously
  (`PATCH /v1/invoices/{id}`); accepts either a raw JSON string or a `Dictionary<string, object?>`.
- `GetAutoFilledFieldsAsync` — list the invoice fields the backend fills in automatically, per
  validation profile (`GET /v1/invoices/auto-filled-fields`), via the new `AutoFilledField` record.
- `InvoiceSubmission.PartyInput` gained an `Address` property (new `InvoiceSubmission.AddressInput`
  record: `StreetName`/`City`/`PostalZone`/`CountryCode`) — the backend now rejects a party with no
  postal address on every profile, and `SubmitAsync` had no way to supply one.
- `SubmitAsync` reaches full parity with the backend's domain model, so every invoice type it can
  validate can now also be submitted as structured JSON. New on `InvoiceSubmission`: `DueDate`,
  `TaxPointDate`, `TaxCurrencyCode`, `PaymentTermsNote`, the BT-11..BT-19 reference properties,
  `Notes`, `PrecedingInvoiceReferences`, `SupportingDocuments`, `DeliveryPeriodStart`/`End`,
  `InvoicePeriod`, `Delivery`, `Payee`, `TaxRepresentative`, `PaymentMeans`, and
  `AllowanceCharges`. New on `PartyInput`: `RegistrationName`, `TaxRegistrationId`,
  `LegalRegistrationId`/`SchemeId`, `AdditionalLegalInfo`, `Contact` (new `ContactInput`),
  `Identifiers` (new `PartyIdentifierInput`). New on `AddressInput`: `AdditionalStreetName`,
  `AddressLine3`, `CountrySubdivision`. New on `LineInput`: `ItemName`, `Note`, object/order/
  accounting references, seller/buyer/standard item ids, `OriginCountryCode`, `Classifications`
  (new `ItemClassificationInput`), `Attributes` (new `ItemAttributeInput`), `GrossPrice`,
  `PriceDiscount`, `PriceBaseQuantity`/`Unit`, `TaxCategoryCode`, `ExemptionReason`/`Code`,
  `Period` (new `InvoicePeriodInput`), and `AllowanceCharges` (new
  `LineAllowanceChargeInput`). New top-level `PaymentMeansInput` and `AllowanceChargeInput`
  records.

## [0.1.0]

Initial release. Hand-written port of `xingen-java-sdk`, adapted to .NET idioms (async/`Task`
throughout, `CancellationToken` instead of a cancellation-check callback, `decimal`/`DateOnly`
instead of `BigDecimal`/`LocalDate`, records with `init` properties instead of builders).

- `XingenClient` — entry point; holds one connection-pooled `HttpClient`.
- `InvoicesClient` — submit (JSON/file/IDoc/OData), get, list, list-all (`IAsyncEnumerable`),
  download (PDF/IDoc XML), and `*AndWaitAsync` polling helpers.
- `ApiKeysClient` — create, list, revoke.
- Typed exception hierarchy (`XingenException` → `ApiException` → status-specific subtypes).
