using System.Text.Json.Serialization;

namespace TaxManager.Domain.Enums;

/// <summary>
/// Determines a tax record's priority when several records' date ranges cover the same date:
/// the more specific period wins (Daily beats Weekly beats Monthly beats Yearly). See
/// <see cref="TaxManager.Domain.Services.TaxRateResolver"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaxPeriodType
{
    Yearly = 1,
    Monthly = 2,
    Weekly = 3,
    Daily = 4
}
