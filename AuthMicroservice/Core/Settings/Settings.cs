using AuthMicroservice.Core.Interfaces;

namespace AuthMicroservice.Core.Settings;

public class Settings : ISettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TokenSecret { get; set; } = string.Empty;
    public int TokenExpirationInMinutes { get; set; }
    public string EmailTemplateIdRestore { get; set; } = string.Empty;
    public int RestoreCodeExpirationInMinutes { get; set; }
}
