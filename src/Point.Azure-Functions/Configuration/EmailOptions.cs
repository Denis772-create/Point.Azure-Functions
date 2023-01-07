namespace Point.Azure_Functions.Configuration;

public class EmailOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public bool HostUsesLocalCertificate { get; set; }
}