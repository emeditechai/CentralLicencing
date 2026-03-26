namespace CentralLicenceApp.Models
{
    public class PushNotificationSettings
    {
        public string Subject { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Subject)
            && !string.IsNullOrWhiteSpace(PublicKey)
            && !string.IsNullOrWhiteSpace(PrivateKey);
    }
}