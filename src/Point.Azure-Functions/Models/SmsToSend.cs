namespace Point.Azure_Functions.Models
{
    public class SmsToSend
    {
        public string Recipient { get; set; }
        public string Content { get; set; }
    }
}
