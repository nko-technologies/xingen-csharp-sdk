using System.Text;
using Xingen.Sdk.Http;
using Xingen.Sdk.Invoices;
using Xingen.Sdk.Models;
using Xunit;

namespace Xingen.Sdk.Tests.Invoices;

public class InvoiceRecordDeserializationTests
{
    private readonly JsonCodec _codec = new();

    [Fact]
    public async Task DecodesValidatedInvoiceWithNestedInvoiceAndValidationResult()
    {
        var record = await Decode("invoice-record.json");

        Assert.Equal("inv_01HXYZ", record.Id);
        Assert.Equal(InvoiceStatus.Validated, record.Status);
        Assert.True(record.Sandbox);

        var invoice = record.Invoice!;
        Assert.Equal("INV-2024-0042", invoice.InvoiceNumber);
        Assert.Equal(new DateOnly(2024, 3, 15), invoice.IssueDate);
        Assert.Equal(1184.05m, invoice.PayableAmount);
        Assert.Equal("DE123456789", invoice.Supplier!.VatId);
        Assert.Equal("Berlin", invoice.Supplier.Address!.City);
        Assert.Single(invoice.Lines);
        Assert.Equal("Software License Q1", invoice.Lines[0].ItemName);

        Assert.True(record.ValidationResult!.Valid);
        Assert.Empty(record.ValidationResult.Errors);
    }

    [Fact]
    public async Task DecodesProcessingInvoiceWithNullInvoiceAndValidationResult()
    {
        var record = await Decode("invoice-record-processing.json");

        Assert.Equal(InvoiceStatus.Processing, record.Status);
        Assert.False(record.Status.IsTerminal());
        Assert.Null(record.Invoice);
        Assert.Null(record.ValidationResult);
    }

    [Fact]
    public async Task DecodesFailedValidationWithErrorDetails()
    {
        var record = await Decode("invoice-record-failed.json");

        Assert.Equal(InvoiceStatus.FailedValidation, record.Status);
        Assert.True(record.Status.IsTerminal());
        Assert.False(record.ValidationResult!.Valid);

        var error = record.ValidationResult.Errors[0];
        Assert.Equal("BR-01", error.Code);
        Assert.Equal(ValidationLayer.CORE, error.Layer);
        Assert.Equal(Severity.ERROR, error.Severity);
    }

    [Fact]
    public void DecodesExtractionTierForAiPdfExtractionsAndLeavesItNullOtherwise()
    {
        const string aiExtracted = "{\"id\":\"inv_ai\",\"status\":\"validated\",\"createdAt\":\"2026-07-08T09:30:00Z\","
            + "\"validationProfile\":\"EN16931\",\"invoiceFormat\":\"PDF_AI\",\"sandbox\":false,"
            + "\"extractionTier\":\"ACCURATE\",\"canonicalJson\":null,\"validationResult\":null}";

        var record = _codec.Decode<InvoiceRecord>(Encoding.UTF8.GetBytes(aiExtracted));

        Assert.Equal("PDF_AI", record.InvoiceFormat);
        Assert.Equal("ACCURATE", record.ExtractionTier);

        var nonAiRecord = _codec.Decode<InvoiceRecord>(Encoding.UTF8.GetBytes(
            aiExtracted.Replace("\"extractionTier\":\"ACCURATE\",", "")));
        Assert.Null(nonAiRecord.ExtractionTier);
    }

    private async Task<InvoiceRecord> Decode(string fixtureName)
    {
        var bytes = await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName));
        return _codec.Decode<InvoiceRecord>(bytes);
    }
}
