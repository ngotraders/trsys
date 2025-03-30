namespace Trsys.Models.Configurations;

public class EmailConfiguration
{
    public SmtpEmailConfiguration Smtp { get; set; } = new();
    public MicrosoftGraphEmailConfiguration Graph { get; set; } = new();
    public string MailFrom { get; set; }
}
