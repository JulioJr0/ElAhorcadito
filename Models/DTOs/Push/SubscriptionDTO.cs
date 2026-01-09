namespace ElAhorcadito.Models.DTOs.Push
{
    public class SubscriptionDTO
    {
        public string Endpoint { get; set; } = null!;
        public SubscriptionKeys Keys { get; set; } = null!;
    }

    public class SubscriptionKeys
    {
        public string P256dh { get; set; } = null!;
        public string Auth { get; set; } = null!;
    }
}
