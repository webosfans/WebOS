using System.Text.Json.Serialization;

namespace News.DTOs;

public class NewsDto
{
    [JsonPropertyName("feedId")]
    public string? PublisherId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("publishTime")]
    public DateTimeOffset PublishTime;

    [JsonPropertyName("summary")]
    public string? Content { get; set; }

    [JsonPropertyName("feed")]
    public PublisherDto? Publisher { get; set; }
}