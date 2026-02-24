using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public interface ITranslator
{
  Task<string> TranslateAsync(string text, string sourceIso2, string targetIso2, CancellationToken ct);
}

public sealed class OpenAiGpt52Translator(HttpClient http, IConfiguration cfg) : ITranslator
{
  private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
  private readonly string _apiKey = cfg["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("OpenAI API key not configured (OpenAI:ApiKey or OPENAI_API_KEY).");

  public async Task<string> TranslateAsync(string text, string sourceIso2, string targetIso2, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(text))
      return text;

    // Strong constraints: preserve placeholders & HTML, return ONLY translated text.
    var prompt =
        $"Translate from {sourceIso2} to {targetIso2}.\n" +
        $"- Preserve placeholders exactly (e.g., {{0}}, {{1}}, {{name}}, {{count}}).\n" +
        $"- Preserve HTML tags/entities exactly.\n" +
        $"- Do not add quotes or explanations. Return ONLY the translated text.\n\n" +
        text;

    using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

    // Minimal Responses API payload
    var payload = new
    {
      model = "gpt-5.2",
      input = prompt
    };

    req.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

    using var resp = await http.SendAsync(req, ct);
    resp.EnsureSuccessStatusCode();

    var json = await resp.Content.ReadAsStringAsync(ct);
    var parsed = JsonSerializer.Deserialize<ResponsesCreateResponse>(json, JsonOpts)
        ?? throw new InvalidOperationException("Failed to parse OpenAI response.");

    // Responses API provides convenience: output_text (commonly present) OR structured output array.
    // We handle both in a simple way.
    var outText = parsed.output_text;
    if (!string.IsNullOrWhiteSpace(outText))
      return outText.Trim();

    // Fallback: try to read first output content text
    var fallback = parsed.output?.FirstOrDefault()?.content?.FirstOrDefault()?.text;
    return (fallback ?? text).Trim();
  }

  private sealed class ResponsesCreateResponse
  {
    public string? output_text { get; set; }
    public OutputItem[]? output { get; set; }
  }

  private sealed class OutputItem
  {
    public ContentItem[]? content { get; set; }
  }

  private sealed class ContentItem
  {
    public string? type { get; set; }
    public string? text { get; set; }
  }
}
