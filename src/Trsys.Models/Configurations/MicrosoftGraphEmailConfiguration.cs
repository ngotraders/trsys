namespace Trsys.Models.Configurations;

public class MicrosoftGraphEmailConfiguration
{
    public string ClientId { get; set; }
    public string TenantId { get; set; }
    public string ClientSecret { get; set; }
    public bool IsValid => !string.IsNullOrEmpty(TenantId) && !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret);
}
