namespace Desktop.DTOs;

public record class UserProfileDto(string Id)
{
    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public event EventHandler? UserProfileUpdated;

    public void StateHasChanged()
    {
        UserProfileUpdated?.Invoke(this, EventArgs.Empty);
    }
}