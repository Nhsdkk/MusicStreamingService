using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record SubscriberEntity : IAuditable
{
    /// <summary>
    /// Subscriber's id
    /// </summary>
    public Guid SubscriberId { get; set; }

    /// <summary>
    /// Subscriber
    /// </summary>
    public UserEntity Subscriber { get; set; } = null!;
    
    /// <summary>
    /// Id of subscription, that user picked
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Subscription, that user picked
    /// </summary>
    public SubscriptionEntity Subscription { get; set; } = null!;
    
    /// <summary>
    /// Timestamp, when subscription started to be valid
    /// </summary>
    public DateTime ValidFrom { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Timestamp, when subscription ended
    /// </summary>
    public DateTime ValidTo { get; set; }
}
