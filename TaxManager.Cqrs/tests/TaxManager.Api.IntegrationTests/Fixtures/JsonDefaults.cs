using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaxManager.Api.IntegrationTests.Fixtures;

/// <summary>Matches the API's own JSON options (enums as names, e.g. "Yearly") for request/response (de)serialization in tests.</summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
