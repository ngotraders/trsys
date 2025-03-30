namespace Trsys.Models.Configurations;

public class SmtpEmailConfiguration
{
    public string Host { get; set; }
    public int? Port { get; set; }
    public bool? UseSsl { get; set; }
    public string AuthenticationUser { get; set; }
    public string AuthenticationPassword { get; set; }

    public bool IsValid => !string.IsNullOrEmpty(Host)
        && ((string.IsNullOrEmpty(AuthenticationUser) && string.IsNullOrEmpty(AuthenticationPassword))
        || (!string.IsNullOrEmpty(AuthenticationUser) && !string.IsNullOrEmpty(AuthenticationPassword)));
}