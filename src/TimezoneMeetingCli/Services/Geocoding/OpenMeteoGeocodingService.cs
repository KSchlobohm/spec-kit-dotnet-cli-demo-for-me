using System.Text.Json;
using System.Text.Json.Serialization;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.Services.Geocoding;

public class OpenMeteoGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public OpenMeteoGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeocodedLocation?> ResolveLocationAsync(LocationQuery query, CancellationToken cancellationToken = default)
    {
        // For TimezoneId directly, skip geocoding.
        if (query.QueryType == QueryType.TimezoneId)
        {
            return new GeocodedLocation(query.RawInput, 0, 0, query.RawInput);
        }

        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query.RawInput)}&count=1&language=en&format=json";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<OpenMeteoResponse>(content);

            var firstHit = result?.Results?.FirstOrDefault();
            if (firstHit == null)
            {
                return null;
            }

            return new GeocodedLocation(
                string.Join(", ", new[] { firstHit.Name, firstHit.Admin1, firstHit.Country }.Where(s => !string.IsNullOrWhiteSpace(s))),
                firstHit.Latitude,
                firstHit.Longitude,
                firstHit.Timezone ?? "UTC"
            );
        }
        catch (HttpRequestException)
        {
            // Fails gracefully
            return null;
        }
    }

    private class OpenMeteoResponse
    {
        [JsonPropertyName("results")]
        public List<OpenMeteoResult>? Results { get; set; }
    }

    private class OpenMeteoResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("admin1")]
        public string? Admin1 { get; set; }
        
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    }
}