namespace AuthMicroservice.Core.Interfaces;

public interface ISettings
{
    string ConnectionString { get; }
    string TokenSecret { get; }
    int TokenExpirationInMinutes { get; }
    string EmailTemplateIdRestore { get; }
}
