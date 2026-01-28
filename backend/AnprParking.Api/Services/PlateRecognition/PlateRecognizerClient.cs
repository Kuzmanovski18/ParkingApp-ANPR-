using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AnprParking.Api.Services.PlateRecognition;

public class PlateRecognizerClient : IPlateRecognizer
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public PlateRecognizerClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public async Task<string> RecognizePlateAsync(IFormFile image, CancellationToken ct)
    {
        var apiKey = _cfg["PlateRecognizer:ApiKey"];
        var endpoint = _cfg["PlateRecognizer:Endpoint"] ?? "https://api.platerecognizer.com/v1/plate-reader/";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing PlateRecognizer:ApiKey in config.");

        if (image is null || image.Length == 0)
            throw new ArgumentException("No image provided.", nameof(image));

        // multipart/form-data
        using var form = new MultipartFormDataContent();

        await using var stream = image.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(image.ContentType) ? "image/jpeg" : image.ContentType
        );

        // IMPORTANT: Plate Recognizer expects the file field name to be "upload"
        form.Add(fileContent, "upload", image.FileName);

        // Optional params (можеш да ги избришеш ако не ти требаат):
        // - regions: помогнува за точност (нпр "mk", "bg", "rs", "gr")
        // form.Add(new StringContent("mk"), "regions");

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = form
        };

        // Auth header: Authorization: Token <apiKey>
        req.Headers.Authorization = new AuthenticationHeaderValue("Token", apiKey);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            // Да имаш корисна порака во лог/502
            throw new HttpRequestException($"{(int)res.StatusCode} {res.ReasonPhrase} {body}");
        }

        // Parse JSON:
        // Expecting something like:
        // { "results": [ { "plate": "sk1234ab", ... } ], ... }
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("results", out var results) &&
            results.ValueKind == JsonValueKind.Array &&
            results.GetArrayLength() > 0)
        {
            var first = results[0];
            if (first.TryGetProperty("plate", out var plateProp))
                return plateProp.GetString() ?? "";
        }

        // ако нема резултати – врати празно
        return "";
    }
}
