namespace Receiver.Entities;

public class SubscribeEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public DateTime SubscriptionDate { get; set; }
}
