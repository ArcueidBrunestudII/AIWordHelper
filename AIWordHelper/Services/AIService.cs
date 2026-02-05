using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AIWordHelper.Services;

public class AIService
{
    private readonly HttpClient _httpClient = new();

    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";

    public async Task<List<string>> GetModelsAsync()
    {
        var models = new List<string>();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl.TrimEnd('/')}/models");
            request.Headers.Add("Authorization", $"Bearer {ApiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var model in data.EnumerateArray())
                {
                    if (model.TryGetProperty("id", out var id))
                    {
                        models.Add(id.GetString() ?? "");
                    }
                }
            }
        }
        catch
        {
            // Return empty list on error
        }

        return models.Where(m => !string.IsNullOrEmpty(m)).OrderBy(m => m).ToList();
    }

    public async IAsyncEnumerable<string> StreamChatAsync(string model, string prompt)
    {
        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            stream = true
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrl.TrimEnd('/')}/chat/completions")
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Bearer {ApiKey}");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line.Substring(6);
            if (data == "[DONE]") break;

            using var doc = JsonDocument.Parse(data);
            if (doc.RootElement.TryGetProperty("choices", out var choices))
            {
                var firstChoice = choices.EnumerateArray().FirstOrDefault();
                if (firstChoice.TryGetProperty("delta", out var delta))
                {
                    if (delta.TryGetProperty("content", out var contentToken))
                    {
                        var text = contentToken.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            yield return text;
                        }
                    }
                }
            }
        }
    }
}
