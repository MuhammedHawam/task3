namespace PartnersHub.Shared.Integration.Options;

public sealed class MiddlewareApiOptions
{
    public const string SectionName = "MiddlewareApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
