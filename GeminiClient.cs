using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GeminiApiChat;

public sealed class GeminiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> SendChatAsync(string apiKey, string model, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("請先填入 API Key");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("請先選擇 Model");
        }

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        var request = new GeminiRequest
        {
            Contents = history.Select(message => new Content
            {
                Role = message.Role,
                Parts = [new Part { Text = message.Text }]
            }).ToList()
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini API 錯誤 ({(int)response.StatusCode}): {payload}");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Gemini API 回傳資料格式異常。");

        var text = geminiResponse.Candidates?
            .FirstOrDefault()?
            .Content?
            .Parts?
            .FirstOrDefault()?
            .Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini API 未回傳可用的文字內容。");
        }

        return text;
    }
}

public sealed class ChatMessage
{
    public required string Role { get; init; }
    public required string Text { get; init; }
}

public sealed class GeminiRequest
{
    public List<Content> Contents { get; set; } = [];
}

public sealed class Content
{
    public string Role { get; set; } = "user";
    public List<Part> Parts { get; set; } = [];
}

public sealed class Part
{
    public string Text { get; set; } = string.Empty;
}

public sealed class GeminiResponse
{
    public List<Candidate>? Candidates { get; set; }
}

public sealed class Candidate
{
    public Content? Content { get; set; }
}
