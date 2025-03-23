using System.Text.Json.Serialization;

namespace News.DTOs;

public record class PublisherDto(string PublisherId, string PublisherName)
{
    [JsonPropertyName("subscriptionName")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("iconUri")]
    public string? Icon { get; set; }

    public string? Introduction { get; set; }
}