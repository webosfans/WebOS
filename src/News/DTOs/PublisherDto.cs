namespace News.DTOs;

public record class PublisherDto(string PublisherId, string PublisherName)
{
    public string? Icon { get; set; }
    public string? Introduction { get; set; }
}