namespace WebOS.DTOs;

public record class AppDto(string DisplayName, string Icon, string RouteAddress)
{
    public int DefaultWidth { get; set; }
    public int DefaultHeight { get; set; }
}