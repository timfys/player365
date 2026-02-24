namespace SmartWinners.Configuration;

public class SmtpClientConfiguration : MyConfiguration
{
    public string SmtpHost { get; set; }
    
    public int SmtpPort { get; set; }
    
    public int SmtpSslPort { get; set; }
    
    public bool SmtpUseSsl {get; set; }

    public string SmtpLogin { get; set; }

    public string SmtpPassword { get; set; }

    public string Subject { get; set; }
    
    public string FromName { get; set; }
    
    public string FromAddress { get; set; }
    
    public string ToName { get; set; }
    
    public string ToAddress { get; set; }
    public string ToAddressTest { get; set; }
}