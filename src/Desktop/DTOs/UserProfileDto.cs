namespace Desktop.DTOs;

public record class UserProfileDto(string Id)
{
    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }
}