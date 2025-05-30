namespace RP5.API.Models;

public class InfluxDbOptions
{
    public string? Host { get; set; }
    public string? Token { get; set; }
    public string? OrgId { get; set; }
    public string? Bucket { get; set; }
}